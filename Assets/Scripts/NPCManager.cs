using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab; // ������ NPC
    [SerializeField] private Transform spawnPoint; // ����� ������
    [SerializeField] private Transform targetPoint; // ������� ����� ��������
    [SerializeField] private Transform exitPoint; // ����� ������

    private List<NPCController> activeNPCs = new List<NPCController>(); // ������ �������� NPC
    
    private void OnEnable()
    {
        QuestEventBus.spawnNPC += SpawnAndStartNPC;
    }

    private void OnDisable()
    {
        QuestEventBus.spawnNPC -= SpawnAndStartNPC;
    }
    
    // ����� ��� ������ � ������� NPC
    public void SpawnAndStartNPC()
    {
        if (npcPrefab == null || spawnPoint == null || targetPoint == null || exitPoint == null)
        {
            Debug.LogError("NPC prefab, spawn point, target point, or exit point not assigned!");
            return;
        }

        // ���������, ��� ����� ������ ��������� �� NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(spawnPoint.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogError($"Spawn point {spawnPoint.position} is not on NavMesh!");
            return;
        }

        // ������� NPC
        GameObject npcInstance = Instantiate(npcPrefab, hit.position, spawnPoint.rotation);
        NPCController npc = npcInstance.GetComponent<NPCController>();
        if (npc != null)
        {
            // �������������� NPC
            npc.Initialize(targetPoint, exitPoint);
            activeNPCs.Add(npc);

            // ���� ���� ���� ��� ������������� NavMeshAgent
            StartCoroutine(StartNPCBehavior(npc));
            Debug.Log($"Spawned NPC at {hit.position}");
        }
        else
        {
            Debug.LogError("NPC prefab does not have NPCController component!");
            Destroy(npcInstance);
        }
    }

    // �������� ��� ������� ��������� NPC
    private IEnumerator StartNPCBehavior(NPCController npc)
    {
        yield return null; // ���� ���� ���� ��� ������������� NavMeshAgent
        npc.StartBehavior();
    }

    // ����� ��� ����������� ���� �������� NPC
    public void DestroyAllNPCs()
    {
        foreach (var npc in activeNPCs)
        {
            if (npc != null)
            {
                Destroy(npc.gameObject);
            }
        }
        activeNPCs.Clear();
        Debug.Log("All NPCs destroyed");
    }

    // ����� ��� ��������, ���� �� �������� NPC
    public bool HasActiveNPCs()
    {
        activeNPCs.RemoveAll(npc => npc == null); // ������� ������������ NPC
        return activeNPCs.Count > 0;
    }
}