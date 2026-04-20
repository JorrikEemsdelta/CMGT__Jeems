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
    [Tooltip("Drag the specific body part/mesh that holds the face material here.")]
    public SkinnedMeshRenderer faceRenderer;
    [Tooltip("If the face is combined with the body, which material slot is the face? (Usually 0, 1, or 2)")]
    public int faceMaterialIndex = 0;
    
    [Space(5)]
    public Material normalFaceMaterial;
    public Material sadFaceMaterial;
    public Material happyFaceMaterial; // Optional!

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

    private Coroutine currentRouteCoroutine; 
    private Coroutine faceResetCoroutine; // Tracks the face emotion

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        ResetTimer();
    }

    void Update()
    {
        if (animator == null) return; 

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
        // 1. Apply the sad/happy face instantly
        ChangeFaceMaterialInstantly(emotionFace);

        // 2. Wait two frames to give the Animator time to receive the trigger and start moving out of Idle
        yield return null;
        yield return null;

        // 3. Keep waiting as long as the Animator is transitioning, OR is currently playing the Fail/Win animation
        while (animator.IsInTransition(0) || !animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName))
        {
            yield return null;
        }

        // 4. The exact moment we return to the Idle state, reset the face!
        ChangeFaceMaterialInstantly(normalFaceMaterial);
    }

    // ==========================================
    // QUIZ RESULTS (Win / Fail)
    // ==========================================

    public void TriggerRandomVictory()
    {
        if (animator == null) return; 
        if (animator.GetBool("isWalking")) return;

        animator.ResetTrigger("PlayFail");
        animator.ResetTrigger("PlayVictory");

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("VictoryIndex", randomAnimation);
        animator.SetTrigger("PlayVictory");

        // --- Trigger dynamic happy face ---
        if (happyFaceMaterial != null) 
        {
            if (faceResetCoroutine != null) StopCoroutine(faceResetCoroutine);
            faceResetCoroutine = StartCoroutine(EmotionTiedToAnimationRoutine(happyFaceMaterial));
        }
    }

    public void TriggerRandomFail()
    {
        if (animator == null) return;
        if (animator.GetBool("isWalking")) return;

        animator.ResetTrigger("PlayVictory");
        animator.ResetTrigger("PlayFail");

        int randomAnimation = Random.Range(0, 3);
        animator.SetInteger("FailIndex", randomAnimation);
        animator.SetTrigger("PlayFail");

        // --- Trigger dynamic sad face ---
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

        // Return face to normal instantly if they suddenly start walking
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