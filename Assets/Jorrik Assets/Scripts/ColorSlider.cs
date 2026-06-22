using UnityEngine;
using UnityEngine.UI;

public class ColorSaver : MonoBehaviour
{
    [Header("Target Settings")]
    public Renderer targetRenderer;
    public string saveKey = "PlayerColor";

    [Header("UI Reference")]
    public Slider colorSlider;

    void Start()
    {
        // Default to 0 (which will now be White)
        float savedValue = PlayerPrefs.GetFloat(saveKey, 0f);

        if (colorSlider != null)
        {
            colorSlider.value = savedValue;
        }

        ApplyColor(savedValue);
    }

    public void UpdateColorFromSlider(float value)
    {
        ApplyColor(value);
        PlayerPrefs.SetFloat(saveKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyColor(float value)
    {
        if (targetRenderer == null) return;

        Color finalColor;

        // Logic: If slider is at the very beginning (0 to 0.05), make it White
        if (value < 0.05f)
        {
            finalColor = Color.white;
        }
        else
        {
            // Remap the rest of the slider (0.05 to 1.0) to the full color spectrum
            // This prevents the "double red" issue at the start
            float shiftedHue = (value - 0.05f) / 0.95f;
            finalColor = Color.HSVToRGB(shiftedHue, 0.8f, 0.9f);
        }

        targetRenderer.material.color = finalColor;
    }
}