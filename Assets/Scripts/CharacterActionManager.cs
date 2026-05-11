using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Required for the New Input System

[System.Serializable]
public class WalkRoute
{
    public string routeName = "New Route";
    [Tooltip("Drag your waypoint empty GameObjects here in the order the character should walk them.")]
    public Transform[] waypoints;

    [Space(5)]
    [Tooltip("If true, the character will turn to match the blue arrow (Forward Z-axis) of the FINAL waypoint when they stop.")]
    public bool faceFinalWaypointDirection = true;
}

[RequireComponent(typeof(Animator))]
public class CharacterActionManager : MonoBehaviour
{
    [Header("Core References")]
    public Animator animator;

    [Space(10)]
    [Header("Face Emotions")]
    [Tooltip("Drag the specific body part/mesh that holds the face material here.")]
    public SkinnedMeshRenderer faceRenderer;
    [Tooltip("If the face is combined with the body, which material slot is the face? (Usually 0, 1, or 2)")]
    public int faceMaterialIndex = 0;

    [Space(5)]
    public Material normalFaceMaterial;
    public Material sadFaceMaterial;
    public Material happyFaceMaterial;

    [Space(10)]
    [Header("Idle Randomizer Settings")]
    public float minIdleTime = 5f;
    public float maxIdleTime = 15f;
    public string idleStateName = "Idle";
    [Tooltip("The name of your Talking state in the Animator to prevent idle overlaps.")]
    public string talkingStateName = "Talking";
    private float nextActionTime;

    [Space(10)]
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float turnSpeed = 5f;

    [Space(10)]
    [Header("Route Setup")]
    public WalkRoute[] availableRoutes;

    private Coroutine currentRouteCoroutine;
    private Coroutine faceResetCoroutine;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        ResetTimer();
    }

    void Update()
    {
        if (animator == null) return;



        // If walking, we don't process random idles
        if (animator.GetBool("isWalking"))
        {
            ResetTimer();
            return;
        }

        bool isIdling = animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName);
        bool isTalking = animator.GetCurrentAnimatorStateInfo(0).IsName(talkingStateName);
        bool isTransitioning = animator.IsInTransition(0);

        // Only trigger random idle animations if we are in the Idle state and not already talking
        if (isIdling && !isTalking && !isTransitioning)
        {
            if (Time.time >= nextActionTime)
            {
                TriggerRandomIdleAnimation();
            }
        }
        else
        {
            ResetTimer();
        }
    }

    // ==========================================
    // TALKING LOGIC
    // ==========================================

    public void TriggerRandomTalking()
    {
        if (animator == null || animator.GetBool("isWalking")) return;

        // Reset triggers to prevent queuing
        animator.ResetTrigger("PlayTalking1");
        animator.ResetTrigger("PlayTalking2");

        // Randomly choose between 1 and 2
        int randomTalk = Random.Range(1, 3);
        animator.SetTrigger("PlayTalking" + randomTalk);
    }

    // ==========================================
    // DYNAMIC EMOTION LOGIC
    // ==========================================

    private void ChangeFaceMaterialInstantly(Material newFace)
    {
        if (faceRenderer == null || newFace == null) return;

        Material[] mats = faceRenderer.materials;
        if (faceMaterialIndex >= 0 && faceMaterialIndex < mats.Length)
        {
            mats[faceMaterialIndex] = newFace;
            faceRenderer.materials = mats;
        }
    }


    private IEnumerator EmotionTiedToAnimationRoutine(Material emotionFace)
    {
        ChangeFaceMaterialInstantly(emotionFace);

        yield return null;
        yield return null;

        while (animator.IsInTransition(0) || !animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName))
        {
            yield return null;
        }

        ChangeFaceMaterialInstantly(normalFaceMaterial);
    }

    // ==========================================
    // QUIZ RESULTS (Win / Fail)
    // ==========================================

    public void TriggerRandomVictory()
    {
        if (animator == null || animator.GetBool("isWalking")) return;

        animator.ResetTrigger("PlayFail");
        animator.ResetTrigger("PlayVictory");

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("VictoryIndex", randomAnimation);
        animator.SetTrigger("PlayVictory");

        if (happyFaceMaterial != null)
        {
            if (faceResetCoroutine != null) StopCoroutine(faceResetCoroutine);
            faceResetCoroutine = StartCoroutine(EmotionTiedToAnimationRoutine(happyFaceMaterial));
        }
    }

    public void TriggerRandomFail()
    {
        if (animator == null || animator.GetBool("isWalking")) return;

        animator.ResetTrigger("PlayVictory");
        animator.ResetTrigger("PlayFail");

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("FailIndex", randomAnimation);
        animator.SetTrigger("PlayFail");

        if (sadFaceMaterial != null)
        {
            if (faceResetCoroutine != null) StopCoroutine(faceResetCoroutine);
            faceResetCoroutine = StartCoroutine(EmotionTiedToAnimationRoutine(sadFaceMaterial));
        }
    }

    // ==========================================
    // RANDOM IDLE ACTIONS
    // ==========================================

    private void TriggerRandomIdleAnimation()
    {
        if (animator == null) return;

        int chance = Random.Range(0, 100);

        if (chance < 50) animator.SetTrigger("isWaving");
        else animator.SetTrigger("isLooking");

        ResetTimer();
    }

    private void ResetTimer()
    {
        nextActionTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
    }

    // ==========================================
    // MOVEMENT & ROUTES
    // ==========================================

    public void StartWalkingRoute(int routeIndex)
    {
        if (routeIndex < 0 || routeIndex >= availableRoutes.Length) return;

        if (currentRouteCoroutine != null)
        {
            StopCoroutine(currentRouteCoroutine);
        }

        if (faceResetCoroutine != null) StopCoroutine(faceResetCoroutine);
        ChangeFaceMaterialInstantly(normalFaceMaterial);

        currentRouteCoroutine = StartCoroutine(FollowRouteRoutine(availableRoutes[routeIndex]));
    }

    private IEnumerator FollowRouteRoutine(WalkRoute route)
    {
        if (route.waypoints.Length == 0) yield break;

        if (animator != null) animator.SetBool("isWalking", true);

        foreach (Transform waypoint in route.waypoints)
        {
            if (waypoint == null) continue;

            while (Vector3.Distance(transform.position, waypoint.position) > 0.05f)
            {
                Vector3 direction = (waypoint.position - transform.position).normalized;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    waypoint.position,
                    walkSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = waypoint.position;
        }

        if (route.faceFinalWaypointDirection)
        {
            Transform finalWaypoint = route.waypoints[route.waypoints.Length - 1];
            Vector3 finalEuler = finalWaypoint.eulerAngles;
            Quaternion flatFinalRotation = Quaternion.Euler(0, finalEuler.y, 0);

            while (Quaternion.Angle(transform.rotation, flatFinalRotation) > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, flatFinalRotation, turnSpeed * Time.deltaTime);
                yield return null;
            }

            transform.rotation = flatFinalRotation;
        }

        if (animator != null) animator.SetBool("isWalking", false);
        currentRouteCoroutine = null;
    }
}