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
    [SerializeField] private string waypointNamePrefix = "spawn_";

    [Header("순찰 위치 직접 설정")]
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
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            for (int i = activeNPCs.Count - 1; i >= 0; i--)
            {
                if (activeNPCs[i] == null || activeNPCs[i].Equals(null))
                {
                    activeNPCs.RemoveAt(i);
                }
            }

            if (activeNPCs.Count < maxNPCCount)
            {
                SpawnNPC();
                yield return new WaitForSeconds(0.4f); // 스폰 병목 방지
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnNPC()
    {
        if (spawnAndPatrolWaypoints.Count == 0) return;

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

        Unit unitScript = spawnedNPC.GetComponent<Unit>();
        if (unitScript != null)
        {
            unitScript.enabled = false;
            unitScript.SetupPatrolWaypoints(patrolWaypoints);
            StartCoroutine(EnableUnitRoutine(unitScript, 0.3f));
        }
    }

    private IEnumerator EnableUnitRoutine(Unit unit, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (unit != null)
        {
            unit.enabled = true;
            unit.StartFirstPatrol();
        }
    }
}