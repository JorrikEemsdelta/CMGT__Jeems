using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public TextMeshProUGUI taskText;
    public TeamsManager teamsManager; // Sleep hier dadelijk _Managers in

    private bool searchedUser = false;
    private bool createdGroup = false;
    private bool usedJe = false;
    private bool sharedLink = false;
    private bool phishingGerapporteerd = false;

    void Start()
    {
        UpdateTaskUI();
    }

    public void CompleteTask(string taskName)
    {
        switch (taskName)
        {
            case "Search": searchedUser = true; break;
            case "Group": createdGroup = true; break;
            case "Je": usedJe = true; break;
            case "Link": sharedLink = true; break;
            case "Phishing": phishingGerapporteerd = true; break;
        }

        UpdateTaskUI();
        CheckAantalVoltooid(); // Check na elke taak of Dave mag komen!
    }

    void CheckAantalVoltooid()
    {
        // Tel hoeveel basistaken er klaar zijn
        int teller = 0;
        if (searchedUser) teller++;
        if (createdGroup) teller++;
        if (usedJe) teller++;
        if (sharedLink) teller++;

        // Als er precies 2 taken af zijn (en Dave is nog niet gerapporteerd), activeer Dave!
        if (teller >= 2 && !phishingGerapporteerd)
        {
            if (teamsManager != null)
            {
                teamsManager.ActiveerPhishingAanval();
            }
        }
    }

    void UpdateTaskUI()
    {
        taskText.text = "<b>LEERDOELEN:</b>\n\n" +
            (searchedUser ? "<s>[X] Zoek naar een collega</s>\n" : "[ ] Zoek naar een collega (Typ 'Anouk')\n") +
            (createdGroup ? "<s>[X] Maak een nieuwe groepschat aan</s>\n" : "[ ] Maak een nieuwe groepschat aan\n") +
            (usedJe ? "<s>[X] Gebruik informele taal ('je/jouw')</s>\n" : "[ ] Gebruik een informele toon (Zeg 'je' in plaats van 'u')\n") +
            (sharedLink ? "<s>[X] Deel een OneDrive/SharePoint-link</s>\n" : "[ ] Deel een OneDrive/SharePoint-link met Anouk\n") +
            (phishingGerapporteerd ? "<s>[X] Rapporteer de phishing aanval</s>\n" : "[ ] Herken en rapporteer een verdacht extern bericht\n");

        if (searchedUser && createdGroup && usedJe && sharedLink && phishingGerapporteerd)
        {
            taskText.text += "\n<color=green><b>🎉 Training voltooid! Je bent nu Teams & Security expert!</b></color>";
        }
    }
}