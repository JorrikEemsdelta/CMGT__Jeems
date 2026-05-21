using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AssignmentSaveData
{
    public List<Assignment> savedAssignments;
}

public enum AssignmentType
{
    RoomBooking,
    SecurityIncident,
    DataBreach,
    GeneralReport,
    ManualQuestionText,
    ManualQuestionNumber
}

public enum SecurityCategory
{
    Phishing,
    FysiekeBeveiliging,
    Storing,
    Overig
}

public enum GeneralCategory
{
    SoftwareEnApplicaties,
    Apparatuur,
    MobieleTelefonie,
    WachtwoordWijzigen,
    GemeentelijkGebouw,
    POZaken,
    EHerkenning,
    Overig
}

[System.Serializable]
public class SecurityIncidentPrompt
{
    public SecurityCategory category;
    [TextArea(2, 4)]
    public string description;
}

[System.Serializable]
public class GeneralReportPrompt
{
    public GeneralCategory category;
    [TextArea(2, 4)]
    public string description;
}

[System.Serializable]
public class Assignment
{
    public string title;
    [TextArea(3, 6)]
    public string description;
    public AssignmentType type;
    public string targetCategory; 
    public string targetDate;
    public int targetStartHour;
    public int targetEndHour; 
    public int targetPeople; 
    public string correctTextAnswer;
    public int correctNumberAnswer;
    public bool isCompleted;
}

public class AssignmentManager : MonoBehaviour
{
    public static AssignmentManager Instance;

    public int maxActiveTasks = 3;

    [Header("Generator Bronnen")]
    public List<SecurityIncidentPrompt> securityPrompts = new List<SecurityIncidentPrompt> {
        new SecurityIncidentPrompt { category = SecurityCategory.Overig, description = "Je krijgt vreemde meldingen op je computer te zien." },
        new SecurityIncidentPrompt { category = SecurityCategory.Overig, description = "Je bent bestanden kwijt, je harde schijf is geheel of gedeeltelijk gewist." },
        new SecurityIncidentPrompt { category = SecurityCategory.Phishing, description = "Er bevindt zich een nieuwe werkbalk in je internetbrowser waar je niet om hebt gevraagd." },
        new SecurityIncidentPrompt { category = SecurityCategory.Storing, description = "Je virusscanner haalt geen updates meer op of geeft vage foutmeaning." },
        new SecurityIncidentPrompt { category = SecurityCategory.Overig, description = "Mogelijke (vermoedelijke) besmettingen met virussen i.c.m. malware." },
        new SecurityIncidentPrompt { category = SecurityCategory.Overig, description = "Er zijn pogingen gedaan om ongeautoriseerd toegang te krijgen tot informatie of systemen (hacken)." },
        new SecurityIncidentPrompt { category = SecurityCategory.FysiekeBeveiliging, description = "Diefstal of verlies van data of hardware (bijv. in de vorm van laptop, tablet, smartphone of USB-stick)." },
        new SecurityIncidentPrompt { category = SecurityCategory.Storing, description = "Een informatiesysteem bevat onjuiste of niet-actuele informatie." },
        new SecurityIncidentPrompt { category = SecurityCategory.FysiekeBeveiliging, description = "Dossiers c.q. vertrouwelijke documenten liggen rond te zwerven in de organisatie." },
        new SecurityIncidentPrompt { category = SecurityCategory.Phishing, description = "Je mailbox is overgenomen door iemand van buitenaf." },
        new SecurityIncidentPrompt { category = SecurityCategory.Phishing, description = "Wachtwoorden zijn om welke reden dan ook bekend geraakt." },
        new SecurityIncidentPrompt { category = SecurityCategory.Storing, description = "Een backup van gegevens is niet gelukt." },
        new SecurityIncidentPrompt { category = SecurityCategory.Phishing, description = "Je hebt plotseling een ander wachtwoord nadat je terugkomt van vakantie." },
        new SecurityIncidentPrompt { category = SecurityCategory.FysiekeBeveiliging, description = "Personen die daarvoor niet geautoriseerd zijn bevinden zich zonder begeleiding in afgeschermde gedeelten van het gemeentehuis." }
    };

    public List<string> dataBreachPrompts = new List<string> {
        "Een apparaat met daarop een kopie van het klantenbestand van de organisatie is zoekgeraakt of gestolen.",
        "De enige kopie van een verzameling persoonsgegevens is door 'ransomware' (gijzelsoftware) versleuteld.",
        "Iemand heeft his computer niet gelocked, waardoor mogelijk anderen in mappen met persoonsgegevens hebben kunnen neuzen."
    };

