using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    private GrabLitch grabLitch;
    public Transform holdParent; // 물건을 고정시킬 손 위치
    private GameObject currentGrabbedObject; // 현재 들고 있는 오브젝트 저장

    void Awake()
    {
        grabLitch = GetComponentInChildren<GrabLitch>();
    }

    void Update()
    {
        // 마우스 왼쪽 버튼이 눌렸을 때
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentGrabbedObject == null)
            {
                // 1. 아무것도 안 들고 있다면 -> 잡기 시도
                if (grabLitch != null && grabLitch.canGrab && grabLitch.targetObject != null)
                {
                    Grab(grabLitch.targetObject);
                }
            }
            else
            {
                // 2. 이미 무언가 들고 있다면 -> 놓기
                Drop();
            }
        }
    }

    void Grab(GameObject obj)
    {
        currentGrabbedObject = obj;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 정지
            rb.useGravity = false; // 중력 끄기
        }

        // 콜라이더가 트리거와 계속 충돌해서 튕기는 것을 방지하려면 꺼주는 것이 좋습니다.
        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        obj.transform.SetParent(holdParent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity; // 회전도 정렬

        Debug.Log($"{obj.name}을(를) 마우스로 잡았습니다.");
    }

    void Drop()
    {
        if (currentGrabbedObject == null) return;

        Rigidbody rb = currentGrabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // 물리 다시 시작
            rb.useGravity = true;  // 중력 다시 켜기
        }

        Collider col = currentGrabbedObject.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        currentGrabbedObject.transform.SetParent(null); // 부모 해제

        Debug.Log($"{currentGrabbedObject.name}을(를) 놓았습니다.");
        currentGrabbedObject = null;
    }
}