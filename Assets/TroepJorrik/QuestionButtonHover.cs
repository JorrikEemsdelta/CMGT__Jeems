using System.Collections;
using UnityEngine;
// This is the extra line needed for the new system
using UnityEngine.InputSystem; 

public class QuestionButtonHover : MonoBehaviour
{
    [Header("Scene Transition")]
    // Instead of loading a scene, we toggle UI GameObjects
    public GameObject uiToDisable;
    public GameObject uiToEnable;
    [Header("Fade Settings")] 
    public float fadeDuration = 0.25f;

    [Header("Shader Settings")]
    public string shaderReferenceName = "_outline_scale"; 
    public float activeScale = 1.1f;

    private MeshRenderer meshRenderer;
    private Coroutine fadeCoroutine;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetOutlineScale(0f);
    }

    void Update()
    {
        // 1. Get mouse position in the New Input System
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        // 2. Create the ray
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == this.transform)
            {
                SetOutlineScale(activeScale);

                // 3. Check for left-click in the New Input System
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    LoadTargetScene();
                }
            }
            else
            {
                SetOutlineScale(0f);
            }
        }
        else
        {
            SetOutlineScale(0f);
        }
    }

    void SetOutlineScale(float value)
    {
        if (meshRenderer != null && meshRenderer.materials.Length > 1)
        {
            // Update the material in the second slot
            meshRenderer.materials[1].SetFloat(shaderReferenceName, value);
        }
    }

    void LoadTargetScene()
    {
        bool enableIsActive = uiToEnable != null && uiToEnable.activeSelf;

        if (enableIsActive)
        {
            // If currently fading in, stop it
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (uiToEnable != null) uiToEnable.SetActive(false);
            if (uiToDisable != null) uiToDisable.SetActive(true);
        }
        else
        {
            if (uiToDisable != null) uiToDisable.SetActive(false);

            if (uiToEnable != null)
            {
                // Ensure the UI is active so CanvasGroup/graphics are visible for fading
                uiToEnable.SetActive(true);

                // Start fade-in coroutine, replacing any existing one
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeInUI(uiToEnable, fadeDuration));
            }
        }
    }

    IEnumerator FadeInUI(GameObject target, float duration)
    {
        if (target == null) yield break;

        // Try to get or add a CanvasGroup for fading
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = target.AddComponent<CanvasGroup>();
        }

        cg.interactable = false;
        cg.blocksRaycasts = false;

        float elapsed = 0f;
        cg.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        fadeCoroutine = null;
    }
}