    public List<GeneralReportPrompt> generalReportPrompts = new List<GeneralReportPrompt> {
        new GeneralReportPrompt { category = GeneralCategory.SoftwareEnApplicaties, description = "Microsoft Word crasht telkens bij het opstarten." },
        new GeneralReportPrompt { category = GeneralCategory.Apparatuur, description = "De printer op de 2e verdieping geeft een inkt-error." },
        new GeneralReportPrompt { category = GeneralCategory.MobieleTelefonie, description = "Ik heb een nieuwe simkaart nodig voor mijn werktelefoon." },
        new GeneralReportPrompt { category = GeneralCategory.WachtwoordWijzigen, description = "Ik ben het wachtwoord van mijn laptop vergeten, kun je deze resetten?" },
        new GeneralReportPrompt { category = GeneralCategory.GemeentelijkGebouw, description = "De lamp in de gang op de begane grond is kapot." },
        new GeneralReportPrompt { category = GeneralCategory.POZaken, description = "Ik wil graag mijn ouderschapsverlof aanvragen, waar kan ik dat doen?" },
        new GeneralReportPrompt { category = GeneralCategory.EHerkenning, description = "Mijn E-Herkenning token werkt niet meer bij het inloggen." },
        new GeneralReportPrompt { category = GeneralCategory.Overig, description = "Ik heb een vraag over de kerstpakketten van dit jaar." }
    };

    [HideInInspector]
    public List<Assignment> assignments = new List<Assignment>();

    void OnValidate()
    {
        if (securityPrompts != null) securityPrompts = securityPrompts.OrderBy(p => p.category).ToList();
        if (generalReportPrompts != null) generalReportPrompts = generalReportPrompts.OrderBy(p => p.category).ToList();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadData();
    }

    void Start()
    {
        CheckAndGenerateEndlessTasks();
    }

    private void ForceUIRefresh()
    {
        AssignmentUIController uiController = FindFirstObjectByType<AssignmentUIController>();
        if (uiController != null) uiController.RefreshAssignments();
    }

    public void CheckAndGenerateEndlessTasks()
    {
        int activeCount = assignments.Count(a => !a.isCompleted);
        bool generatedNew = false;

        while (activeCount < maxActiveTasks)
        {
            GenerateRandomTask();
            activeCount++;
            generatedNew = true;
        }

        if (generatedNew) ForceUIRefresh();
    }

    private string GetSecurityString(SecurityCategory category)
    {
        switch (category)
        {
            case SecurityCategory.Phishing: return "Phishing";
            case SecurityCategory.FysiekeBeveiliging: return "Fysieke beveiliging";
            case SecurityCategory.Storing: return "Storing";
            case SecurityCategory.Overig: return "Overig";
            default: return "Overig";
        }
    }

    private string GetGeneralString(GeneralCategory category)
    {
        switch (category)
        {
            case GeneralCategory.SoftwareEnApplicaties: return "Software / applicaties";
            case GeneralCategory.Apparatuur: return "Apparatuur";
            case GeneralCategory.MobieleTelefonie: return "Diensten omtrent mobiele telefoonnummers";
            case GeneralCategory.WachtwoordWijzigen: return "Wachtwoord wijzigen";
            case GeneralCategory.GemeentelijkGebouw: return "Een gemeentelijk gebouw";
            case GeneralCategory.POZaken: return "P&O zaken";
            case GeneralCategory.EHerkenning: return "E - Herkenning";
            case GeneralCategory.Overig: return "Overig";
            default: return "Overig";
        }
    }

