using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Rigidbody player_rigidbody;

    private Vector2 moveInput;

    // 여기부터
    public float jumpForce = 5f;

    [Header("바닥 감지 (레이캐스트)")]
    public float rayLength = 1.1f; // 캐릭터 중심에서 바닥까지의 거리 + 약간의 여유분
    public LayerMask groundLayer;  // 바닥으로 인식할 레이어
    private bool isGrounded;       // 현재 바닥에 닿아있는가?

    void Awake()
    {
        player_rigidbody = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Jump.performed += OnJump;

        // 키에서 손을 뗐을 때(canceled) OnMoveCanceled 함수 실행
        inputActions.Player.Move.canceled += OnMoveCanceled;


    }

    // 4. 스크립트가 켜질 때 인풋 시스템 활성화
    void OnEnable()
    {
        inputActions.Enable();
    }

    // 5. 스크립트가 꺼질 때 인풋 시스템 비활성화
    void OnDisable()
    {
        inputActions.Disable();
    }

    // 6. 이벤트 발생 시 실행될 함수들
    private void OnMove(InputAction.CallbackContext context)
    {
        // Vector2 형태로 들어온 입력값(WASD 방향)을 읽어서 변수에 저장
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // 손을 떼면 이동 값을 0으로 초기화
        moveInput = Vector2.zero;
    }



    void Update()
    {
        Vector3 origin = transform.position; // 시작점: 캐릭터의 중심
        Vector3 direction = Vector3.down;    // 방향: 아래쪽

        Debug.DrawRay(origin, direction * rayLength, Color.red);

        if (Physics.Raycast(origin, direction, rayLength, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(moveDirection * 5f * Time.deltaTime);
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            player_rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}