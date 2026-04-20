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
    [Header("Idle Randomizer Settings")]
    public float minIdleTime = 5f;
    public float maxIdleTime = 15f;
    public string idleStateName = "Idle";
    private float nextActionTime;

    [Space(10)]
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float turnSpeed = 5f; 

    [Space(10)]
    [Header("Route Setup")]
    public WalkRoute[] availableRoutes;

    // --- NEW: This specific variable tracks the active walking route so we can kill it instantly ---
    private Coroutine currentRouteCoroutine; 

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        ResetTimer();
    }

    void Update()
    {
        if (animator == null) return; 

        // --- PREVENT INTERRUPTION: Do not trigger random idles if walking ---
        if (animator.GetBool("isWalking")) 
        {
            ResetTimer();
            return; 
        }

        bool isIdling = animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName);
        bool isTransitioning = animator.IsInTransition(0);

        if (isIdling && !isTransitioning)
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
    // QUIZ RESULTS (Win / Fail)
    // ==========================================

    public void TriggerRandomVictory()
    {
        if (animator == null) return; 

        // --- PREVENT INTERRUPTION: Ignore victory command if currently walking ---
        if (animator.GetBool("isWalking")) return;

        animator.ResetTrigger("PlayFail");
        animator.ResetTrigger("PlayVictory");

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("VictoryIndex", randomAnimation);
        animator.SetTrigger("PlayVictory");
    }

    public void TriggerRandomFail()
    {
        if (animator == null) return;

        // --- PREVENT INTERRUPTION: Ignore fail command if currently walking ---
        if (animator.GetBool("isWalking")) return;

        animator.ResetTrigger("PlayVictory");
        animator.ResetTrigger("PlayFail");

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("FailIndex", randomAnimation);
        animator.SetTrigger("PlayFail");
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

        // --- INSTANT CANCEL: If a route is currently playing, kill it immediately ---
        if (currentRouteCoroutine != null)
        {
            StopCoroutine(currentRouteCoroutine);
        }

        // Start the new route and save it to our tracker variable
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

        // Route finished successfully! Turn off walking status.
        if (animator != null) animator.SetBool("isWalking", false);
        
        // Clear the active tracker
        currentRouteCoroutine = null;
    }
}