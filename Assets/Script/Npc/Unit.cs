using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("순찰 위치 설정")]
    [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;

    [Header("이동 설정")]
    [SerializeField] private float speed = 5f;

    private Vector3[] path;
    private int targetIndex;
    private Transform currentTarget;

    // --- 중복 방문 방지를 위한 리스트 ---
    private List<Transform> availableWaypoints = new List<Transform>();

    private bool isGrabbed = false;
    private bool isThrown = false;
    private bool isWaiting = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 태어나자마자 넘어지지 않게 물리 회전축 잠금
        LockRotation(true);
    }

    private void Start()
    {
        if (patrolWaypoints != null && patrolWaypoints.Count > 0)
        {
            ResetAvailableWaypoints();
            MoveToNextRandomWaypoint();
        }
    }

    public void SetupPatrolWaypoints(List<Transform> waypoints)
    {
        patrolWaypoints = new List<Transform>(waypoints);
        ResetAvailableWaypoints();
        MoveToNextRandomWaypoint();
    }

    private void ResetAvailableWaypoints()
    {
        if (patrolWaypoints == null || patrolWaypoints.Count == 0) return;
        availableWaypoints = new List<Transform>(patrolWaypoints);
    }

    private void MoveToNextRandomWaypoint()
    {
        if (patrolWaypoints == null || patrolWaypoints.Count == 0) return;

        isWaiting = false;

        if (availableWaypoints == null || availableWaypoints.Count == 0)
        {
            ResetAvailableWaypoints();
        }

        if (availableWaypoints.Count == 0) return;

        int randomIndex = Random.Range(0, availableWaypoints.Count);
        currentTarget = availableWaypoints[randomIndex];
        availableWaypoints.RemoveAt(randomIndex);

        ForceStandUp();
        LockRotation(true);

        PathReqeustManager.ReqeustPath(transform.position, currentTarget.position, OnPathFound);
    }

    public void OnGrabbed()
    {
        isGrabbed = true;
        isThrown = false;
        isWaiting = false;

        LockRotation(false);

        StopCoroutine("FollowPath");
        StopCoroutine("WaitForLanding");
        StopCoroutine("WaitAtWaypoint");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        targetIndex = 0;

        if (currentTarget != null)
            PathReqeustManager.ReqeustPath(transform.position, currentTarget.position, OnPathFound);
        else
            MoveToNextRandomWaypoint();
    }

    public void OnThrown()
    {
        isGrabbed = false;
        isThrown = true;
        isWaiting = false;

        LockRotation(false);

        StopCoroutine("FollowPath");
        StopCoroutine("WaitAtWaypoint");
        StartCoroutine("WaitForLanding");
    }

    IEnumerator WaitForLanding()
    {
        yield return new WaitForSeconds(0.4f);

        while (rb != null && GetVelocity().magnitude > 0.3f)
        {
            yield return null;
        }

        isThrown = false;
        targetIndex = 0;

        ForceStandUp();
        LockRotation(true);

        if (currentTarget != null)
            PathReqeustManager.ReqeustPath(transform.position, currentTarget.position, OnPathFound);
        else
            MoveToNextRandomWaypoint();
    }

    // Unit.cs 내부 수정할 부분

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
            // [수정] 즉시 재요청하는 무한 루프 버그를 지우고, 안전하게 쿨타임을 가지는 코루틴을 실행합니다.
            StopCoroutine("HandlePathRequestFailure");
            StartCoroutine("HandlePathRequestFailure");
        }
    }

    // [추가] 길찾기 실패 시 프레임 마비를 차단하고 안전하게 재시도하는 코루틴
    private IEnumerator HandlePathRequestFailure()
    {
        // 0.5초간 대기하여 큐에 명령이 수천 개 쌓이는 과부하를 원천 방지
        yield return new WaitForSeconds(0.5f);

        if (!isGrabbed && !isThrown && !isWaiting)
        {
            MoveToNextRandomWaypoint();
        }
    }

    // Unit.cs의 FollowPath() 코루틴 내부 수정
    IEnumerator FollowPath()
    {
        if (path.Length == 0)
        {
            StartCoroutine("WaitAtWaypoint");
            yield break;
        }
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (isGrabbed || isThrown) yield break;

            // [보완] 거리를 0.4f로 늘려주어 물리 오차 때문에 멈칫거리는 버그를 예방합니다.
            if (Vector3.Distance(transform.position, currentWaypoint) < 0.4f)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    StopCoroutine("FollowPath");
                    StartCoroutine("WaitAtWaypoint");
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }

            // 물리 이동 및 물리 회전
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            if (rb != null) rb.MovePosition(nextPosition);
            else transform.position = nextPosition;

            Vector3 direction = (currentWaypoint - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion nextRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                if (rb != null) rb.MoveRotation(nextRotation);
                else transform.rotation = nextRotation;
            }

            yield return null;
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        if (availableWaypoints == null || availableWaypoints.Count == 0)
        {
            ResetAvailableWaypoints();
        }

        MoveToNextRandomWaypoint();
    }

    private void LockRotation(bool shouldLock)
    {
        if (rb == null) return;

        if (shouldLock)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    private void ForceStandUp()
    {
        if (rb == null) return;

        rb.angularVelocity = Vector3.zero;
        if (rb != null)
        {
#if UNITY_2023_1_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
        }

        Vector3 currentEuler = transform.eulerAngles;

        // 회전 고정 상태일 때 트랜스폼 대신 물리 시스템으로 안전하게 정렬 후 고정
        Quaternion targetRot = Quaternion.Euler(0f, currentEuler.y, 0f);
        rb.MoveRotation(targetRot);
    }

    private Vector3 GetVelocity()
    {
#if UNITY_2023_1_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }
}