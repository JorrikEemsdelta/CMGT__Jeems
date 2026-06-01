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
        new SecurityIncidentPrompt { category = SecurityCategory.Storing, description = "Je virusscanner haalt geen updates meer op of geeft vage foutmeldingen." },
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
        "Iemand heeft zijn computer niet gelocked, waardoor mogelijk anderen in mappen met persoonsgegevens hebben kunnen neuzen."
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

    // This runs in the Unity Editor when fields are validated. It sorts the prompt collections by category for clean organization.
    void OnValidate()
    {
        if (securityPrompts != null) securityPrompts = securityPrompts.OrderBy(p => p.category).ToList();
        if (generalReportPrompts != null) generalReportPrompts = generalReportPrompts.OrderBy(p => p.category).ToList();
    }

    // This runs when the script starts. It configures the Singleton Instance and loads saved assignments.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadData();
    }

    // This runs on the first frame and fills the assignment list up to the active task limit.
    void Start()
    {
        CheckAndGenerateEndlessTasks();
    }

    // This finds the active UI controller in the scene and tells it to redraw the tasks list.
    private void ForceUIRefresh()
    {
        AssignmentUIController uiController = FindFirstObjectByType<AssignmentUIController>();
        if (uiController != null) uiController.RefreshAssignments();
    }

    // This monitors active tasks and generates randomized tasks if they fall below the active task limit, updating the UI.
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

    // This maps a Security Category enum to its Dutch label text string.
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

    // This maps a General Category enum to its Dutch label text string.
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

    // This generates a randomized assignment of a chosen type (RoomBooking, ManualQuestion, Incident, DataBreach, GeneralReport), ensuring that bookable slots are free before giving booking tasks.
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
        bool canDoBookingAction = bookingMgr != null && bookingMgr.rooms.Count > 0 && bookingMgr.weekDates != null && bookingMgr.weekDates.Length > 0;

        Debug.Log($"[Generator] Rol: {categoryRoll}. Vragen Mogelijk: {canDoQuestions}, Zelf Boeken Mogelijk: {canDoBookingAction}");

        if (categoryRoll == 0 && !canDoQuestions && !canDoBookingAction)
        {
            Debug.LogWarning("[Generator Fallback] Reroll gedwongen naar Security/Datalek/Algemeen.");
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
                // THE FIX: Check actual availability before creating the task
                bool foundValidSlot = false;
                string rDate = "";
                int rStart = 0;
                int rEnd = 0;
                int rPeople = 0;
                int loopCount = 0;

                while (!foundValidSlot && loopCount < 50)
                {
                    rDate = bookingMgr.weekDates[Random.Range(0, bookingMgr.weekDates.Length)];
                    rStart = Random.Range(9, 15);
                    int rDuration = Random.Range(1, 3);
                    rEnd = rStart + rDuration;

                    // Check which rooms are FREE at this random moment
                    List<RoomData> freeRooms = new List<RoomData>();
                    foreach (var room in bookingMgr.rooms)
                    {
                        bool isFree = true;
                        foreach (var res in compBookings)
                        {
                            if (res.roomName.Trim().ToLower() == room.roomName.Trim().ToLower() && 
                                res.date.Trim().ToLower() == rDate.Trim().ToLower())
                            {
                                // Overlap check
                                if (rStart < res.endHour && rEnd > res.startHour)
                                {
                                    isFree = false;
                                    break;
                                }
                            }
                        }
                        if (isFree) freeRooms.Add(room);
                    }

                    // If there are free rooms, determine the number of people based on the LARGEST FREE room
                    if (freeRooms.Count > 0)
                    {
                        int maxFreeCapacity = freeRooms.Max(r => r.capacity);
                        rPeople = Random.Range(2, maxFreeCapacity + 1);
                        foundValidSlot = true;
                    }

                    loopCount++;
                }

                if (foundValidSlot)
                {
                    newTask.type = AssignmentType.RoomBooking;
                    newTask.title = "Nieuwe Kamerreservering";
                    newTask.description = $"Actie vereist: Boek een geschikte kamer voor {rPeople} personen op {rDate} van {rStart}:00 tot {rEnd}:00 uur.";
                    newTask.targetCategory = ""; 
                    newTask.targetDate = rDate;
                    newTask.targetStartHour = rStart;
                    newTask.targetEndHour = rEnd;
                    newTask.targetPeople = rPeople;
                    Debug.Log("🎲 Generator: 'Zelf Boeken' opdracht gemaakt (Gegarandeerd dat er een vrije kamer is).");
                }
                else 
                {
                    // Fallback: If the schedule is 100% full, it becomes a general report
                    GeneralReportPrompt prompt = generalReportPrompts[Random.Range(0, generalReportPrompts.Count)];
                    newTask.title = "Algemene Melding";
                    string requiredCat = GetGeneralString(prompt.category);
                    newTask.description = $"Situatie:\n\"{prompt.description}\"\n\nMaak een ticket aan en kies de juiste categorie: {requiredCat}.";
                    newTask.type = AssignmentType.GeneralReport;
                    newTask.targetCategory = requiredCat; 
                    Debug.LogWarning("🎲 Generator: Rooster was te vol. Fallback opdracht aangemaakt.");
                }
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
        // --- 1/4 CHANCE: GENERAL REPORTS ---
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

    // This checks if a newly created room booking satisfies any active RoomBooking assignment, marking it completed if matches are found.
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

                bool isRoomFree = true;
                if (BookingManager.Instance != null)
                {
                    foreach (var reservation in BookingManager.Instance.allReservations)
                    {
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
    
    // This completes tasks of the SecurityIncident type with a matching category.
    public void CheckActionSecurityIncident(string category) { CompleteTasksOfType(AssignmentType.SecurityIncident, category); }

    // This completes tasks of the DataBreach type.
    public void CheckActionDataBreach() { CompleteTasksOfType(AssignmentType.DataBreach, ""); }

    // This completes tasks of the GeneralReport type with a matching category.
    public void CheckActionGeneralReport(string category) { CompleteTasksOfType(AssignmentType.GeneralReport, category); }

    // This helper searches through the active assignments and completes those of the specified task type and category.
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

    // This validates a text answer for a manual question task (case-insensitive and space-insensitive), completing the task if correct.
    public bool CheckStringAnswer(Assignment task, string playerAnswer)
    {
        if (task.type == AssignmentType.ManualQuestionText)
        {
            // Remove all spaces and make everything lowercase for the ultimate forgiving check
            string cleanCorrectAnswer = task.correctTextAnswer.Replace(" ", "").ToLower();
            string cleanPlayerAnswer = playerAnswer.Replace(" ", "").ToLower();

            if (cleanCorrectAnswer == cleanPlayerAnswer)
            {
                task.isCompleted = true;
                SaveData();
                CheckAndGenerateEndlessTasks();
                return true;
            }
        }
        return false;
    }

    // This validates a numeric answer for a manual question task, completing the task if correct.
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

    // This discards a specific task from the active assignments list.
    public void DismissTask(Assignment task)
    {
        if (assignments.Contains(task))
        {
            assignments.Remove(task);
            SaveData();
            ForceUIRefresh();
        }
    }

    // This serializes the assignments list to JSON and saves it in player preferences.
    public void SaveData()
    {
        AssignmentSaveData data = new AssignmentSaveData();
        data.savedAssignments = assignments;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("AssignmentSaveData", json);
        PlayerPrefs.Save();
    }

    // This loads the assignments list from local player preferences storage.
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

    // This erases the assignments save key from preferences, empties the list, and triggers a fresh task generation cycle.
    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey("AssignmentSaveData");
        PlayerPrefs.Save();
        assignments.Clear(); 
        CheckAndGenerateEndlessTasks();
    }
}