using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab; // Префаб NPC
    [SerializeField] private Transform spawnPoint; // Точка спавна
    [SerializeField] private Transform targetPoint; // Целевая точка движения
    [SerializeField] private Transform exitPoint; // Точка выхода

    private List<NPCController> activeNPCs = new List<NPCController>(); // Список активных NPC
    
    private void OnEnable()
    {
        QuestEventBus.spawnNPC += SpawnAndStartNPC;
    }

    private void OnDisable()
    {
        QuestEventBus.spawnNPC -= SpawnAndStartNPC;
    }
    
    // Метод для спавна и запуска NPC
    public void SpawnAndStartNPC()
    {
        if (npcPrefab == null || spawnPoint == null || targetPoint == null || exitPoint == null)
        {
            Debug.LogError("NPC prefab, spawn point, target point, or exit point not assigned!");
            return;
        }

        // Проверяем, что точка спавна находится на NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(spawnPoint.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogError($"Spawn point {spawnPoint.position} is not on NavMesh!");
            return;
        }

        // Спавним NPC
        GameObject npcInstance = Instantiate(npcPrefab, hit.position, spawnPoint.rotation);
        NPCController npc = npcInstance.GetComponent<NPCController>();
        if (npc != null)
        {
            // Инициализируем NPC
            npc.Initialize(targetPoint, exitPoint);
            activeNPCs.Add(npc);

            // Ждем один кадр для инициализации NavMeshAgent
            StartCoroutine(StartNPCBehavior(npc));
            Debug.Log($"Spawned NPC at {hit.position}");
        }
        else
        {
            Debug.LogError("NPC prefab does not have NPCController component!");
            Destroy(npcInstance);
        }
    }

    // Корутина для запуска поведения NPC
    private IEnumerator StartNPCBehavior(NPCController npc)
    {
        yield return null; // Ждем один кадр для инициализации NavMeshAgent
        npc.StartBehavior();
    }

    // Метод для уничтожения всех активных NPC
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

    // Метод для проверки, есть ли активные NPC
    public bool HasActiveNPCs()
    {
        activeNPCs.RemoveAll(npc => npc == null); // Удаляем уничтоженные NPC
        return activeNPCs.Count > 0;
    }
}