using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    PathReqeustManager requestManager;
    AGrid grid;

    private void Awake()
    {
        requestManager = GetComponent<PathReqeustManager>();
        grid = GetComponent<AGrid>();
    }

    // PathRequestManager로부터 길찾기 요청을 시작
    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        ANode startNode = grid.GetNodeFromWorldPoint(startPos);
        ANode targetNode = grid.GetNodeFromWorldPoint(targetPos);

        if (startNode.isWalkAble && targetNode.isWalkAble)
        {
            List<ANode> openList = new List<ANode>();
            HashSet<ANode> closedList = new HashSet<ANode>();
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                ANode currentNode = openList[0];

                // F cost 최솟값 노드 선택 (같으면 H cost 기준)
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].fCost < currentNode.fCost ||
                        openList[i].fCost == currentNode.fCost &&
                        openList[i].hCost < currentNode.hCost)
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // 목표 도달 시 탐색 종료
                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                // 이웃 노드 탐색
                foreach (ANode n in grid.GetNeighbours(currentNode))
                {
                    if (!n.isWalkAble || closedList.Contains(n))
                        continue;

                    int newCost = currentNode.gCost + GetDistanceCost(currentNode, n);
                    if (newCost < n.gCost || !openList.Contains(n))
                    {
                        n.gCost = newCost;
                        n.hCost = GetDistanceCost(n, targetNode);
                        n.parentNode = currentNode;

                        if (!openList.Contains(n))
                            openList.Add(n);
                    }
                }
            }
        }

        yield return null;

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    // 목표 노드에서 시작 노드까지 역추적하여 경로 구성
    Vector3[] RetracePath(ANode startNode, ANode endNode)
    {
        List<ANode> path = new List<ANode>();
        ANode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }

        grid.path = path;  // Gizmo 시각화용으로 grid에 전달

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    // 방향 변경 지점만 waypoint로 추출 (경로 단순화)
    Vector3[] SimplifyPath(List<ANode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(
                path[i - 1].gridX - path[i].gridX,
                path[i - 1].gridY - path[i].gridY
            );

            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPos);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    // 두 노드 간 거리 비용 (대각선: 14, 직선: 10)
    int GetDistanceCost(ANode nodeA, ANode nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
}