    private void GenerateRandomTask()
    {
        Assignment newTask = new Assignment();
        int categoryRoll = Random.Range(0, 4);

        BookingManager bookingMgr = BookingManager.Instance;
        if (bookingMgr == null) bookingMgr = FindFirstObjectByType<BookingManager>();

        List<Reservation> compBookings = new List<Reservation>();
        if (bookingMgr != null)
        {
            compBookings = bookingMgr.allReservations.Where(r => !r.isPlayerBooking).ToList();
        }

        bool canDoQuestions = compBookings.Count > 0;
        bool canDoBookingAction = bookingMgr != null && bookingMgr.rooms.Count > 0 && bookingMgr.weekDates.Length > 0;

        if (categoryRoll == 0 && !canDoQuestions && !canDoBookingAction)
        {
            categoryRoll = Random.Range(1, 4); 
        }

        // --- 1/4 KANS: KAMERBOEKINGEN ---
        if (categoryRoll == 0)
        {
            int bookingSubRoll = Random.Range(0, 3);
            if (!canDoQuestions) bookingSubRoll = 2;

            if (bookingSubRoll == 0) 
            {
                Reservation target = compBookings[Random.Range(0, compBookings.Count)];
                newTask.type = AssignmentType.ManualQuestionText;
                newTask.title = "Systeem Controle (Naam)";
                newTask.description = $"Kijk in het reserveringssysteem: Wie heeft {target.roomName} geboekt op {target.date} om {target.startHour}:00 uur? (Typ de naam exact over)";
                newTask.correctTextAnswer = target.bookerName;
            }
            else if (bookingSubRoll == 1) 
            {
                Reservation target = compBookings[Random.Range(0, compBookings.Count)];
                newTask.type = AssignmentType.ManualQuestionNumber;
                newTask.title = "Systeem Controle (Aantal)";
                newTask.description = $"Kijk in het reserveringssysteem: Voor hoeveel personen is {target.roomName} geboekt op {target.date} om {target.startHour}:00 uur? (Vul alleen een getal in)";
                newTask.correctNumberAnswer = target.amountOfPeople;
            }
            else 
            {
                int maxAvailableCapacity = bookingMgr.rooms.Max(r => r.capacity);
                int rPeople = Random.Range(2, maxAvailableCapacity + 1);
                
                string rDate = bookingMgr.weekDates[Random.Range(0, bookingMgr.weekDates.Length)];
                int rStart = Random.Range(9, 15); 
                int rDuration = Random.Range(1, 3); 
                int rEnd = rStart + rDuration;

                newTask.type = AssignmentType.RoomBooking;
                newTask.title = "Nieuwe Kamerreservering";
                newTask.description = $"Actie vereist: Boek een geschikte kamer voor {rPeople} personen op {rDate} van {rStart}:00 tot {rEnd}:00 uur.";
                
                newTask.targetCategory = ""; 
                newTask.targetDate = rDate;
                newTask.targetStartHour = rStart;
                newTask.targetEndHour = rEnd;
                newTask.targetPeople = rPeople;
            }
        }
        // --- 1/4 KANS: BEVEILIGINGSINCIDENTEN ---
        else if (categoryRoll == 1 && securityPrompts.Count > 0)
        {
            SecurityIncidentPrompt prompt = securityPrompts[Random.Range(0, securityPrompts.Count)];
            newTask.type = AssignmentType.SecurityIncident;
            newTask.title = "Beveiligingsincident Melden";
            string requiredCat = GetSecurityString(prompt.category);
            newTask.description = $"Situatie:\n\"{prompt.description}\"\n\nRegistreer dit incident onder de juiste categorie: {requiredCat}.";
            newTask.targetCategory = requiredCat; 
        }
        // --- 1/4 KANS: DATALEKKEN ---
        else if (categoryRoll == 2 && dataBreachPrompts.Count > 0)
        {
            string prompt = dataBreachPrompts[Random.Range(0, dataBreachPrompts.Count)];
            string rDate = "Onbekend";
            if (bookingMgr != null && bookingMgr.weekDates.Length > 0)
            {
                rDate = bookingMgr.weekDates[Random.Range(0, bookingMgr.weekDates.Length)];
            }

            newTask.type = AssignmentType.DataBreach;
            newTask.title = "Datalek Melden (AVG)";
            newTask.description = $"Situatie:\n\"{prompt}\"\n\nDatum van inbreuk: {rDate}\n\nMeld dit direct in het Datalek formulier.";
            newTask.targetCategory = ""; 
        }
        // --- 1/4 KANS: ALGEMENE MELDINGEN ---
        else if (categoryRoll == 3 && generalReportPrompts.Count > 0)
        {
            GeneralReportPrompt prompt = generalReportPrompts[Random.Range(0, generalReportPrompts.Count)];
            newTask.title = "Algemene Melding";
            string requiredCat = GetGeneralString(prompt.category);
            newTask.description = $"Situatie:\n\"{prompt.description}\"\n\nMaak een ticket aan en kies de juiste categorie: {requiredCat}.";
            newTask.type = AssignmentType.GeneralReport;
            newTask.targetCategory = requiredCat; 
        }

        assignments.Add(newTask);
        SaveData();
    }

