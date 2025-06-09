using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using SojaExiles; // ��� ������������� OpenCloseDoor

public class NPCController : MonoBehaviour
{
    [SerializeField] private Transform targetPoint; // ������� ����� ��������
    [SerializeField] private Transform exitPoint; // ����� ������
    [SerializeField] private float interactionDistance = 3f; // ��������� ��� ���������
    [SerializeField] private float doorInteractionDistance = 2f; // ��������� ��� �������������� � ������

    // ����� NPC
    private readonly string arrivalPhrase = "��, ��� ��� ����?";
    private readonly string[] coffeeReactions = new string[]
    {
        "��� ���, ������ ������?!", // 0%
        "�� ��������? ��� �� �������� �������!", // 25%
        "����������? ��� �� � �����������!", // 50%
        "�, ������ ������! �������!" // 100%
    };

    private NavMeshAgent agent;
    private bool isActive = false;
    private GameObject player;
    private bool hasReachedTarget = false;
    private bool isInteracting = false;

    // ������������� NPC ����� ������
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

    // ������ ��������� NPC
    public void StartBehavior()
    {
        // ���������, ��� ����� ������� � ��������� �� NavMesh
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent is not initialized or not on NavMesh!");
            return;
        }

        isActive = true;
        StartCoroutine(MoveToTarget());
    }

    // �������� ��� �������� � ������� �����
    IEnumerator MoveToTarget()
    {
        // ���� ���� ���� ��� ������������� NavMeshAgent
        yield return null;

        // ������������� ������� �����
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

        // ����, ���� NPC ������ �� �����
        while (Vector3.Distance(transform.position, targetPoint.position) > 0.5f)
        {
            yield return null;
        }

        // ������������� NPC � ������������ � ������
        agent.isStopped = true;
        hasReachedTarget = true;
        if (player != null)
        {
            transform.LookAt(player.transform);
        }
        Speak(arrivalPhrase);

        // ���� �������������� (����� Use)
        while (isActive && !isInteracting)
        {
            // ������������ ������� � ������
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= interactionDistance)
            {
                transform.LookAt(player.transform);
            }
            yield return null;
        }
    }

    // ����� ��� ������������ �����
    private void Speak(string phrase)
    {
        Debug.Log($"NPC �������: {phrase}"); // �������� �� �����/��������
    }

    // �������������� � NPC (���������� �������� �������)
    public void Use(float fillLevel)
    {
        if (!hasReachedTarget || !isActive || isInteracting) return;

        isInteracting = true;
        StartCoroutine(ProcessCoffee(fillLevel));
    }

    // �������� ��� ��������� ����
    IEnumerator ProcessCoffee(float fillLevel)
    {
        // ����������� FillLevel (0�1) � ��������� ������� (0, 0.25, 0.5, 1.0)
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

        // ���� ������ �� ������, NPC ������
        if (fillLevel > 0)
        {
            yield return StartCoroutine(ExitSequence());
        }
        else
        {
            isInteracting = false; // ��������� ��������� ��������������
        }
    }

    // �������� ��� ����� � ����� ������
    IEnumerator ExitSequence()
    {
        agent.isStopped = false;

        // ���������, ��� ����� ������ �� NavMesh
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

        // ��������� ������� ����� �� ����
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.5f)
        {
            // ���� ����� ����������
            Collider[] colliders = Physics.OverlapSphere(transform.position, doorInteractionDistance);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Door"))
                {
                    OpenCloseDoor door = collider.GetComponent<OpenCloseDoor>();
                    if (door != null && !door.open)
                    {
                        // ������������� NPC ����� ������
                        agent.isStopped = true;
                        door.Use(); // ��������� �����
                        yield return new WaitForSeconds(0.5f); // ���� ��������
                        agent.isStopped = false; // ���������� ��������
                    }
                }
            }
            yield return null;
        }

        // ���������� NPC
        isActive = false;
        Destroy(gameObject);
    }
}