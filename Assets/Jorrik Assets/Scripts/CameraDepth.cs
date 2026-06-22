using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Slightly pans the camera based on mouse position to create a depth/parallax effect for UI
public class CameraDepth : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Maximum horizontal and vertical offset (in world units) the camera can move from its start position")]
    private Vector2 maxOffset = new Vector2(0.5f, 0.3f);

    [SerializeField]
    [Tooltip("Global multiplier applied to the mouse input before applying maxOffset. Increase to make movement stronger.")]
    private float sensitivity = 1f;

    [SerializeField]
    [Tooltip("Separate multipliers for X and Y axis. Helpful to tune horizontal vs vertical strength.")]
    private Vector2 axisMultiplier = new Vector2(1f, 1f);

    [SerializeField]
    [Tooltip("Invert movement on the X or Y axis")]
    private bool invertX = false;

    [SerializeField]
    private bool invertY = false;

    [SerializeField]
    [Tooltip("How quickly the camera moves to follow the mouse (smaller is snappier)")]
    private float smoothTime = 0.12f;

    // If set, this camera's transform will be used. If left null, the script uses the transform this component is attached to.
    [SerializeField]
    private Camera targetCamera = null;

    private Vector3 initialLocalPosition;
    private Vector3 initialWorldPosition;
    private Vector3 velocityWorld = Vector3.zero;
    private Vector3 velocityLocal = Vector3.zero;

    [SerializeField]
    [Tooltip("Apply offsets to world position instead of localPosition. Useful if the camera is parented to another transform.")]
    private bool useWorldPosition = false;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Store the starting local and world positions so offsets are applied relative to them
        initialLocalPosition = transform.localPosition;
        initialWorldPosition = transform.position;
    }

    void LateUpdate()
    {
        // If there's no screen (e.g., running in headless mode) bail out
        if (Screen.width == 0 || Screen.height == 0)
            return;

        // Normalize mouse position around center to range [-1, 1]
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 mouse;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System
        if (Mouse.current != null)
            mouse = Mouse.current.position.ReadValue();
        else
            mouse = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#else
        // Legacy Input Manager
        mouse = Input.mousePosition;
#endif
        Vector2 normalized = (mouse - screenCenter) / screenCenter; // x,y in [-1,1]

        // Clamp to avoid extreme offsets if mouse goes far outside
        normalized = Vector2.ClampMagnitude(normalized, 1f);

        // Apply sensitivity, per-axis multipliers and inversion
        float inputX = normalized.x * sensitivity * axisMultiplier.x * (invertX ? -1f : 1f);
        float inputY = normalized.y * sensitivity * axisMultiplier.y * (invertY ? -1f : 1f);

        // Compute offset in camera space (so movement follows screen axes even if camera rotated)
        Vector3 camRight = targetCamera.transform.right;
        Vector3 camUp = targetCamera.transform.up;
        Vector3 offsetWorld = camRight * (inputX * maxOffset.x) + camUp * (inputY * maxOffset.y);

        // Compute target world position relative to the initial world position
        Vector3 targetWorldPos = initialWorldPosition + offsetWorld;

        // Smoothly move camera either in world or local coordinates
        if (useWorldPosition)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetWorldPos, ref velocityWorld, smoothTime);
        }
        else
        {
            // Convert desired world position to local position relative to parent
            Transform parent = transform.parent;
            Vector3 targetLocalPos = (parent != null) ? parent.InverseTransformPoint(targetWorldPos) : targetWorldPos;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetLocalPos, ref velocityLocal, smoothTime);
        }
    }

    // Public setters so other scripts or UI can tweak the behaviour at runtime if desired
    public void SetMaxOffset(Vector2 offset) => maxOffset = offset;
    public void SetSmoothTime(float time) => smoothTime = Mathf.Max(0.001f, time);
    public void SetSensitivity(float s) => sensitivity = s;
    public void SetAxisMultiplier(Vector2 m) => axisMultiplier = m;
    public void SetInvert(bool x, bool y) { invertX = x; invertY = y; }
    public void SetUseWorldPosition(bool useWorld) => useWorldPosition = useWorld;
}
