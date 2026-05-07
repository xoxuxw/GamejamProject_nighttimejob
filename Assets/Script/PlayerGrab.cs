using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    private GrabLitch grabLitch;
    public Transform holdParent;
    private GameObject currentGrabbedObject;

    void Awake()
    {
        grabLitch = GetComponentInChildren<GrabLitch>();
    }

    void Update()
    {
        // 1. 왼쪽 클릭: 잡기 또는 놓기
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentGrabbedObject == null)
            {
                if (grabLitch != null && grabLitch.canGrab && grabLitch.targetObject != null)
                    Grab(grabLitch.targetObject);
            }
            else
            {
                Drop();
            }
        }

        // 2. 우클릭: 잡은 상태에서 레이어별 상호작용 (우클릭 액션)
        if (Mouse.current.rightButton.wasPressedThisFrame && currentGrabbedObject != null)
        {
            HandleInteraction(currentGrabbedObject);
        }
    }

    void HandleInteraction(GameObject obj)
    {
        // 레이어 번호를 이름으로 변환해서 체크
        string layerName = LayerMask.LayerToName(obj.layer);

        switch (layerName)
        {
            case "BadNPC":
                InteractWithNPC(obj);
                break;
            case "Broomstick":
                InteractWithBroomstick(obj);
                break;
            case "Scanner":
                InteractWithScanner(obj);
                break;
            default:
                Debug.Log("이 물건은 특수 상호작용이 없습니다.");
                break;
        }
    }

    // --- 레이어별 상세 로직 뼈대 ---

    void InteractWithNPC(GameObject npc)
    {
        Debug.Log("BadNPC 상호작용");
        // 예: npc.GetComponent<NPCAction>().Scare();
    }

    void InteractWithBroomstick(GameObject broom)
    {
        Debug.Log("Broomstick 상호작용");
    }

    void InteractWithScanner(GameObject scanner)
    {
        Debug.Log("Scanner 상호작용");
    }

    // --- 잡기 및 놓기 기본 로직 ---

    void Grab(GameObject obj)
    {
        currentGrabbedObject = obj;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        obj.transform.SetParent(holdParent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void Drop()
    {
        Rigidbody rb = currentGrabbedObject.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }

        Collider col = currentGrabbedObject.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        currentGrabbedObject.transform.SetParent(null);
        currentGrabbedObject = null;
    }
}