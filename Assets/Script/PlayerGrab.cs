using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    private GrabLitch grabLitch;

    [Header("연결 설정")]
    public Transform holdParent; // 물건이 붙을 위치 (예: 손 오브젝트)
    private GameObject currentGrabbedObject;

    [Header("던지기 설정")]
    public float throwForce = 15f;

    void Awake()
    {
        // 자식 오브젝트에 있을 GrabLitch(트리거)를 찾음
        grabLitch = GetComponentInChildren<GrabLitch>();
    }

    void Update()
    {
        // 1. 왼쪽 클릭: 잡기 / 놓기
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentGrabbedObject == null)
            {
                if (grabLitch != null && grabLitch.canGrab && grabLitch.targetObject != null)
                {
                    Grab(grabLitch.targetObject);
                }
            }
            else
            {
                Drop();
            }
        }

        // 2. 오른쪽 클릭: 잡은 상태에서 상호작용 (던지기 등)
        if (Mouse.current.rightButton.wasPressedThisFrame && currentGrabbedObject != null)
        {
            HandleInteraction(currentGrabbedObject);
        }
    }

    void HandleInteraction(GameObject obj)
    {
        string layerName = LayerMask.LayerToName(obj.layer);

        switch (layerName)
        {
            case "BadNPC":
                InteractWithNPC(obj);
                break;
            case "Broomstick":
                Debug.Log("Broomstick 상호작용");
                break;
            case "Scanner":
                Debug.Log("Scanner 상호작용");
                break;
        }
    }

    void InteractWithNPC(GameObject npc)
    {
        // 던지기 로직
        npc.transform.SetParent(null);

        Rigidbody rb = npc.GetComponent<Rigidbody>();
        Collider col = npc.GetComponent<Collider>();

        if (col != null) col.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            // 정면 약간 위쪽으로 던지기
            Vector3 throwDir = (transform.forward + Vector3.up * 0.1f).normalized;
            rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
            // 약간의 회전 추가
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        currentGrabbedObject = null;
        Debug.Log("NPC를 던졌습니다!");
    }

    void Grab(GameObject obj)
    {
        currentGrabbedObject = obj;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Collider col = obj.GetComponent<Collider>();

        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        if (col != null) col.enabled = false; // 잡는 동안 충돌 방지

        obj.transform.SetParent(holdParent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void Drop()
    {
        if (currentGrabbedObject == null) return;

        Rigidbody rb = currentGrabbedObject.GetComponent<Rigidbody>();
        Collider col = currentGrabbedObject.GetComponent<Collider>();

        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        if (col != null) col.enabled = true;

        currentGrabbedObject.transform.SetParent(null);
        currentGrabbedObject = null;
    }
}