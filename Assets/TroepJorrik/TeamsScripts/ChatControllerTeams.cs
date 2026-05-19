using UnityEngine;
using TMPro;

public class ChatControllerTeams : MonoBehaviour
{
    public TMP_InputField chatInputField;
    public TextMeshProUGUI messageHistoryText;
    public TaskManager taskManager;

    [Header("Tabbladen UI")]
    public GameObject chatInhoudPanel;       // Het paneel met de chatberichten
    public GameObject bestandenInhoudPanel;  // Het paneel met de bestandenlijst
    public TextMeshProUGUI chatHeaderTitle;   // Om te checken in welke chat we zitten

    private string gekopieerdeBestandLink = "";

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

    // 3. Aangeroepen door de "Koppeling kopiëren" knop bij het bestand in de groep
    public void KopieerKoppelingUitBestanden()
    {
        // We controleren eerst of de gebruiker wel in de Groepsapp zit, 
        // want daar stond het bestand immers!
        if (chatHeaderTitle.text.Contains("Groep"))
        {
            gekopieerdeBestandLink = "https://intranet.eemsdelta.nl/umbraco/media/2377/zo-werkt-eemsdelta.pdf";
            GUIUtility.systemCopyBuffer = gekopieerdeBestandLink; // Kopieer naar echt klembord

            messageHistoryText.text += "\n<i>Systeem: Link naar 'Onboarding_Handleiding.docx' gekopieerd! Ga nu naar de chat met Anouk om hem te delen.</i>";

            // Schakel voor het gemak direct terug naar het chat-tabblad
            OpenTabChat();
        }
        else
        {
            messageHistoryText.text += "\n<color=red><i>Systeem: Er staan nog geen bestanden in deze chat. Schakel over naar de Project Groep om het bestand te zoeken.</i></color>";
        }
    }

    public void SendMessage()
    {
        string userMessage = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        messageHistoryText.text += $"\n<b>Jij:</b> {userMessage}";

        AnalyzeLanguage(userMessage);

        // Check of ze de link delen én of ze dat wel tegen Anouk doen!
        if (userMessage.Contains("zo-werkt-eemsdelta.pdf") || userMessage.Contains("ProjectTeamAlpha"))
        {
            if (chatHeaderTitle.text.Contains("Anouk"))
            {
                taskManager.CompleteTask("Link");
            }
            else
            {
            
                messageHistoryText.text += "\n<color=orange><i> Tip: Je deelt het bestand nu in de verkeerde chat. De opdracht was om deze naar Anouk te sturen!</i></color>";
            }
        }

        chatInputField.text = "";
    }

    private void AnalyzeLanguage(string message)
    {
        string lowerMessage = message.ToLower().Trim();
        if (ContainsWord(lowerMessage, "u") || ContainsWord(lowerMessage, "uw"))
        {
            messageHistoryText.text += "\n<color=orange><i> Tip: Binnen Teams communiceren we informeel. Gebruik 'je', 'jij' of 'jouw' in plaats van 'u' of 'uw'.</i></color>";
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
        foreach (string w in words) { if (w == word) return true; }
        return false;
    }
}