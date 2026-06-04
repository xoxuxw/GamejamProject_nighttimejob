using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("순찰 위치 설정")]
    [SerializeField] private List<Transform> patrolWaypoints; // 유니티 인스펙터에서 등록할 전체 빈 오브젝트 리스트
    [SerializeField] private float minWaitTime = 2f;         // 도착 후 최소 대기 시간
    [SerializeField] private float maxWaitTime = 5f;         // 도착 후 최대 대기 시간

    [Header("이동 설정")]
    [SerializeField] private float speed = 5f;

    private Vector3[] path;
    private int targetIndex;
    private Transform currentTarget; // 현재 이동 목표인 빈 오브젝트

    // --- 중복 방문 방지를 위한 리스트 ---
    private List<Transform> availableWaypoints = new List<Transform>();

    private bool isGrabbed = false;
    private bool isThrown = false;
    private bool isWaiting = false; // 대기 중인지 상태 체크

    private void Start()
    {
        // 시작할 때 최초로 사용 가능한 웨이포인트 목록을 세팅하고 이동 시작
        ResetAvailableWaypoints();
        MoveToNextRandomWaypoint();
    }

    // 가야 할 목적지 리스트를 원본으로부터 다시 충전하는 함수
    private void ResetAvailableWaypoints()
    {
        if (patrolWaypoints == null || patrolWaypoints.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 원본 patrolWaypoints 리스트가 비어있어 초기화할 수 없습니다.");
            return;
        }

        // 원본 리스트의 데이터를 복사해옵니다.
        availableWaypoints = new List<Transform>(patrolWaypoints);
        Debug.Log($"{gameObject.name}: 모든 사이클을 완료하여 목적지 목록을 새로 충전합니다. (총 {availableWaypoints.Count}개)");
    }

    // 다음 목적지를 정하고 길찾기를 요청하는 함수
    private void MoveToNextRandomWaypoint()
    {
        // 방어적 프로그래밍: 인스펙터에 등록된 원본이 없으면 실행 중지
        if (patrolWaypoints == null || patrolWaypoints.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 순찰할 빈 오브젝트(Waypoints)가 리스트에 없습니다!");
            return;
        }

        isWaiting = false;

        // [수정 포인트 1] 만약 현재 남은 목적지가 하나도 없다면 즉시 리스트를 재충전합니다.
        if (availableWaypoints == null || availableWaypoints.Count == 0)
        {
            ResetAvailableWaypoints();
        }

        // 혹시나 원본 리스트 자체가 비어있어 충전 후에도 0개라면 실행을 중지하여 에러 방지
        if (availableWaypoints.Count == 0) return;

        // 남은 목적지 리스트 중에서 무작위로 하나를 선택
        int randomIndex = Random.Range(0, availableWaypoints.Count);
        currentTarget = availableWaypoints[randomIndex];

        // 이번 사이클에서 재방문하지 않도록 남은 목록에서 제외시킵니다.
        availableWaypoints.RemoveAt(randomIndex);

        // 선택된 목적지로 A* 길찾기 요청
        PathReqeustManager.ReqeustPath(transform.position, currentTarget.position, OnPathFound);
    }

    public void OnGrabbed()
    {
        isGrabbed = true;
        isThrown = false;
        isWaiting = false;
        StopCoroutine("FollowPath");
        StopCoroutine("WaitForLanding");
        StopCoroutine("WaitAtWaypoint");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        targetIndex = 0;

        // 놓아졌을 때 기존 가던 목적지가 있다면 그곳으로 재요청, 없으면 새로 지정
        if (currentTarget != null)
            PathReqeustManager.ReqeustPath(transform.position, currentTarget.position, OnPathFound);
        else
            MoveToNextRandomWaypoint();
    }

    // 던졌을 때 호출 - 착지 후 이동 재개
    public void OnThrown()
    {
        isGrabbed = false;
        isThrown = true;
        isWaiting = false;
        StopCoroutine("FollowPath");
        StopCoroutine("WaitAtWaypoint");
        StartCoroutine("WaitForLanding");
    }

    // Rigidbody 속도가 낮아지면 착지로 판단
    IEnumerator WaitForLanding()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // 던진 직후 잠깐 대기
        yield return new WaitForSeconds(0.3f);

        while (rb != null && rb.linearVelocity.magnitude > 0.5f)
        {
            yield return null;
        }

        isThrown = false;
        targetIndex = 0;

        if (currentTarget != null)
            PathReqeustManager.ReqeustPath(transform.position, currentTarget.position, OnPathFound);
        else
            MoveToNextRandomWaypoint();
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful && !isGrabbed && !isThrown && !isWaiting)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
        else if (!pathSuccessful && !isGrabbed && !isThrown && !isWaiting)
        {
            // 혹시라도 길찾기에 실패했을 경우 멍때리지 않고 다음 목적지를 찾도록 예외 처리 추가
            Debug.LogWarning($"{gameObject.name}: {currentTarget.name}으로의 길찾기에 실패했습니다. 다른 목적지를 탐색합니다.");
            MoveToNextRandomWaypoint();
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length == 0)
        {
            // 경로의 길이가 0이라는 것은 이미 목적지 노드 위에 서있거나 도달 불가능하다는 뜻이므로 다음 타겟팅 유도
            StartCoroutine("WaitAtWaypoint");
            yield break;
        }

        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (isGrabbed || isThrown) yield break;

            // 목적지 노드(중간 거점)에 가까워졌을 때
            if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f)
            {
                targetIndex++;

                // 최종 목적지에 안전하게 도달했는지 확인
                if (targetIndex >= path.Length)
                {
                    StopCoroutine("FollowPath");
                    StartCoroutine("WaitAtWaypoint");
                    yield break;
                }

                currentWaypoint = path[targetIndex];
            }

            // 캐릭터 이동
            transform.position = Vector3.MoveTowards(
                transform.position, currentWaypoint, speed * Time.deltaTime
            );

            // 이동 방향으로 회전
            Vector3 direction = (currentWaypoint - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, Time.deltaTime * 10f
                );
            }

            yield return null;
        }
    }

    // 목적지에 도착한 후 무작위 시간 동안 대기하는 코루틴
    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        // [수정 포인트 2] 대기가 끝나고 출발하기 전에도 한 번 더 리스트 상태를 안전하게 검사합니다.
        if (availableWaypoints == null || availableWaypoints.Count == 0)
        {
            ResetAvailableWaypoints();
        }

        // 대기가 끝나면 다시 새로운 랜덤 목적지로 출발
        MoveToNextRandomWaypoint();
    }
}