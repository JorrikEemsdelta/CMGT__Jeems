using UnityEngine;
// This is the extra line needed for the new system
using UnityEngine.InputSystem; 

public class QuestionButtonHover : MonoBehaviour
{
    [Header("Scene Transition")]
    // Instead of loading a scene, we toggle UI GameObjects
    public GameObject uiToDisable;
    public GameObject uiToEnable;

    [Header("Shader Settings")]
    public string shaderReferenceName = "_outline_scale"; 
    public float activeScale = 1.1f;

    private MeshRenderer meshRenderer;

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
            if (uiToEnable != null) uiToEnable.SetActive(false);
            if (uiToDisable != null) uiToDisable.SetActive(true);
        }
        else
        {
            if (uiToEnable != null) uiToEnable.SetActive(true);
            if (uiToDisable != null) uiToDisable.SetActive(false);
        }
    }
}