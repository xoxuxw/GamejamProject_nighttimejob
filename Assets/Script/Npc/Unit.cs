using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;  // 인스펙터에서 목표 오브젝트 연결 필수!
    float speed = 5;
    Vector3[] path;
    int targetIndex;

    private void Start()
    {
        PathReqeustManager.ReqeustPath(transform.position, target.position, OnPathFound);
    }
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        Debug.Log("경로 탐색 결과: " + pathSuccessful + " / 경로 길이: " + newPath.Length);

        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;  // ← 이거 빠진 경우도 있어요! 추가 권장
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            // 부동소수점 오차 방지를 위해 Distance로 도달 판정
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
            yield return null;
        }
    }
}