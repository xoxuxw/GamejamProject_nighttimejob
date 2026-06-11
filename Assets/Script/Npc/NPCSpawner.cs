using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("프리팹 설정")]
    [SerializeField] private GameObject normalNPCPrefab; // 정상 손님 프리팹
    [SerializeField] private GameObject badNPCPrefab;    // BadNPC 레이어를 가진 프리팹

    [Range(0f, 100f)]
    [SerializeField] private float badNPCOccurChance = 15f; // BadNPC가 태어날 확률 (15%)

    [Header("순찰 이름 규칙 설정")]
    [SerializeField] private string waypointNamePrefix = "spot_"; // 찾을 오브젝트 이름의 접두사

    // 자동으로 채워질 순찰 및 스폰 위치 리스트
    private List<Transform> spawnAndPatrolWaypoints = new List<Transform>();

    [Header("제한 설정")]
    [SerializeField] private int maxNPCCount = 10;       // 최대 스폰 인원 수
    [SerializeField] private float spawnInterval = 3f;   // 스폰 체크 주기 (초)

    // 현재 맵에 살아있는 NPC들을 관리할 리스트
    private List<GameObject> activeNPCs = new List<GameObject>();

    private void Awake()
    {
        // 게임이 시작되자마자 씬에서 규칙에 맞는 이름을 가진 오브젝트를 자동으로 모두 찾습니다.
        FindWaypointsAutomatically();
    }

    private void Start()
    {
        if (spawnAndPatrolWaypoints == null || spawnAndPatrolWaypoints.Count == 0)
        {
            Debug.LogError($"스포너가 '{waypointNamePrefix}'로 시작하는 오브젝트를 찾지 못했습니다! 씬의 오브젝트 이름을 확인해주세요.");
            return;
        }

        // 반복적으로 스폰을 시도하는 루틴 시작
        StartCoroutine(SpawnRoutine());
    }

    // 이름 규칙에 따라 오브젝트를 자동으로 찾아 리스트에 수집하는 함수
    private void FindWaypointsAutomatically()
    {
        spawnAndPatrolWaypoints.Clear();
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

        Debug.Log($"[NPCSpawner] '{waypointNamePrefix}' 규칙을 가진 지점을 자동으로 {spawnAndPatrolWaypoints.Count}개 찾아서 할당했습니다.");
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 리스트에서 이미 파괴된(죽거나 사라진) NPC 제거
            activeNPCs.RemoveAll(npc => npc == null);

            // 현재 살아있는 NPC가 10명 미만일 때만 스폰 진행
            if (activeNPCs.Count < maxNPCCount)
            {
                SpawnNPC();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnNPC()
    {
        // [수정 완료] 수집된 지점들 중에서 무작위로 하나를 골라 스폰 위치로 지정합니다.
        int randomIndex = Random.Range(0, spawnAndPatrolWaypoints.Count);
        Vector3 spawnPosition = spawnAndPatrolWaypoints[randomIndex].position;

        // 확률에 따른 프리팹 결정
        GameObject prefabToSpawn = normalNPCPrefab;
        float roll = Random.Range(0f, 100f);

        if (roll <= badNPCOccurChance)
        {
            prefabToSpawn = badNPCOccurChance > 0 && badNPCPrefab != null ? badNPCPrefab : normalNPCPrefab;
        }

        // 선택된 무작위 위치에 프리팹 생성
        GameObject spawnedNPC = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // 생성된 NPC에게 순찰 지점 리스트 주입
        Unit unitScript = spawnedNPC.GetComponent<Unit>();
        if (unitScript != null)
        {
            unitScript.SetupPatrolWaypoints(spawnAndPatrolWaypoints);
        }

        activeNPCs.Add(spawnedNPC);
    }
}