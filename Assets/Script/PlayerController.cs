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

    void HandleMovement()
    {
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