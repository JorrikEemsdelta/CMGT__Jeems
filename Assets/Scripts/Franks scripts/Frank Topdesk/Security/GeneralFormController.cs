using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq; 

public class GeneralFormController : MonoBehaviour
{
    [Header("Form Elements")]
    public ToggleGroup categoryToggleGroup;
    public TMP_InputField inputOmschrijving;

    [Header("Navigation")]
    public MenuController menuController;

    // This is called when the player clicks the submit button on the general report form. It finds which category toggle is active, extracts its text, validates that a description is filled in, submits the report to the GeneralReportManager, resets the form inputs, and navigates home.
    public void OnSubmitClicked()
    {
        // Safety check
        if (categoryToggleGroup == null)
        {
            Debug.LogError("ToggleGroup is missing in the Inspector!");
            return;
        }

        // 1. Find which toggle is currently turned on
        Toggle activeToggle = categoryToggleGroup.ActiveToggles().FirstOrDefault();
        string selectedCategory = "";
        
        if (activeToggle != null)
        {
            // --- THE FIX ---
            // Try to find a TextMeshPro label first
            TextMeshProUGUI tmpText = activeToggle.GetComponentInChildren<TextMeshProUGUI>();
            // If that fails, look for a standard Unity Text label
            Text normalText = activeToggle.GetComponentInChildren<Text>();

            if (tmpText != null)
            {
                selectedCategory = tmpText.text;
            }
            else if (normalText != null)
            {
                selectedCategory = normalText.text;
            }
            else
            {
                // Fallback just in case: use the name of the GameObject itself
                selectedCategory = activeToggle.name; 
            }
        }

        string description = inputOmschrijving != null ? inputOmschrijving.text : "";

        // 2. Validation Check
        if (string.IsNullOrEmpty(selectedCategory))
        {
            Debug.LogWarning("Selecteer a.u.b. een categorie uit de lijst!");
            return;
        }

        if (string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("Vul a.u.b. een omschrijving in!");
            return;
        }

        // 3. Save it to the database
        if (GeneralReportManager.Instance != null)
        {
            GeneralReportManager.Instance.SubmitReport(selectedCategory, description);
        }
        else
        {
            Debug.LogError("GeneralReportManager is niet in de scene gevonden!");
            return;
        }

        // 4. Clear the form for next time
        categoryToggleGroup.SetAllTogglesOff();
        if (inputOmschrijving != null) inputOmschrijving.text = "";

        // 5. Go back to Home Screen
        if (menuController != null)
        {
            menuController.GoToHome();
        }
    }
}