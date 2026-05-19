using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TeamsManager : MonoBehaviour
{
    public TMP_InputField searchInputField;
    public TextMeshProUGUI chatHeaderTitle;
    public TextMeshProUGUI messageHistoryText;
    public TaskManager taskManager;
    public ChatControllerTeams chatController; // Sleep hier het _Managers GameObject in
    public GameObject mainChatPanel;

    [Header("Zijbalk Instellingen")]
    public GameObject chatListContent;
    public GameObject groepsKnopPrefab;
    public GameObject anoukZijbalkKnop; // Sleep hier de Anouk_Button uit de zijbalk in

    [Header("Phishing & Security UI")]
    public GameObject externTag;           // Sleep hier de paarse EXTERN tag in
    public GameObject rapporteerdButton;    // Sleep hier de Rapporteer_Button in
    public GameObject daveZijbalkKnop;     // Sleep hier de Dave_Button uit de zijbalk in

    private bool groepBestaatAl = false;

    void Start()
    {
        if (mainChatPanel != null) mainChatPanel.SetActive(false);
        if (externTag != null) externTag.SetActive(false);
        if (rapporteerdButton != null) rapporteerdButton.SetActive(false);

        // Zorg dat beide knoppen bij de start onzichtbaar zijn
        if (daveZijbalkKnop != null) daveZijbalkKnop.SetActive(false);
        if (anoukZijbalkKnop != null) anoukZijbalkKnop.SetActive(false);
    }

    // Aangeroepen door de TaskManager als er 2 opdrachten af zijn//////
    public void ActiveerPhishingAanval()
    {
        if (daveZijbalkKnop != null && !daveZijbalkKnop.activeSelf)
        {
            daveZijbalkKnop.SetActive(true);
            if (messageHistoryText != null)
            {
                messageHistoryText.text += "\n<color=#7A7A7A><i>Systeem: Je hebt een nieuw chatbericht ontvangen van een onbekende gebruiker.</i></color>";
            }
        }
    }

    // Gekoppeld aan het OnEndEdit of OnValueChanged event van je Search Input Field
    public void WordtGezocht(string waarde)
    {
        string zoekTerm = waarde.ToLower().Trim();
        if (zoekTerm.Contains("anouk"))
        {
            taskManager.CompleteTask("Search");

            // Maak de Anouk knop links in de lijst nu permanent zichtbaar
            if (anoukZijbalkKnop != null)
            {
                anoukZijbalkKnop.SetActive(true);
            }

            OpenChatAnouk();
        }
    }

    public void OpenChatAnouk()
    {
        if (externTag != null) externTag.SetActive(false);
        if (rapporteerdButton != null) rapporteerdButton.SetActive(false);

        // FORCEER RESET: Zet de tabbladen terug naar de gewone chat
        if (chatController != null)
        {
           chatController.OpenTabChat();
        }

        if (mainChatPanel != null) mainChatPanel.SetActive(true);
        chatHeaderTitle.text = "Anouk (HR)";
        messageHistoryText.text = "<color=#A0A0A0><i>Begin van chatgeschiedenis met Anouk</i></color>\n\n<b>Anouk:</b> Hoi! Welkom bij de onboarding. Laat me weten als je hulp nodig hebt.";
    }

    public void OpenChatPhishingDave()
    {
        // Forceer ook hier het gewone chat-tabblad
        if (chatController != null)
        {
            chatController.OpenTabChat();
        }

        if (mainChatPanel != null) mainChatPanel.SetActive(true);
        if (externTag != null) externTag.SetActive(true);
        if (rapporteerdButton != null) rapporteerdButton.SetActive(true);

        chatHeaderTitle.text = "Dave (Extern)";
        messageHistoryText.text = "<color=#FF4500><b>LET OP: Deze persoon is buiten de organisatie.</b></color>\n\n" +
                                   "<b>Dave:</b> Hey! Ik ben de IT-manager van de hoofdlocatie. Ik zit nu in een heel belangrijke meeting met de directie en we moeten direct 3 Amazon Giftcards van 50 euro hebben voor een klant. Kun jij die nu snel halen en de codes hier sturen? Ik regel dat de administratie het vanmiddag naar je terugstort! Haast!!";
    }

    public void RapporteerPhishingAanval()
    {
        taskManager.CompleteTask("Phishing");

        if (externTag != null) externTag.SetActive(false);
        if (rapporteerdButton != null) rapporteerdButton.SetActive(false);

        chatHeaderTitle.text = "Veiligheidscentrum";
        messageHistoryText.text = "<color=green><b>Systeem: Dit gesprek is succesvol gerapporteerd bij de IT Security afdeling!</b>\n" +
                                   "De externe gebruiker Dave is permanent geblokkeerd. Goed scherp gebleven!</color>";

        if (daveZijbalkKnop != null) daveZijbalkKnop.SetActive(false);
    }

    public void OpenGroepsChat()
    {
        if (externTag != null) externTag.SetActive(false);
        if (rapporteerdButton != null) rapporteerdButton.SetActive(false);

        // Altijd openen op het Chat-tabblad als je de groep aanklikt//
        if (chatController != null)
        {
            chatController.OpenTabChat();
        }

        if (mainChatPanel != null) mainChatPanel.SetActive(true);
        chatHeaderTitle.text = "Project Team Alpha (Groep)";
        messageHistoryText.text = "<color=#A0A0A0><i>Je bevindt je in de groepschat met Anouk en Ben.</i></color>\n\n<b>Ben:</b> Top dat deze groep is aangemaakt! Heeft iemand de link naar de documenten?";
    }

    public void CreateGroupChat()
    {
        taskManager.CompleteTask("Group");
        OpenGroepsChat();

        if (!groepBestaatAl)
        {
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

                groepBestaatAl = true;
            }
        }
    }
}