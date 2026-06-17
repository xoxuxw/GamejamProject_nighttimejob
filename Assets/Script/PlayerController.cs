using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Rigidbody player_rigidbody;
    private Vector2 moveInput;

    [Header("카메라 설정")]
    public Transform cameraTransform; // 인스펙터에서 Main Camera 연결
    public float mouseSensitivity = 0.1f; // Mouse.current 방식에 맞는 감도 (0.05 ~ 0.2 추천)
    private float xRotation = 0f;

    [Header("이동 및 점프")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("바닥 감지")]
    public float rayLength = 1.1f;
    public LayerMask groundLayer;
    private bool isGrounded;

    void Awake()
    {
        player_rigidbody = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();

        // 이동 및 점프 이벤트 연결
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += OnJump;

        // 마우스 커서 고정 및 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        HandleGroundCheck();
        HandleRotation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleRotation()
    {
        // [방어적 코드] 인풋 매칭 에러 및 NaN 값으로 인한 추락을 원천 차단하기 위해
        // 현재 활성화된 마우스의 실시간 델타 변화량만 안전하게 수집합니다.
        Vector2 mouseDelta = Vector2.zero;

        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }

        // 값이 비정상적으로 튀거나 계산 오류(NaN)가 나는 것을 방지하는 안전장치
        if (float.IsNaN(mouseDelta.x) || float.IsNaN(mouseDelta.y)) return;

        // 마우스 감도(Sensitivity)를 곱해 회전량 계산
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // 1. 플레이어 몸통 좌우 회전 (Y축 기준 회전)
        transform.Rotate(Vector3.up * mouseX);

        // 2. 카메라 위아래 회전 (X축 기준 회전)
        if (cameraTransform != null)
        {
            xRotation -= mouseY; // 마우스를 위로 올리면 화면이 위를 보도록 마이너스 연산

            // [제한 설정] 고개가 뒤로 뒤집히거나 땅바닥 뚫어보기 방지
            xRotation = Mathf.Clamp(xRotation, -85f, 85f);

            // 카메라의 로컬 회전값(위아래)에만 안전하게 반영
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize();

        Vector3 targetPosition = player_rigidbody.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        player_rigidbody.MovePosition(targetPosition);
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayLength, groundLayer);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            player_rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}