    public void CheckActionBooking(string roomName, string date, int start, int end, int people) 
    { 
        bool hasChanged = false;
        foreach (var task in assignments)
        {
            if (!task.isCompleted && task.type == AssignmentType.RoomBooking)
            {
                bool dateMatch = string.IsNullOrEmpty(task.targetDate) || task.targetDate.Trim().ToLower() == date.Trim().ToLower();
                bool startMatch = (task.targetStartHour == 0) || (task.targetStartHour == start);
                bool endMatch = (task.targetEndHour == 0) || (task.targetEndHour == end);
                bool peopleMatch = (task.targetPeople == 0) || (task.targetPeople <= people); 

                bool capacityMatch = true;
                if (task.targetPeople > 0 && BookingManager.Instance != null)
                {
                    int roomCap = BookingManager.Instance.GetRoomCapacity(roomName);
                    capacityMatch = roomCap >= task.targetPeople;
                }

                // FIX: Controleer of de gekozen kamer NIET al bezet is door een computerboeking
                bool isRoomFree = true;
                if (BookingManager.Instance != null)
                {
                    foreach (var reservation in BookingManager.Instance.allReservations)
                    {
                        // We controleren alleen overlappingen met computerboekingen (!isPlayerBooking)
                        if (!reservation.isPlayerBooking &&
                            reservation.roomName.Trim().ToLower() == roomName.Trim().ToLower() &&
                            reservation.date.Trim().ToLower() == date.Trim().ToLower())
                        {
                            if (start < reservation.endHour && end > reservation.startHour)
                            {
                                isRoomFree = false;
                                break; 
                            }
                        }
                    }
                }

                bool specificRoomMatch = string.IsNullOrEmpty(task.targetCategory) || task.targetCategory.Trim().ToLower() == roomName.Trim().ToLower();

                if (dateMatch && startMatch && endMatch && peopleMatch && capacityMatch && isRoomFree && specificRoomMatch)
                {
                    task.isCompleted = true;
                    hasChanged = true;
                    Debug.Log($"🎉 KAMER SUCCESVOL GEBOEKT EN VRIJ: {task.title}");
                }
                else if (!isRoomFree && dateMatch && startMatch && endMatch)
                {
                    Debug.LogWarning($"⚠️ Opdracht afgewezen: Kamer '{roomName}' is al bezet door een computerboeking op dit tijdstip.");
                }
            }
        }

        if (hasChanged) 
        {
            SaveData();
            CheckAndGenerateEndlessTasks(); 
        }
    }
    
    public void CheckActionSecurityIncident(string category) { CompleteTasksOfType(AssignmentType.SecurityIncident, category); }
    public void CheckActionDataBreach() { CompleteTasksOfType(AssignmentType.DataBreach, ""); }
    public void CheckActionGeneralReport(string category) { CompleteTasksOfType(AssignmentType.GeneralReport, category); }

    private void CompleteTasksOfType(AssignmentType actionType, string actionCategory)
    {
        bool hasChanged = false;
        foreach (var task in assignments)
        {
            if (!task.isCompleted && task.type == actionType)
            {
                if (string.IsNullOrEmpty(task.targetCategory) || task.targetCategory.Trim().ToLower() == actionCategory.Trim().ToLower())
                {
                    task.isCompleted = true;
                    hasChanged = true;
                    Debug.Log($"🎉 OPDRACHT VOLTOOID: {task.title}");
                }
            }
        }

        if (hasChanged) 
        {
            SaveData();
            CheckAndGenerateEndlessTasks();
        }
    }

    public bool CheckStringAnswer(Assignment task, string playerAnswer)
    {
        if (task.type == AssignmentType.ManualQuestionText && task.correctTextAnswer.Trim().ToLower() == playerAnswer.Trim().ToLower())
        {
            task.isCompleted = true;
            SaveData();
            CheckAndGenerateEndlessTasks();
            return true;
        }
        return false;
    }

    public bool CheckIntAnswer(Assignment task, int playerAnswer)
    {
        if (task.type == AssignmentType.ManualQuestionNumber && task.correctNumberAnswer == playerAnswer)
        {
            task.isCompleted = true;
            SaveData();
            CheckAndGenerateEndlessTasks();
            return true;
        }
        return false;
    }

    public void DismissTask(Assignment task)
    {
        if (assignments.Contains(task))
        {
            assignments.Remove(task);
            SaveData();
            ForceUIRefresh();
        }
    }

    public void SaveData()
    {
        AssignmentSaveData data = new AssignmentSaveData();
        data.savedAssignments = assignments;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("AssignmentSaveData", json);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("AssignmentSaveData"))
        {
            string json = PlayerPrefs.GetString("AssignmentSaveData");
            AssignmentSaveData data = JsonUtility.FromJson<AssignmentSaveData>(json);
            if (data.savedAssignments != null && data.savedAssignments.Count > 0)
            {
                assignments = data.savedAssignments;
            }
        }
    }

    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey("AssignmentSaveData");
        PlayerPrefs.Save();
        assignments.Clear(); 
        CheckAndGenerateEndlessTasks();
    }
}