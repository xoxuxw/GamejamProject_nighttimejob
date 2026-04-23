using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    private GrabLitch grabLitch;
    public Transform holdParent; // 물건을 고정시킬 손 위치 (인스펙터에서 할당)

    void Awake()
    {
        grabLitch = GetComponentInChildren<GrabLitch>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (grabLitch != null && grabLitch.canGrab && grabLitch.targetObject != null)
            {
                Grab(grabLitch.targetObject);
            }
        }
    }

    void Grab(GameObject obj)
    {
        Debug.Log($"{obj.name}을(를) 물리적으로 잡습니다!");

        // 예시: 물리 효과를 끄고 손 자식으로 넣기
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 엔진 영향 정지
        }

        obj.transform.SetParent(holdParent); // 손 위치로 부모 변경
        obj.transform.localPosition = Vector3.zero; // 위치 초기화
    }
}