using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;
    float speed = 5;
    Vector3[] path;
    int targetIndex;

    private bool isGrabbed = false;
    private bool isThrown = false;  // 던져진 상태 추가

    private void Start()
    {
        PathReqeustManager.ReqeustPath(transform.position, target.position, OnPathFound);
    }

    public void OnGrabbed()
    {
        isGrabbed = true;
        isThrown = false;
        StopCoroutine("FollowPath");
        StopCoroutine("WaitForLanding");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        targetIndex = 0;
        PathReqeustManager.ReqeustPath(transform.position, target.position, OnPathFound);
    }

    // 던졌을 때 호출 - 착지 후 이동 재개
    public void OnThrown()
    {
        isGrabbed = false;
        isThrown = true;
        StopCoroutine("FollowPath");
        StartCoroutine("WaitForLanding");
    }

    // Rigidbody 속도가 낮아지면 착지로 판단
    IEnumerator WaitForLanding()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // 던진 직후 잠깐 대기
        yield return new WaitForSeconds(0.3f);

        // 속도가 0.5 이하가 될 때까지 대기
        while (rb != null && rb.linearVelocity.magnitude > 0.5f)
        {
            yield return null;
        }

        isThrown = false;
        targetIndex = 0;
        PathReqeustManager.ReqeustPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful && !isGrabbed && !isThrown)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length == 0) yield break;
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (isGrabbed || isThrown) yield break;

            if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                    yield break;

                currentWaypoint = path[targetIndex];
            }

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
}