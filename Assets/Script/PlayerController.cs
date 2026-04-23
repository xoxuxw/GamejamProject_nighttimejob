using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Rigidbody player_rigidbody;
    private Vector2 moveInput;
    private Vector2 mouseLook; // 마우스 움직임 값을 담을 변수

    [Header("카메라 설정")]
    public Transform cameraTransform; // 인스펙터에서 Main Camera를 꼭 연결하세요!
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

        // 이동과 점프만 인풋 액션 사용
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += OnJump;

        // 마우스 커서 숨기기 및 중앙 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        // [수정 핵심] 인풋 액션 설정 대신 마우스 델타값을 직접 읽어옵니다.
        if (Mouse.current != null)
        {
            mouseLook = Mouse.current.delta.ReadValue();
        }

        HandleGroundCheck();
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        float mouseX = mouseLook.x * mouseSensitivity;
        float mouseY = mouseLook.y * mouseSensitivity;

        // 1. 좌우 회전 (플레이어 몸통 회전)
        transform.Rotate(Vector3.up * mouseX);

        // 2. 상하 회전 (카메라만 회전)
        if (cameraTransform != null)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -85f, 85f); // 고개가 너무 꺾이지 않게 제한
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        // 플레이어가 바라보는 방향을 기준으로 이동
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
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