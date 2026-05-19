using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TeamsManager : MonoBehaviour
{
    public TMP_InputField searchInputField;
    public TextMeshProUGUI chatHeaderTitle;
    public TextMeshProUGUI messageHistoryText;
    public TaskManager taskManager;
    public GameObject mainChatPanel;

    [Header("Zijbalk Instellingen")]
    public GameObject chatListContent;
    public GameObject groepsKnopPrefab;

    void Start()
    {
        if (mainChatPanel != null)
        {
            mainChatPanel.SetActive(false);
        }
    }

    // Deze functie wordt aangeroepen door de ZOEKBALK
    public void WordtGezocht(string waarde)
    {
        string zoekTerm = waarde.ToLower().Trim();

        if (zoekTerm.Contains("anouk"))
        {
            // Vink de taak ALLEEN hier af, omdat de gebruiker nu echt gezocht heeft!
            taskManager.CompleteTask("Search");

            // Open daarna de chat
            OpenChatAnouk();
        }
    }

    // Deze functie wordt aangeroepen door de KNOP aan de linkerkant (en door de zoekbalk hierboven)
    public void OpenChatAnouk()
    {
        if (mainChatPanel != null) mainChatPanel.SetActive(true);

        chatHeaderTitle.text = "Anouk (HR)";

        messageHistoryText.text = "<color=#A0A0A0><i>Begin van chatgeschiedenis met Anouk</i></color>\n\n" +
                                   "<b>Anouk:</b> Hoi! Welkom bij de onboarding. Laat me weten als je hulp nodig hebt.";
    }

    public void OpenGroepsChat()
    {
        if (mainChatPanel != null) mainChatPanel.SetActive(true);

        chatHeaderTitle.text = "Team Onboarding (Groep)";

        messageHistoryText.text = "<color=#A0A0A0><i>Je bevindt je in de groepschat met Anouk en Ben.</i></color>\n\n" +
                                   "<b>Ben:</b> Top dat deze groep is aangemaakt! Heeft iemand de link naar de documenten?";
    }

    public void CreateGroupChat()
    {
        taskManager.CompleteTask("Group");
        OpenGroepsChat();

        if (groepsKnopPrefab != null && chatListContent != null)
        {
            GameObject nieuweKnop = Instantiate(groepsKnopPrefab, chatListContent.transform);

            TextMeshProUGUI knopTekst = nieuweKnop.GetComponentInChildren<TextMeshProUGUI>();
            if (knopTekst != null)
            {
                knopTekst.text = "Project Team Alpha";
            }

            Button btn = nieuweKnop.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OpenGroepsChat);
            }
        }
    }
}