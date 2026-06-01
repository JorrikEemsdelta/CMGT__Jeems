using System.Collections;
using UnityEngine;

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
    public SkinnedMeshRenderer faceRenderer;
    public int faceMaterialIndex = 0;
    public Material normalFaceMaterial;
    public Material sadFaceMaterial;
    public Material happyFaceMaterial;

    [Space(10)]
    [Header("Idle Randomizer Settings")]
    public float minIdleTime = 5f;
    public float maxIdleTime = 15f;
    public string idleStateName = "Idle";
    public string walkingStateName = "Walking";
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

    // This runs when the script is loaded. It connects to the Animator component and starts the idle timer.
    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        ResetTimer();
    }

    // This runs every frame. If the character is walking, it resets the idle timer. If idling, it triggers random animations like waving or looking around after a random delay.
    void Update()
    {
        if (animator == null) return;

        if (animator.GetBool("isWalking"))
        {
            ResetTimer();
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isIdling = stateInfo.IsName(idleStateName);
        
        // FIX: Explicitly check for both Talking states based on your Animator graph
        bool isTalking = stateInfo.IsName("Talking 1") || stateInfo.IsName("Talking 2");
        bool isTransitioning = animator.IsInTransition(0);

        if (isIdling && !isTalking && !isTransitioning)
        {
            if (Time.time >= nextActionTime)
            {
                TriggerRandomIdleAnimation();
            }
        }
        else if (!isIdling && !isTalking && !isTransitioning && !animator.GetBool("isWalking"))
        {
            ResetTimer();
        }
    }

    // ==========================================
    // TALKING LOGIC (PRIORITY FIX)
    // ==========================================
    // This interrupts idle animations and forces the animator to immediately play a random talking animation.
    public void TriggerRandomTalking()
    {
        if (animator == null || animator.GetBool("isWalking")) return;

        // Clear out any pending waves or looks
        ClearIdleTriggers();

        int randomTalk = Random.Range(1, 3);
        string targetState = "Talking " + randomTalk;

        // FIX: Force the animator to transition to the talking state immediately.
        // This overrides any Waving or Looking animation currently playing.
        animator.CrossFadeInFixedTime(targetState, 0.1f);
        
        // Keep the trigger set just in case your graph relies on it, but the CrossFade handles the heavy lifting
        animator.SetTrigger("PlayTalking" + randomTalk);

        // Reset the idle timer so they don't immediately wave the millisecond talking finishes
        ResetTimer();
    }

    // This makes the character play a random victory animation and temporarily changes their facial expression to happy.
    public void TriggerRandomVictory()
    {
        if (animator == null || animator.GetBool("isWalking")) return;
        ClearIdleTriggers();

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("VictoryIndex", randomAnimation);
        animator.SetTrigger("PlayVictory");

        if (happyFaceMaterial != null)
        {
            if (faceResetCoroutine != null) StopCoroutine(faceResetCoroutine);
            faceResetCoroutine = StartCoroutine(EmotionTiedToAnimationRoutine(happyFaceMaterial));
        }
    }

    // This makes the character play a random fail animation and temporarily changes their facial expression to sad.
    public void TriggerRandomFail()
    {
        if (animator == null || animator.GetBool("isWalking")) return;
        ClearIdleTriggers();

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("FailIndex", randomAnimation);
        animator.SetTrigger("PlayFail");

        if (sadFaceMaterial != null)
        {
            if (faceResetCoroutine != null) StopCoroutine(faceResetCoroutine);
            faceResetCoroutine = StartCoroutine(EmotionTiedToAnimationRoutine(sadFaceMaterial));
        }
    }

    // This triggers a random idle action (either waving or looking around) and resets the timer for the next idle action.
    private void TriggerRandomIdleAnimation()
    {
        if (animator == null || animator.GetBool("isWalking")) return;

        int chance = Random.Range(0, 100);
        if (chance < 50) animator.SetTrigger("isWaving");
        else animator.SetTrigger("isLooking");

        ResetTimer();
    }

    // This resets all animator triggers to ensure no old button presses or animations get queued up in the animator state machine.
    private void ClearIdleTriggers()
    {
        animator.ResetTrigger("isWaving");
        animator.ResetTrigger("isLooking");
        animator.ResetTrigger("PlayTalking1");
        animator.ResetTrigger("PlayTalking2");
        animator.ResetTrigger("PlayVictory");
        animator.ResetTrigger("PlayFail");
    }

    // This calculates a random point of time in the future to schedule the next random idle action.
    private void ResetTimer()
    {
        nextActionTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
    }

    // This stops any current walking route and starts a new walking route sequence from the available routes list.
    public void StartWalkingRoute(int routeIndex)
    {
        if (routeIndex < 0 || routeIndex >= availableRoutes.Length) return;

        if (currentRouteCoroutine != null) StopCoroutine(currentRouteCoroutine);
        if (faceResetCoroutine != null) StopCoroutine(faceResetCoroutine);
        
        ChangeFaceMaterialInstantly(normalFaceMaterial);
        currentRouteCoroutine = StartCoroutine(FollowRouteRoutine(availableRoutes[routeIndex]));
    }

    // This moves the character step-by-step through a list of waypoints, steering their rotation to look where they are walking, and returns them to idle when complete.
    private IEnumerator FollowRouteRoutine(WalkRoute route)
    {
        if (route.waypoints.Length == 0) yield break;

        ClearIdleTriggers();
        animator.SetBool("isWalking", true);
        animator.CrossFadeInFixedTime(walkingStateName, 0.1f);

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

                transform.position = Vector3.MoveTowards(transform.position, waypoint.position, walkSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = waypoint.position;
        }

        if (route.faceFinalWaypointDirection)
        {
            Transform finalWaypoint = route.waypoints[route.waypoints.Length - 1];
            Quaternion flatFinalRotation = Quaternion.Euler(0, finalWaypoint.eulerAngles.y, 0);
            while (Quaternion.Angle(transform.rotation, flatFinalRotation) > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, flatFinalRotation, turnSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = flatFinalRotation;
        }

        animator.SetBool("isWalking", false);
        currentRouteCoroutine = null;
    }

    // This changes the facial mesh texture immediately by swapping the material on the renderer at the designated index.
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

    // This temporarily displays an emotion face (like happy/sad) and resets it back to normal once the character goes back to their Idle state.
    private IEnumerator EmotionTiedToAnimationRoutine(Material emotionFace)
    {
        ChangeFaceMaterialInstantly(emotionFace);
        yield return new WaitForSeconds(0.1f);
        while (animator.IsInTransition(0) || !animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName))
        {
            yield return null;
        }
        ChangeFaceMaterialInstantly(normalFaceMaterial);
    }
}