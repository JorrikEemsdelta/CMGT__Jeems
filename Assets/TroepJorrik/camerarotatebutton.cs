using System.Collections;
using UnityEngine;

public class camerarotatebutton : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Target camera to rotate. If null, Camera.main will be used.")]
    private Camera targetCamera = null;

    [SerializeField]
    [Tooltip("Duration in seconds for one 90-degree rotation")]
    private float rotateDuration = 0.5f;

    [SerializeField]
    [Tooltip("Easing curve for the rotation progress (x=time 0..1, y=progress 0..1). Default is EaseInOut.")]
    private AnimationCurve easeCurve = null;

    private Coroutine rotateCoroutine;

    // Public method to call from a UI Button OnClick
    public void RotateCamera90Y()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        Debug.Log($"camerarotatebutton.RotateCamera90Y called on '{gameObject.name}' targetCamera='{targetCamera.name}'");

        // If a rotation is already in progress, stop it so we start from the current rotation
        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);

        rotateCoroutine = StartCoroutine(RotateYCoroutine(90f));
    }

    // Public method to rotate 90 degrees to the left (counter-clockwise)
    public void RotateCamera90YLeft()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        Debug.Log($"camerarotatebutton.RotateCamera90YLeft called on '{gameObject.name}' targetCamera='{targetCamera.name}'");

        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);

        rotateCoroutine = StartCoroutine(RotateYCoroutine(-90f));
    }

    // Convenience explicit right rotation method
    public void RotateCamera90YRight()
    {
        RotateCamera90Y();
    }

    // Helper for testing: immediately rotate 90 degrees around world Y. Call from inspector to verify wiring.
    public void RotateImmediate90Y()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogWarning("RotateImmediate90Y: no target camera");
            return;
        }

        targetCamera.transform.Rotate(0f, 90f, 0f, Space.World);
        Debug.Log("RotateImmediate90Y performed");
    }

    private IEnumerator RotateYCoroutine(float deltaY)
    {
        Transform t = targetCamera.transform;

        Vector3 startEuler = t.eulerAngles;
        float startY = startEuler.y;
        float endY = startY + deltaY;

        float elapsed = 0f;

        // Ensure we have a usable easing curve
        if (easeCurve == null)
            easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // If duration is zero or negative, jump immediately
        if (rotateDuration <= 0f)
        {
            t.rotation = Quaternion.Euler(startEuler.x, endY, startEuler.z);
            yield break;
        }

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float frac = Mathf.Clamp01(elapsed / rotateDuration);
            float eased = easeCurve.Evaluate(frac);
            float y = Mathf.LerpAngle(startY, endY, eased);
            t.rotation = Quaternion.Euler(startEuler.x, y, startEuler.z);
            yield return null;
        }

        // Ensure final rotation is exact
        t.rotation = Quaternion.Euler(startEuler.x, endY, startEuler.z);
        rotateCoroutine = null;
    }
}
