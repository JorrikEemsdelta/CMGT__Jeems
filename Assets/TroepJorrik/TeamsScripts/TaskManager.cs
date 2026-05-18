using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public TextMeshProUGUI taskText;

    private bool searchedUser = false;
    private bool createdGroup = false;
    private bool usedJe = false;
    private bool sharedLink = false;

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
        }
        UpdateTaskUI();
    }

    void UpdateTaskUI()
    {
        taskText.text = "<b>LEERDOELEN:</b>\n\n" +
            (searchedUser ? "<s>[X] Zoek naar een collega</s>\n" : "[ ] Zoek naar een collega (Typ 'Anouk')\n") +
            (createdGroup ? "<s>[X] Maak een nieuwe groepschat aan</s>\n" : "[ ] Maak een nieuwe groepschat aan\n") +
            (usedJe ? "<s>[X] Gebruik informele taal ('je/jouw')</s>\n" : "[ ] Gebruik een informele toon (Zeg 'je' in plaats van 'u')\n") +
            (sharedLink ? "<s>[X] Deel een geldige bestandskoppeling</s>\n" : "[ ] Deel een SharePoint- of OneDrive-link\n");

        if (searchedUser && createdGroup && usedJe && sharedLink)
        {
            taskText.text += "\n<color=green><b>🎉 Training voltooid! Goed gedaan!</b></color>";
        }
    }
}