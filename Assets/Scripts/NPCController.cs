using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using SojaExiles; // ??? ????????????? OpenCloseDoor

public class NPCController : MonoBehaviour
{
    [SerializeField] private Transform targetPoint; // ??????? ????? ????????
    [SerializeField] private Transform exitPoint; // ????? ??????
    [SerializeField] private float interactionDistance = 3f; // ????????? ??? ?????????
    [SerializeField] private float doorInteractionDistance = 3f; // ????????? ??? ?????????????? ? ??????
    [SerializeField] private float maxNavMeshDistance = 5f; // ???????????? ?????????? ??? ?????? ????? ?? NavMesh
    [SerializeField] private float moveSpeed = 2.5f; // ???????? ???????? NPC
    [SerializeField] private AudioClip FootstepAudioClip; // ???? ????? ??? ????????

    public AudioClip startTalk;
    public List<AudioClip> coffeeReactions;
    
    
    private NavMeshAgent agent;
    private Animator animator; // ???????? NPC
    private bool isActive = false;
    private GameObject player;
    private bool hasReachedTarget = false;
    private bool isInteracting = false;

    // ????????????? NPC ????? ??????
    public void Initialize(Transform target, Transform exit)
    {
        targetPoint = target;
        exitPoint = exit;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player with tag 'Player' not found!");
        }
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on NPC!");
            return;
        }
        if (animator == null)
        {
            Debug.LogWarning($"Animator component not found on NPC {gameObject.name}! No animations will play.");
        }
        if (FootstepAudioClip == null)
        {
            Debug.LogWarning($"FootstepAudioClip not assigned on NPC {gameObject.name}! No footstep sounds will play.");
        }

        // ????????? ???????? ??????
        agent.speed = moveSpeed;
        agent.angularSpeed = 120f; // ??? ???????? ????????
        agent.stoppingDistance = 0.5f; // ???????? ?????????
    }

    // ?????? ????????? NPC
    public void StartBehavior()
    {
        // ?????????, ??? ????? ??????? ? ????????? ?? NavMesh
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError($"NavMeshAgent on {gameObject.name} is not initialized or not on NavMesh!");
            return;
        }

        isActive = true;
        StartCoroutine(MoveToTarget());
    }

    // ???????? ??? ???????? ? ??????? ?????
    IEnumerator MoveToTarget()
    {
        // ???? ???? ???? ??? ????????????? NavMeshAgent
        yield return null;

        // ?????????, ??? targetPoint ?? NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(targetPoint.position, out hit, maxNavMeshDistance, NavMesh.AllAreas))
        {
            Debug.LogError($"Target point {targetPoint.position} is not on NavMesh within {maxNavMeshDistance}m!");
            yield break;
        }

        // ????????????? ??????? ????? ? ???????? ???????? ??????
        agent.SetDestination(hit.position);
        SetAnimationState(true);
        Debug.Log($"NPC {gameObject.name} moving to target point {hit.position} with animation.");

        // ????????? ??????? ????? ?? ????
        bool doorOpened = false;
        while (Vector3.Distance(transform.position, targetPoint.position) > 0.5f)
        {
            // ???? ????? ??????????
            Collider[] colliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.5f, doorInteractionDistance, LayerMask.GetMask("Ground"));
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Door"))
                {
                    OpenCloseDoor door = collider.GetComponent<OpenCloseDoor>();
                    if (door != null && !door.open)
                    {
                        // ????????????? NPC ? ????????
                        agent.isStopped = true;
                        SetAnimationState(false);
                        Debug.Log($"NPC {gameObject.name} detected door {collider.gameObject.name} on way to target, opening it.");
                        door.Use(); // ????????? ?????
                        yield return new WaitForSeconds(0.6f); // ???? ???????? + ?????
                        agent.isStopped = false;
                        SetAnimationState(true); // ???????????? ???????? ??????
                        doorOpened = true;

                        // ????????? ???? ????? ???????? ?????
                        if (NavMesh.SamplePosition(targetPoint.position, out hit, maxNavMeshDistance, NavMesh.AllAreas))
                        {
                            agent.SetDestination(hit.position);
                            Debug.Log($"NPC {gameObject.name} updated path to {hit.position} after opening door to target.");
                        }
                    }
                }
            }

            // ????????? ???????????? ? ???????? ??????
            if (!doorOpened)
            {
                RaycastHit rayHit;
                Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
                Vector3 rayDirection = agent.velocity.normalized != Vector3.zero ? agent.velocity.normalized : transform.forward;
                if (Physics.Raycast(rayOrigin, rayDirection, out rayHit, doorInteractionDistance, LayerMask.GetMask("Ground")))
                {
                    if (rayHit.collider.CompareTag("Door"))
                    {
                        OpenCloseDoor door = rayHit.collider.GetComponent<OpenCloseDoor>();
                        if (door != null && !door.open)
                        {
                            agent.isStopped = true;
                            SetAnimationState(false);
                            Debug.Log($"NPC {gameObject.name} hit closed door {rayHit.collider.gameObject.name} on way to target, opening it.");
                            door.Use();
                            yield return new WaitForSeconds(0.6f);
                            agent.isStopped = false;
                            SetAnimationState(true);
                            doorOpened = true;

                            // ????????? ????
                            if (NavMesh.SamplePosition(targetPoint.position, out hit, maxNavMeshDistance, NavMesh.AllAreas))
                            {
                                agent.SetDestination(hit.position);
                                Debug.Log($"NPC {gameObject.name} updated path to {hit.position} after hitting door to target.");
                            }
                        }
                    }
                }
            }

            // ????????? ????
            if (doorOpened)
            {
                NavMeshPath path = new NavMeshPath();
                if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                {
                    Debug.LogWarning($"NPC {gameObject.name} cannot find a complete path to {hit.position} after opening door to target!");
                }
            }

            yield return null;
        }

        // ????????????? NPC ? ???????? ???????? ???????
        agent.isStopped = true;
        SetAnimationState(false);
        hasReachedTarget = true;
        if (player != null)
        {
            transform.LookAt(player.transform);
        }
        Speak(startTalk);
        QuestEventBus.GetDoneAction("Client");
        // ???? ?????????????? (????? Use)
        while (isActive && !isInteracting)
        {
            // ???????????? ??????? ? ??????
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= interactionDistance)
            {
                transform.LookAt(player.transform);
            }
            yield return null;
        }
    }

    // ????? ??? ?????????? ?????????
    private void SetAnimationState(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
            Debug.Log($"NPC {gameObject.name} animation state: isWalking = {isWalking}");
        }
    }

    // ????? ??? ????????? ?????? ?????
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(FootstepAudioClip, transform.position, 0.5f);
                Debug.Log($"NPC {gameObject.name} played footstep sound at {transform.position}");
            }
            else
            {
                Debug.LogWarning($"FootstepAudioClip is null on NPC {gameObject.name}!");
            }
        }
    }

    // ????? ??? ???????????? ?????
    private void Speak(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip,transform.position);
    }

    // ?????????????? ? NPC (?????????? ???????? ???????)
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cup") || other.gameObject.CompareTag("Lid"))
        {
            var C = other.transform.GetComponent<CoffeeFillSingleParticle>();
            ProcessCoffee(C.FillLevel);
            Destroy(C.gameObject);
        }
    }

    // ???????? ??? ????????? ????
    private void ProcessCoffee(float fillLevel)
    {
        // ??????????? FillLevel (0–1) ? ????????? ??????? (0, 0.25, 0.5, 1.0)
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

        // ???? ?????? ?? ??????, NPC ??????
        if (fillLevel > 0)
        {
           StartCoroutine(ExitSequence());
           QuestEventBus.GetDoneAction("Coffee");
        }
        else
        {
            isInteracting = false; // ????????? ????????? ??????????????
        }
    }

    // ???????? ??? ????? ? ????? ??????
    IEnumerator ExitSequence()
    {
        yield return new WaitForSeconds(2);
        // ?????????, ??? ????? ?????? ?? NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(exitPoint.position, out hit, maxNavMeshDistance, NavMesh.AllAreas))
        {
            Debug.LogError($"Exit point {exitPoint.position} is not on NavMesh within {maxNavMeshDistance}m!");
            yield break;
        }

        agent.isStopped = false;
        agent.SetDestination(hit.position);
        SetAnimationState(true); // ???????? ???????? ??????
        Debug.Log($"NPC {gameObject.name} moving to exit point {hit.position} with animation.");

        // ????????? ??????? ????? ?? ????
        bool doorOpened = false;
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.5f)
        {
            // ???? ????? ??????????
            Collider[] colliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.5f, doorInteractionDistance, LayerMask.GetMask("Ground"));
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Door"))
                {
                    OpenCloseDoor door = collider.GetComponent<OpenCloseDoor>();
                    if (door != null && !door.open)
                    {
                        // ????????????? NPC ? ????????
                        agent.isStopped = true;
                        SetAnimationState(false);
                        Debug.Log($"NPC {gameObject.name} detected door {collider.gameObject.name} on way to exit, opening it.");
                        door.Use(); // ????????? ?????
                        yield return new WaitForSeconds(0.6f); // ???? ???????? + ?????
                        agent.isStopped = false;
                        SetAnimationState(true); // ???????????? ???????? ??????
                        doorOpened = true;

                        // ????????? ???? ????? ???????? ?????
                        if (NavMesh.SamplePosition(exitPoint.position, out hit, maxNavMeshDistance, NavMesh.AllAreas))
                        {
                            agent.SetDestination(hit.position);
                            Debug.Log($"NPC {gameObject.name} updated path to {hit.position} after opening door to exit.");
                        }
                    }
                }
            }

            // ????????? ???????????? ? ???????? ??????
            if (!doorOpened)
            {
                RaycastHit rayHit;
                Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
                Vector3 rayDirection = agent.velocity.normalized != Vector3.zero ? agent.velocity.normalized : transform.forward;
                if (Physics.Raycast(rayOrigin, rayDirection, out rayHit, doorInteractionDistance, LayerMask.GetMask("Ground")))
                {
                    if (rayHit.collider.CompareTag("Door"))
                    {
                        OpenCloseDoor door = rayHit.collider.GetComponent<OpenCloseDoor>();
                        if (door != null && !door.open)
                        {
                            agent.isStopped = true;
                            SetAnimationState(false);
                            Debug.Log($"NPC {gameObject.name} hit closed door {rayHit.collider.gameObject.name} on way to exit, opening it.");
                            door.Use();
                            yield return new WaitForSeconds(0.6f);
                            agent.isStopped = false;
                            SetAnimationState(true);
                            doorOpened = true;

                            // ????????? ????
                            if (NavMesh.SamplePosition(exitPoint.position, out hit, maxNavMeshDistance, NavMesh.AllAreas))
                            {
                                agent.SetDestination(hit.position);
                                Debug.Log($"NPC {gameObject.name} updated path to {hit.position} after hitting door to exit.");
                            }
                        }
                    }
                }
            }

            // ????????? ????
            if (doorOpened)
            {
                NavMeshPath path = new NavMeshPath();
                if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                {
                    Debug.LogWarning($"NPC {gameObject.name} cannot find a complete path to {hit.position} after opening door to exit!");
                }
            }

            yield return null;
        }

        // ????????????? NPC ? ???????? ????? ????????????
        agent.isStopped = true;
        SetAnimationState(false);
        isActive = false;
        Debug.Log($"NPC {gameObject.name} reached exit point and is destroyed.");
        Destroy(gameObject);
    }
    
    public void TriggerMetamorphosis(bool toMonster)
    {
        // ?????? ????? ??????
        // ????????: 
        // normalModel.SetActive(!toMonster);
        // horrorModel.SetActive(toMonster);
    }
}