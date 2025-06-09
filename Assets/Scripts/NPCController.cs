using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using SojaExiles; // Для использования OpenCloseDoor

public class NPCController : MonoBehaviour
{
    [SerializeField] private Transform targetPoint; // Целевая точка движения
    [SerializeField] private Transform exitPoint; // Точка выхода
    [SerializeField] private float interactionDistance = 3f; // Дистанция для разговора
    [SerializeField] private float doorInteractionDistance = 2f; // Дистанция для взаимодействия с дверью

    // Фразы NPC
    private readonly string arrivalPhrase = "Эй, где мой кофе?";
    private readonly string[] coffeeReactions = new string[]
    {
        "Это что, пустой стакан?!", // 0%
        "Ты серьёзно? Это же четверть стакана!", // 25%
        "Наполовину? Мог бы и постараться!", // 50%
        "О, полный стакан! Спасибо!" // 100%
    };

    private NavMeshAgent agent;
    private bool isActive = false;
    private GameObject player;
    private bool hasReachedTarget = false;
    private bool isInteracting = false;

    // Инициализация NPC после спавна
    public void Initialize(Transform target, Transform exit)
    {
        targetPoint = target;
        exitPoint = exit;
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player with tag 'Player' not found!");
        }
    }

    // Запуск поведения NPC
    public void StartBehavior()
    {
        // Проверяем, что агент активен и находится на NavMesh
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent is not initialized or not on NavMesh!");
            return;
        }

        isActive = true;
        StartCoroutine(MoveToTarget());
    }

    // Корутина для движения к целевой точке
    IEnumerator MoveToTarget()
    {
        // Ждем один кадр для инициализации NavMeshAgent
        yield return null;

        // Устанавливаем целевую точку
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPoint.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogError($"Target point {targetPoint.position} is not on NavMesh!");
            yield break;
        }

        // Ждем, пока NPC дойдет до точки
        while (Vector3.Distance(transform.position, targetPoint.position) > 0.5f)
        {
            yield return null;
        }

        // Останавливаем NPC и поворачиваем к игроку
        agent.isStopped = true;
        hasReachedTarget = true;
        if (player != null)
        {
            transform.LookAt(player.transform);
        }
        Speak(arrivalPhrase);

        // Ждем взаимодействия (через Use)
        while (isActive && !isInteracting)
        {
            // Поддерживаем поворот к игроку
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= interactionDistance)
            {
                transform.LookAt(player.transform);
            }
            yield return null;
        }
    }

    // Метод для произнесения фразы
    private void Speak(string phrase)
    {
        Debug.Log($"NPC говорит: {phrase}"); // Замените на аудио/субтитры
    }

    // Взаимодействие с NPC (вызывается системой квестов)
    public void Use(float fillLevel)
    {
        if (!hasReachedTarget || !isActive || isInteracting) return;

        isInteracting = true;
        StartCoroutine(ProcessCoffee(fillLevel));
    }

    // Корутина для обработки кофе
    IEnumerator ProcessCoffee(float fillLevel)
    {
        // Преобразуем FillLevel (0–1) в ближайший процент (0, 0.25, 0.5, 1.0)
        float[] thresholds = { 0f, 0.25f, 0.5f, 1f };
        int reactionIndex = 0;
        float minDiff = Mathf.Abs(fillLevel - thresholds[0]);
        for (int i = 1; i < thresholds.Length; i++)
        {
            float diff = Mathf.Abs(fillLevel - thresholds[i]);
            if (diff < minDiff)
            {
                minDiff = diff;
                reactionIndex = i;
            }
        }

        Speak(coffeeReactions[reactionIndex]);

        // Если стакан не пустой, NPC уходит
        if (fillLevel > 0)
        {
            yield return StartCoroutine(ExitSequence());
        }
        else
        {
            isInteracting = false; // Разрешаем повторное взаимодействие
        }
    }

    // Корутина для ухода к точке выхода
    IEnumerator ExitSequence()
    {
        agent.isStopped = false;

        // Проверяем, что точка выхода на NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(exitPoint.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogError($"Exit point {exitPoint.position} is not on NavMesh!");
            yield break;
        }

        // Проверяем наличие двери на пути
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.5f)
        {
            // Ищем дверь поблизости
            Collider[] colliders = Physics.OverlapSphere(transform.position, doorInteractionDistance);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Door"))
                {
                    OpenCloseDoor door = collider.GetComponent<OpenCloseDoor>();
                    if (door != null && !door.open)
                    {
                        // Останавливаем NPC перед дверью
                        agent.isStopped = true;
                        door.Use(); // Открываем дверь
                        yield return new WaitForSeconds(0.5f); // Ждем анимацию
                        agent.isStopped = false; // Продолжаем движение
                    }
                }
            }
            yield return null;
        }

        // Уничтожаем NPC
        isActive = false;
        Destroy(gameObject);
    }
}