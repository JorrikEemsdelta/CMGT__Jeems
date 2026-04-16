using System.Collections;
using UnityEngine;

public class CameraRotateButton : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private float rotateDuration = 0.8f; // slightly longer = smoother

    [SerializeField]
    private AnimationCurve easeCurve;

    private Coroutine rotateCoroutine;

    private void Start()
    {
        
        if (easeCurve == null)
            easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    public void RotateCamera90Right()
    {
        StartRotation(90f);
    }

    public void RotateCamera90Left()
    {
        StartRotation(-90f);
    }

    private void StartRotation(float angle)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        // Stop current rotation cleanly
        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);

        rotateCoroutine = StartCoroutine(RotateCoroutine(angle));
    }

    private IEnumerator RotateCoroutine(float deltaY)
    {
        Transform t = targetCamera.transform;

        Quaternion startRot = t.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, deltaY, 0f);

        float elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(elapsed / rotateDuration);

            float eased = easeCurve.Evaluate(t01);

            // Smooth rotation
            t.rotation = Quaternion.Slerp(startRot, endRot, eased);

            yield return null;
        }

        // Snap exactly to final rotation (prevents drift)
        t.rotation = endRot;

        rotateCoroutine = null;
    }
}