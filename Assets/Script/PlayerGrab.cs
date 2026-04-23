using UnityEngine;
using UnityEngine.InputSystem; // 이 네임스페이스가 반드시 필요합니다.

public class PlayerGrab : MonoBehaviour
{
    public GrabLitch grabLitch;

    void Awake()
    {
        grabLitch = GetComponent<GrabLitch>();
    }

    void Update()
    {
        // Keyboard.current를 사용하는 것이 New Input System의 방식입니다.
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (grabLitch != null && grabLitch.canGrab)
            {
                Grab();
            }
        }
    }

    void Grab()
    {
        Debug.Log("Grabbed!");
    }
}