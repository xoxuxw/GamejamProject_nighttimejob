using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("프리팹 설정")]
    [SerializeField] private GameObject normalNPCPrefab;
    [SerializeField] private GameObject badNPCPrefab;

    [Range(0f, 100f)]
    [SerializeField] private float badNPCOccurChance = 15f;

    [Header("스폰 이름 규칙 설정")]
    [SerializeField] private string waypointNamePrefix = "spawn_"; // [수정] spawn_ 으로 수집하도록 유도

    [Header("순찰 위치 직접 설정")]
    // [추가] 인스펙터에서 spot_ 들을 그룹으로 묶은 부모 오브젝트의 자식들을 싹 넣어줄 리스트
    [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();

    private List<Transform> spawnAndPatrolWaypoints = new List<Transform>();

    [Header("제한 설정")]
    [SerializeField] private int maxNPCCount = 10;
    [SerializeField] private float spawnInterval = 3f;

    private List<GameObject> activeNPCs = new List<GameObject>();

    private void Awake()
    {
        FindWaypointsAutomatically();
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private void FindWaypointsAutomatically()
    {
        int index = 1;
        while (true)
        {
            string targetName = waypointNamePrefix + index;
            GameObject foundObj = GameObject.Find(targetName);

            if (foundObj != null)
            {
                spawnAndPatrolWaypoints.Add(foundObj.transform);
                index++;
            }
            else
            {
                break;
            }
        }
        Debug.Log($"[NPCSpawner] 스폰 지점을 자동으로 {spawnAndPatrolWaypoints.Count}개 찾았습니다.");
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            activeNPCs.RemoveAll(npc => npc == null);

            if (activeNPCs.Count < maxNPCCount)
            {
                SpawnNPC();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnNPC()
    {
        if (spawnAndPatrolWaypoints.Count == 0) return;

        // 1. spawn_ 지점 중 랜덤하게 골라 스폰 위치 결정
        int randomIndex = Random.Range(0, spawnAndPatrolWaypoints.Count);
        Vector3 spawnPosition = spawnAndPatrolWaypoints[randomIndex].position;

        GameObject prefabToSpawn = normalNPCPrefab;
        float roll = Random.Range(0f, 100f);

        if (roll <= badNPCOccurChance)
        {
            prefabToSpawn = badNPCOccurChance > 0 && badNPCPrefab != null ? badNPCPrefab : normalNPCPrefab;
        }

        GameObject spawnedNPC = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        activeNPCs.Add(spawnedNPC);

        // 2. [핵심 추가] 태어난 NPC에게 스폰 지점이 아닌, "순찰(patrolWaypoints) 지점 리스트"를 강제로 쥐여줍니다.
        Unit unitScript = spawnedNPC.GetComponent<Unit>();
        if (unitScript != null)
        {
            unitScript.SetupPatrolWaypoints(patrolWaypoints);
        }
    }
}