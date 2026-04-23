using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Rigidbody player_rigidbody;
    private Vector2 moveInput;
    private Vector2 mouseLook;

    [Header("카메라 설정")]
    public Transform cameraTransform; // 인스펙터에서 Main Camera를 드래그해서 넣어주세요!
    public float mouseSensitivity = 0.1f;
    private float xRotation = 0f; // 이 변수를 이제 실제로 사용합니다.

    [Header("이동 설정")]
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

        // 마우스 델타 값 받기
        inputActions.Player.Look.performed += ctx => mouseLook = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => mouseLook = Vector2.zero;

        // 마우스 커서 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        HandleGroundCheck();
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        float mouseX = mouseLook.x * mouseSensitivity;
        float mouseY = mouseLook.y * mouseSensitivity;

        // 1. 좌우 회전: 캐릭터 몸통을 Y축 기준으로 회전
        transform.Rotate(Vector3.up * mouseX);

        // 2. 상하 회전: 카메라의 X축 회전값 계산 및 제한(-90~90도)
        if (cameraTransform != null)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        // 바라보는 방향 기준으로 이동
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