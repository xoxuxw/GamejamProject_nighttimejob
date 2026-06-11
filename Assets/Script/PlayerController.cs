using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Rigidbody player_rigidbody;
    private Vector2 moveInput;
    private Vector2 mouseLook;

    [Header("카메라 설정")]
    public Transform cameraTransform; // 인스펙터에서 Main Camera 연결
    public float mouseSensitivity = 0.15f;
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

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += OnJump;

        // 마우스 커서 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        // 마우스 델타값 직접 읽기 (Input Action 에러 방지)
        if (Mouse.current != null)
        {
            mouseLook = Mouse.current.delta.ReadValue();
        }

        HandleGroundCheck();
        HandleRotation();
    }

    // [중요] 물리 연산(이동)은 Update가 아니라 FixedUpdate에서 처리해야 물리 고장이 나지 않습니다.
    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleRotation()
    {
        float mouseX = mouseLook.x * mouseSensitivity;
        float mouseY = mouseLook.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        if (cameraTransform != null)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -85f, 85f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    // 물리 기반 이동으로 전면 수정
    void HandleMovement()
    {
        // 입력에 따른 이동 방향 계산
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize(); // 대각선 이동 시 빨라짐 방지

        // Rigidbody를 사용해 벽이나 NPC와 정상적으로 밀쳐내며 이동 (관통 및 강제 밀침 현상 차단)
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