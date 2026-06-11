using UnityEngine;

public class GrabLitch : MonoBehaviour
{
    public bool canGrab { get; private set; }

    // 현재 트리거 범위 안에 들어와 있는 오브젝트를 저장
    public GameObject targetObject { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        // [수정] 레이어가 BadNPC 뿐만 아니라 일반 "NPC" 레이어도 잡을 수 있도록 조건 추가
        if (other.gameObject.layer == LayerMask.NameToLayer("BadNPC") ||
            other.gameObject.layer == LayerMask.NameToLayer("NPC"))
        {
            canGrab = true;
            targetObject = other.gameObject; // 닿은 오브젝트 저장
            Debug.Log($"대상 발견 (NPC계열): {targetObject.name}");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Broomstick"))
        {
            canGrab = true;
            targetObject = other.gameObject; // 닿은 오브젝트 저장
            Debug.Log($"대상 발견: {targetObject.name}");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Scanner"))
        {
            canGrab = true;
            targetObject = other.gameObject; // 닿은 오브젝트 저장
            Debug.Log($"대상 발견: {targetObject.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 나가려는 오브젝트가 현재 저장된 targetObject와 같다면 비우기
        if (other.gameObject == targetObject)
        {
            Debug.Log($"대상 사라짐: {targetObject.name}");
            canGrab = false;
            targetObject = null;
        }
    }
}