using UnityEngine;
using TMPro;

public class SecurityFormController : MonoBehaviour
{
    [Header("Form Inputs")]
    public TMP_InputField inputDatum;
    public TMP_Dropdown dropdownSoort;
    public TMP_InputField inputKorteOmschrijving;
    public TMP_InputField inputUitgebreideBeschrijving;

    [Header("Navigation")]
    public MenuController menuController; 

    // This runs when the submit button is clicked on the security incident form. It reads the fields, validates that the date and short description are entered, submits the incident to the SecurityManager, resets all form inputs, and opens the home dashboard.
    public void OnSubmitClicked()
    {
        // 1. Read all the values from the UI
        string date = inputDatum.text;
        string category = dropdownSoort.options[dropdownSoort.value].text;
        string shortDesc = inputKorteOmschrijving.text;
        string longDesc = inputUitgebreideBeschrijving.text;

        // 2. Simple check to make sure they didn't leave it completely blank
        if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(shortDesc))
        {
            Debug.LogWarning("Vul a.u.b. een datum en korte omschrijving in!");
            return;
        }

        // 3. Send the data to the Manager to save it!
        SecurityManager.Instance.SubmitIncident(date, category, shortDesc, longDesc);

        // 4. Clear the form so it is empty for the next time
        inputDatum.text = "";
        dropdownSoort.value = 0; // Resets to the first option (Phishing)
        inputKorteOmschrijving.text = "";
        inputUitgebreideBeschrijving.text = "";

        // 5. Automatically send the player back to the home screen
        if (menuController != null)
        {
            menuController.GoToHome();
        }
    }
}