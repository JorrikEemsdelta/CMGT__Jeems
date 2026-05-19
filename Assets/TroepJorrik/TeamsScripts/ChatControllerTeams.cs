using UnityEngine;
using TMPro;

public class ChatControllerTeams : MonoBehaviour
{
    [Header("Invoer en UI Elementen")]
    public TMP_InputField chatInputField;
    public TextMeshProUGUI messageHistoryText;
    public TextMeshProUGUI chatHeaderTitle;
    public TaskManager taskManager;

    [Header("Tabbladen Panelen")]
    public GameObject chatInhoudPanel;       // Het paneel met de chatberichten (ScrollView)
    public GameObject bestandenInhoudPanel;  // Het paneel met de bestandenlijst

    // Deze variabele onthoudt de link binnen het spel (WebGL-veilig)
    private string internKlembord = "";

    void Update()
    {
        // We controleren elke frame of de speler in de chatbalk aan het typen is
        if (chatInputField != null && chatInputField.isFocused)
        {
            // Het nieuwe Input System equivalent voor: Control + V
            if (UnityEngine.InputSystem.Keyboard.current.ctrlKey.isPressed &&
                UnityEngine.InputSystem.Keyboard.current.vKey.wasPressedThisFrame)
            {
                // Als er iets in ons interne klembord staat, plakken we het erin
                if (!string.IsNullOrEmpty(internKlembord))
                {
                    PlakInternKlembord();
                }
            }
        }
    }

    // 1. Schakel naar het Chat-tabblad
    public void OpenTabChat()
    {
        if (chatInhoudPanel != null) chatInhoudPanel.SetActive(true);
        if (bestandenInhoudPanel != null) bestandenInhoudPanel.SetActive(false);
    }

    // 2. Schakel naar het Bestanden-tabblad
    public void OpenTabBestanden()
    {
        if (chatInhoudPanel != null) chatInhoudPanel.SetActive(false);
        if (bestandenInhoudPanel != null) bestandenInhoudPanel.SetActive(true);
    }

    // 3. Aangeroepen door de Koppeling kopieren knop in de groepsapp
    public void KopieerKoppelingUitBestanden()
    {
        if (chatHeaderTitle != null && chatHeaderTitle.text.Contains("Groep"))
        {
            // We slaan de link intern op in het script om WebGL restricties te omzeilen
            internKlembord = "https://company.sharepoint.com/ProjectTeamAlpha/Onboarding_Handleiding.docx";

            // We vullen de systeem-buffer voor het geval de browser het lokaal wél toestaat
            GUIUtility.systemCopyBuffer = internKlembord;

            if (messageHistoryText != null)
            {
                messageHistoryText.text += "\n<color=cyan><i>Systeem: Link naar Onboarding_Handleiding.docx gekopieerd! Schakel over naar Anouk en gebruik Ctrl+V in de chatbalk om te plakken.</i></color>";
            }
        }
        else
        {
            if (messageHistoryText != null)
            {
                messageHistoryText.text += "\n<color=red><i>Systeem: Er staan nog geen bestanden in deze chat. Schakel over naar de Project Groep om het bestand te zoeken.</i></color>";
            }
        }
    }

    // De neppe plakfunctie die de intern opgeslagen link in de invoerbalk zet
    public void PlakInternKlembord()
    {
        if (chatInputField != null)
        {
            // Voeg de link toe aan de tekst die de gebruiker al had getypt
            chatInputField.text += internKlembord;

            // Zet de typ-cursor helemaal aan het einde van de nieuwe link
            chatInputField.caretPosition = chatInputField.text.Length;
        }
    }

    // Aangeroepen door de Verzend-knop of bij een Enter-input
    public void SendMessage()
    {
        if (chatInputField == null || messageHistoryText == null) return;

        string userMessage = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        // Voeg jouw bericht toe aan de geschiedenis
        messageHistoryText.text += $"\n<b>Jij:</b> {userMessage}";

        // Controleer op informeel taalgebruik (je/jij)
        AnalyzeLanguage(userMessage);

        string lowerMessage = userMessage.ToLower();

        // Controleer of de speler de gekopieerde Sharepoint/OneDrive link deelt
        if (lowerMessage.Contains("sharepoint.com") ||
            lowerMessage.Contains("onedrive.com") ||
            lowerMessage.Contains("onboarding_handleiding"))
        {
            if (chatHeaderTitle != null && chatHeaderTitle.text.Contains("Anouk"))
            {
                taskManager.CompleteTask("Link");
            }
            else
            {
                messageHistoryText.text += "\n<color=orange><i>Systeem: Je deelt het bestand nu in de verkeerde chat. De opdracht was om deze naar Anouk te sturen!</i></color>";
            }
        }

        chatInputField.text = ""; // Maak de invoerbalk leeg
    }

    private void AnalyzeLanguage(string message)
    {
        string lowerMessage = message.ToLower().Trim();

        if (ContainsWord(lowerMessage, "u") || ContainsWord(lowerMessage, "uw"))
        {
            messageHistoryText.text += "\n<color=orange><i>Systeem: Binnen Teams communiceren we informeel. Gebruik je, jij of jouw in plaats van u of uw.</i></color>";
            return;
        }

        if (ContainsWord(lowerMessage, "je") || ContainsWord(lowerMessage, "jij") || ContainsWord(lowerMessage, "jouw"))
        {
            taskManager.CompleteTask("Je");
        }
    }

    private bool ContainsWord(string fullText, string word)
    {
        char[] delimiters = new char[] { ' ', '.', ',', '!', '?', ';', ':' };
        string[] words = fullText.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string w in words)
        {
            if (w == word) return true;
        }
        return false;
    }
}