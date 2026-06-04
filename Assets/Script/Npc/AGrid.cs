using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGrid : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius = 0.5f;  // 기본값 지정
    ANode[,] grid;

    public List<ANode> path;  // 경로 시각화용 (Part 3 추가)

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;

    [Header("실시간 갱신 설정")]
    [SerializeField] private float refreshInterval = 0.2f; // 0.2초마다 맵을 실시간 리스캔 (NPC 위치 추적)

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // 최초 맵 구성
        CreateGrid();
    }

    private void Start()
    {
        // [추가] 게임 시작 후, NPC들의 실시간 이동을 반영하기 위해 코루틴으로 주기적 리스캔을 켜줍니다.
        StartCoroutine(RefreshGridRoutine());
    }

    // [핵심 보완] 게임 중 끊임없이 장애물과 NPC 위치를 레이캐스트/스케닝하여 갱신하는 루틴
    private IEnumerator RefreshGridRoutine()
    {
        while (true)
        {
            UpdateGridWalkability();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    // AGrid.cs 내부 수정할 부분

    void CreateGrid()
    {
        grid = new ANode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position
            - Vector3.right * gridWorldSize.x / 2
            - Vector3.forward * gridWorldSize.y / 2;

        // [추가] NPC와 BadNPC 레이어는 장애물 스캔 마스크에서 강제로 제외시킵니다.
        int npcLayer = LayerMask.GetMask("NPC");
        int badNpcLayer = LayerMask.GetMask("BadNPC");
        int finalMask = unwalkableMask.value & ~npcLayer & ~badNpcLayer;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft
                    + Vector3.right * (x * nodeDiameter + nodeRadius)
                    + Vector3.forward * (y * nodeDiameter + nodeRadius);

                // unwalkableMask 대신 안전하게 정제된 finalMask 사용
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, finalMask);
                grid[x, y] = new ANode(walkable, worldPoint, x, y);
            }
        }
    }

    void UpdateGridWalkability()
    {
        if (grid == null) return;

        // [추가] 실시간 스캔에서도 NPC 레이어들은 완벽히 차단합니다.
        int npcLayer = LayerMask.GetMask("NPC");
        int badNpcLayer = LayerMask.GetMask("BadNPC");
        int finalMask = unwalkableMask.value & ~npcLayer & ~badNpcLayer;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (grid[x, y] == null) continue;

                // unwalkableMask 대신 finalMask 사용
                bool walkable = !Physics.CheckSphere(grid[x, y].worldPos, nodeRadius, finalMask);
                grid[x, y].isWalkAble = walkable;
            }
        }
    }

    // 월드 포지션 → 그리드 노드 변환
    public ANode GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    // 이웃 노드 8방향 반환
    public List<ANode> GetNeighbours(ANode node)
    {
        List<ANode> neighbours = new List<ANode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX &&
                    checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (ANode n in grid)
            {
                Gizmos.color = n.isWalkAble ? Color.white : Color.red;

                // 탐색된 경로 노드는 검정색으로 표시
                if (path != null && path.Contains(n))
                    Gizmos.color = Color.black;

                Gizmos.DrawCube(n.worldPos, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}