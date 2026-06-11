using System;
using System.Collections.Generic;
using UnityEngine;

public class PathReqeustManager : MonoBehaviour
{
    public struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> cb)
        {
            pathStart = start;
            pathEnd = end;
            callback = cb;
        }
    }

    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    private PathRequest currentPathRequest;

    public static PathReqeustManager instance;
    private PathFinding pathfinding;

    [HideInInspector] public bool isProcessingPath;

    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<PathFinding>();
    }

    public static void ReqeustPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        if (instance == null) return;

        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    private void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();

            // 대기열에 있던 중 NPC가 소멸했거나 컴포넌트가 꺼졌다면 즉시 패스
            if (currentPathRequest.callback == null ||
                currentPathRequest.callback.Target == null ||
                currentPathRequest.callback.Target.Equals(null))
            {
                TryProcessNext();
                return;
            }

            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        // 연산이 끝났을 때 NPC가 여전히 살아있는지 안전성 검사
        if (currentPathRequest.callback != null)
        {
            if (currentPathRequest.callback.Target != null && !currentPathRequest.callback.Target.Equals(null))
            {
                currentPathRequest.callback(path, success);
            }
        }

        // 어떤 상황에서든 매니저는 락을 풀고 다음 대기열을 강제로 가동시킵니다.
        isProcessingPath = false;
        TryProcessNext();
    }
}