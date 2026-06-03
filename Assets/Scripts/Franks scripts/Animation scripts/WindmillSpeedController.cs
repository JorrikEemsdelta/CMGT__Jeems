using UnityEngine;

public class WindmillSpeedController : MonoBehaviour
{
    private Animator anim;

    [Header("Global Constraints")]
    [SerializeField] private float minGlobal = 0.1f;
    [SerializeField] private float maxGlobal = 0.8f;
    [SerializeField] private float variance = 0.2f;

    [Header("Calculated Speed")]
    [SerializeField] private float currentSpeed;

    // Static variables are shared by ALL windmills using this script
    private static float sharedBaseSpeed;
    private static bool hasGeneratedBase = false;

    // This runs when the windmill object starts. It gets the Animator component, establishes a shared base speed across all windmills (only done once), adds a small individual speed variance, rounds the speed, sets the animator's speed, and plays the animation at a random starting frame to look natural.
    void Start()
    {
        anim = GetComponent<Animator>();

        if (anim != null)
        {
            // 1. Only the first windmill to run Start() picks the group speed
            if (!hasGeneratedBase)
            {
                sharedBaseSpeed = Random.Range(minGlobal, maxGlobal);
                hasGeneratedBase = true;
            }

            // 2. Define the localized range (clamped by global limits)
            float localMin = Mathf.Max(minGlobal, sharedBaseSpeed - (variance / 2));
            float localMax = Mathf.Min(maxGlobal, sharedBaseSpeed + (variance / 2));

            // 3. Pick individual speed within that .2 range
            float rawSpeed = Random.Range(localMin, localMax);

            // 4. Round to 2 decimal places
            currentSpeed = Mathf.Round(rawSpeed * 100f) / 100f;

            anim.speed = currentSpeed;
            
            // Randomize start frame
            anim.Play(0, -1, Random.value);
        }
    }

    // This runs when the windmill object is destroyed. It resets the shared base speed generation flag so new base speeds can be generated when the scene reloads.
    private void OnDestroy()
    {
        hasGeneratedBase = false;
    }
}