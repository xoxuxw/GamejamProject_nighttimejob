using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrab : MonoBehaviour
{
    private GrabLitch grabLitch;

    [Header("연결 설정")]
    public Transform holdParent; // 물건이 붙을 위치 (손 오브젝트)
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

        // [★ 핵심 수정 및 보완] 
        // 잡혀있는 동안 물체가 플레이어 카메라의 상하 각도 및 몸통의 좌우 각도를 모두 실시간 동기화합니다.
        if (currentGrabbedObject != null && holdParent != null)
        {
            // 1. 위치는 지정된 손(holdParent) 위치에 고정
            currentGrabbedObject.transform.position = holdParent.position;

            // 2. [해결책] 플레이어 본체(좌우)의 회전과 메인 카메라(위아래)의 회전을 조합하여 물체에 강제 주입합니다.
            // 이렇게 해야 카메라가 하늘을 볼 때 물체도 하늘 방향으로 같이 비스듬히 누우며 들립니다.
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // 카메라의 3차원 회전값(상하좌우 모두 포함)을 그대로 물체에 복사합니다.
                currentGrabbedObject.transform.rotation = mainCam.transform.rotation;
            }
            else
            {
                // 만약 메인 카메라를 못 찾을 경우 기존 손 회전값 유지
                currentGrabbedObject.transform.rotation = holdParent.rotation;
            }
        }
    }

    void HandleInteraction(GameObject obj)
    {
        if (obj.layer == LayerMask.NameToLayer("BadNPC") || obj.layer == LayerMask.NameToLayer("NPC"))
        {
            InteractWithNPC(obj);
        }
        else if (obj.layer == LayerMask.NameToLayer("Broomstick"))
        {
            InteractWithBroomstick(obj);
        }
        else if (obj.layer == LayerMask.NameToLayer("Scanner"))
        {
            InteractWithScanner(obj);
        }
    }

    void Grab(GameObject obj)
    {
        currentGrabbedObject = obj;

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.None; // NPC의 뻣뻣한 회전 잠금 완전 해제
        }

        // 내 손(holdParent)의 자식으로 등록하되, 부모 기준 회전을 일단 초기화
        obj.transform.SetParent(holdParent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // Unit 이동 중지 명령 호출
        Unit unit = obj.GetComponent<Unit>();
        if (unit != null) unit.OnGrabbed();
    }

    void Drop()
    {
        if (currentGrabbedObject == null) return;

        Rigidbody rb = currentGrabbedObject.GetComponent<Rigidbody>();
        Collider col = currentGrabbedObject.GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        if (col != null) col.enabled = true;

        currentGrabbedObject.transform.SetParent(null);

        // Unit 이동 재개 명령 호출
        Unit unit = currentGrabbedObject.GetComponent<Unit>();
        if (unit != null) unit.OnReleased();

        currentGrabbedObject = null;
    }

    void InteractWithNPC(GameObject npc)
    {
        npc.transform.SetParent(null);
        Rigidbody rb = npc.GetComponent<Rigidbody>();
        Collider col = npc.GetComponent<Collider>();
        if (col != null) col.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            // 정면 바라보는 방향으로 힘 추가 (이제 카메라가 위를 보면 위쪽 대각선으로 날아갑니다!)
            Camera mainCam = Camera.main;
            Vector3 throwDir = mainCam != null ? mainCam.transform.forward : transform.forward;
            throwDir = (throwDir + Vector3.up * 0.1f).normalized;

            rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        Unit unit = npc.GetComponent<Unit>();
        if (unit != null) unit.OnThrown();

        currentGrabbedObject = null;
    }

    void InteractWithBroomstick(GameObject broom)
    {
        Debug.Log("빗자루 상호작용 실행");
    }

    void InteractWithScanner(GameObject scanner)
    {
        Debug.Log("스캐너 상호작용 실행");
    }
}