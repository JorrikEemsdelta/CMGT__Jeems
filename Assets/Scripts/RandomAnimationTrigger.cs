using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAnimationTrigger : MonoBehaviour
{
    private Animator animator;
    
    [Header("Timing Settings")]
    [Tooltip("Minimum time spent in Idle before a random action can occur.")]
    public float minIdleTime = 5f;
    [Tooltip("Maximum time spent in Idle before a random action can occur.")]
    public float maxIdleTime = 15f;
    
    [Header("State Names")]
    [Tooltip("The exact name of your Idle state in the Animator.")]
    public string idleStateName = "Idle";
    
    private float nextActionTime;

    // This runs when the script starts. It gets the Animator component and calculates when the first random animation should play.
    void Start()
    {
        animator = GetComponent<Animator>();
        ResetTimer();
    }

    // This runs every frame. If the character is fully in their idle state (not transitioning), it checks if enough time has passed to trigger a random animation. Otherwise, it resets the idle timer.
    void Update()
    {
        // 1. Check if we are in the Idle state AND not currently transitioning between states
        bool isIdling = animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName);
        bool isTransitioning = animator.IsInTransition(0);

        if (isIdling && !isTransitioning)
        {
            // 2. If we are fully idling, check if it's time to perform an action
            if (Time.time >= nextActionTime)
            {
                TriggerRandomAnimation();
            }
        }
        else
        {
            // 3. If we are waving, looking around, or transitioning, keep resetting the timer
            // so it doesn't trigger again the millisecond we return to Idle.
            ResetTimer();
        }
    }

    // This triggers a random animation (either waving or looking around, 50% chance each) and resets the timer for the next action.
    void TriggerRandomAnimation()
    {
        // Generate a random number between 0 and 100
        int chance = Random.Range(0, 100);

        // 50/50 split.
        if (chance < 50) 
        {
            animator.SetTrigger("isWaving");
        }
        else 
        {
            animator.SetTrigger("isLooking");
        }

        ResetTimer();
    }

    // This calculates a random point in the future (between minIdleTime and maxIdleTime) to determine when the next idle animation should trigger.
    void ResetTimer()
    {
        // Calculates a random point in the future based on your min/max settings.
        nextActionTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
    }
}