using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class DataBreachFormController : MonoBehaviour
{
    [Header("Form Inputs")]
    public TMP_InputField inputDatumInbreuk;
    public Toggle toggleOnbekend;
    public TMP_InputField inputDatumOntdekt;
    public TMP_InputField inputOmschrijving;

    [Header("Navigation")]
    public MenuController menuController;

    // This runs when the script starts. It registers a listener on the unknown date toggle to handle input enabling/disabling dynamically.
    void Start()
    {
        // Tell the toggle to listen for clicks. When it's clicked, run the OnUnknownToggleChanged function!
        if (toggleOnbekend != null)
        {
            toggleOnbekend.onValueChanged.AddListener(OnUnknownToggleChanged);
        }
    }

    // This is called when the player clicks the 'Onbekend' (Unknown) checkbox. It enables or disables the date input field and clears typed text if checked.
    public void OnUnknownToggleChanged(bool isUnknown)
    {
        // If it IS unknown, make the input field non-interactable (greyed out)
        inputDatumInbreuk.interactable = !isUnknown;
        
        // If they check unknown, clear whatever date they might have started typing
        if (isUnknown)
        {
            inputDatumInbreuk.text = ""; 
        }
    }

    // This validates all inputs on submission (mandates descriptions and dates, unless date is checked unknown), registers the breach via SecurityManager, resets the form, and opens the home dashboard.
    public void OnSubmitClicked()
    {
        bool isUnknown = toggleOnbekend.isOn;
        string breachDate = inputDatumInbreuk.text;
        string discoveredDate = inputDatumOntdekt.text;
        string description = inputOmschrijving.text;

        // Check if mandatory fields are empty
        if (string.IsNullOrEmpty(discoveredDate) || string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("Vul a.u.b. de ontdekkingsdatum en omschrijving in!");
            return;
        }

        // If they didn't check 'Onbekend', they MUST provide a breach date
        if (!isUnknown && string.IsNullOrEmpty(breachDate))
        {
            Debug.LogWarning("Vul a.u.b. de datum van inbreuk in, of vink 'Onbekend' aan!");
            return;
        }

        // Save it!
        SecurityManager.Instance.SubmitDataBreach(isUnknown, breachDate, discoveredDate, description);

        // Clear the form
        inputDatumInbreuk.text = "";
        toggleOnbekend.isOn = false;
        inputDatumOntdekt.text = "";
        inputOmschrijving.text = "";

        // Go home
        if (menuController != null)
        {
            menuController.GoToHome();
        }
    }
}