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

    private List<Transform> availableWaypoints = new List<Transform>();

    private bool isGrabbed = false;
    private bool isThrown = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        LockRotation(true);
    }

    private void Start()
    {
        if (patrolWaypoints != null && patrolWaypoints.Count > 0 && availableWaypoints.Count == 0)
        {
            StartFirstPatrol();
        }
    }

    public void StartFirstPatrol()
    {
        if (isGrabbed || isThrown) return;
        ResetAvailableWaypoints();
        MoveToNextRandomWaypoint();
    }

    public void SetupPatrolWaypoints(List<Transform> waypoints)
    {
        patrolWaypoints = new List<Transform>(waypoints);
        ResetAvailableWaypoints();
    }

    private void ResetAvailableWaypoints()
    {
        if (patrolWaypoints != null)
        {
            availableWaypoints = new List<Transform>(patrolWaypoints);
        }
    }

    private void MoveToNextRandomWaypoint()
    {
        if (isGrabbed || isThrown || !enabled) return;

        if (availableWaypoints == null || availableWaypoints.Count == 0)
        {
            ResetAvailableWaypoints();
        }

        if (availableWaypoints.Count > 0)
        {
            int randomIndex = Random.Range(0, availableWaypoints.Count);
            currentTarget = availableWaypoints[randomIndex];
            availableWaypoints.RemoveAt(randomIndex);

            PathReqeustManager.ReqeustPath(transform.position, currentTarget.position, OnPathFound);
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful && !isGrabbed && !isThrown && enabled)
        {
            path = newPath;
            targetIndex = 0;
            StopAllCoroutines();
            StartCoroutine(FollowPath());
        }
        // [★ 실시간 0.2초 맵 갱신 충돌 방어] 이동 중 혹은 스폰 직후 연산 유실 시 무조건 자동 심폐소생술
        else if (!isGrabbed && !isThrown && enabled)
        {
            StopAllCoroutines();
            StartCoroutine(RetryPathRequestDelay());
        }
    }

    private IEnumerator RetryPathRequestDelay()
    {
        // 0.25초 쉬고 갱신 타이밍을 빗겨 나가 다시 완벽하게 패스를 요청합니다.
        yield return new WaitForSeconds(0.25f);
        if (!isGrabbed && !isThrown && enabled)
        {
            MoveToNextRandomWaypoint();
        }
    }

    IEnumerator FollowPath()
    {
        if (path == null || path.Length == 0) yield break;
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (isGrabbed || isThrown) yield break;

            // 목적지 노드 보정용 (거리 판정 유연화)
            Vector3 currentPosFixed = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 waypointPosFixed = new Vector3(currentWaypoint.x, 0, currentWaypoint.z);

            if (Vector3.Distance(currentPosFixed, waypointPosFixed) < 0.2f)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    StartCoroutine(WaitAtWaypoint());
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }

            Vector3 targetPos = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion nextRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                nextRotation.x = 0;
                nextRotation.z = 0;
                transform.rotation = nextRotation;
            }

            yield return null;
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        MoveToNextRandomWaypoint();
    }

    private void LockRotation(bool shouldLock)
    {
        if (rb == null) return;
        rb.constraints = shouldLock ? (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ) : RigidbodyConstraints.None;
    }

    private void ForceStandUp()
    {
        if (rb == null) return;

        rb.angularVelocity = Vector3.zero;
#if UNITY_2023_1_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif

        Vector3 currentEuler = transform.eulerAngles;
        currentEuler.x = 0;
        currentEuler.z = 0;
        transform.eulerAngles = currentEuler;

        LockRotation(true);
        rb.WakeUp();
    }

    public void OnGrabbed()
    {
        isGrabbed = true;
        StopAllCoroutines();
        LockRotation(false);
    }

    public void OnReleased()
    {
        isGrabbed = false;
        isThrown = false;
        ForceStandUp();
        MoveToNextRandomWaypoint();
    }

    public void OnThrown()
    {
        isGrabbed = false;
        isThrown = true;
        StopAllCoroutines();
        LockRotation(false);
        StartCoroutine(CheckThrownStopRoutine());
    }

    private IEnumerator CheckThrownStopRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (isThrown)
        {
            if (rb != null && rb.linearVelocity.magnitude < 0.1f)
            {
                isThrown = false;
                ForceStandUp();
                MoveToNextRandomWaypoint();
                yield break;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnTriggerEnter(Collider other) { CheckWaterCollision(other.gameObject); }
    private void OnCollisionEnter(Collision collision) { CheckWaterCollision(collision.gameObject); }

    private void CheckWaterCollision(GameObject overlayObject)
    {
        if (overlayObject.layer == LayerMask.NameToLayer("Water"))
        {
            StopAllCoroutines();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            Destroy(gameObject);
        }
    }
}