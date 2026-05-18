using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public enum AssignmentCategory { Question, Action }
public enum AssignmentQuestionType { WhoBookedRoom, NameOfMeeting, AmountOfPeople }

// --- NEW: The Narrative Pool Data ---
[System.Serializable]
public class NarrativeScenario
{
    public AssignmentCategory category;
    [TextArea(2, 4)]
    public string narrativeText;
}

[System.Serializable]
public class AssignmentData
{
    public AssignmentCategory category;
    public AssignmentQuestionType questionType; 
    
    // --- NEW: Stores the story text for this specific assignment ---
    [TextArea(2, 4)]
    public string narrativeText; 

    [TextArea(2, 4)]
    public string assignmentText; 
    public bool isCompleted;

    // Answers (For Questions)
    public string expectedStringAnswer; 
    public int expectedIntAnswer;

    // Booking Requirements (For Actions)
    public bool requireDate;
    public string targetDate;
    public bool requireStartTime;
    public int targetStartHour;
    public bool requireEndTime;
    public int targetEndHour;
    public bool requireCapacity;
    public int targetCapacity;
}

public class AssignmentManager : MonoBehaviour
{
    public static AssignmentManager Instance;

    [Header("Narrative Design")]
    public List<NarrativeScenario> narrativeScenarios = new List<NarrativeScenario>();

    [Header("Active Game State")]
    public List<AssignmentData> activeAssignments = new List<AssignmentData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Invoke("EnsureThreeActiveAssignments", 0.5f);
    }

    // --- NEW: Helper function to grab a random story ---
    string GetRandomNarrative(AssignmentCategory category)
    {
        // Find all scenarios that match the category we need
        var validScenarios = narrativeScenarios.Where(n => n.category == category).ToList();
        
        if (validScenarios.Count > 0)
        {
            return validScenarios[Random.Range(0, validScenarios.Count)].narrativeText;
        }
        
        // Fallback if your colleague hasn't written any stories yet!
        return "Nieuw bericht in de inbox:"; 
    }

    public void EnsureThreeActiveAssignments()
    {
        if (BookingManager.Instance == null) return;

        int activeCount = activeAssignments.Count(a => !a.isCompleted);
        int safetyNet = 0; 
        
        while (activeCount < 3 && safetyNet < 10)
        {
            safetyNet++;
            bool success = false;

            if (Random.value > 0.5f) success = GenerateRandomQuestion();
            else success = GenerateRandomAction();

            if (success) activeCount++;
        }

        if (AssignmentUIController.Instance != null)
        {
            AssignmentUIController.Instance.RefreshUI();
        }
    }

    bool GenerateRandomQuestion()
    {
        List<Reservation> computerBookings = BookingManager.Instance.allReservations
            .Where(r => !r.isPlayerBooking).ToList();

        if (computerBookings.Count == 0) return false;

        Reservation targetRes = computerBookings[Random.Range(0, computerBookings.Count)];
        AssignmentData newAssignment = new AssignmentData();
        
        newAssignment.category = AssignmentCategory.Question;
        
        // --- NEW: Grab a random question story ---
        newAssignment.narrativeText = GetRandomNarrative(AssignmentCategory.Question);

        int randomType = Random.Range(0, 3); 

        if (randomType == 0) 
        {
            newAssignment.questionType = AssignmentQuestionType.WhoBookedRoom;
            newAssignment.assignmentText = $"Wie heeft {targetRes.roomName} geboekt op {targetRes.date} van {targetRes.startHour:00}:00 tot {targetRes.endHour:00}:00?";
            newAssignment.expectedStringAnswer = targetRes.bookerName; 
        }
        else if (randomType == 1) 
        {
            newAssignment.questionType = AssignmentQuestionType.NameOfMeeting;
            newAssignment.assignmentText = $"Wat is de naam van de vergadering in {targetRes.roomName} op {targetRes.date} van {targetRes.startHour:00}:00 tot {targetRes.endHour:00}:00?";
            newAssignment.expectedStringAnswer = targetRes.description; 
        }
        else if (randomType == 2) 
        {
            newAssignment.questionType = AssignmentQuestionType.AmountOfPeople;
            newAssignment.assignmentText = $"Hoeveel mensen zijn er bij de '{targetRes.description}' op {targetRes.date} van {targetRes.startHour:00}:00 tot {targetRes.endHour:00}:00?";
            newAssignment.expectedIntAnswer = targetRes.amountOfPeople; 
        }

        activeAssignments.Add(newAssignment);
        return true;
    }

    bool GenerateRandomAction()
    {
        if (BookingManager.Instance.rooms.Count == 0) return false;

        AssignmentData newAssignment = new AssignmentData();
        newAssignment.category = AssignmentCategory.Action;

        // --- NEW: Grab a random action story ---
        newAssignment.narrativeText = GetRandomNarrative(AssignmentCategory.Action);

        RoomData randomRoom = BookingManager.Instance.rooms[Random.Range(0, BookingManager.Instance.rooms.Count)];
        string randomDate = BookingManager.Instance.weekDates[Random.Range(0, BookingManager.Instance.weekDates.Length)];

        int freeStart = -1;
        int maxDuration = 0;

        for (int h = 9; h < 17; h++)
        {
            if (BookingManager.Instance.IsSlotFree(randomRoom.roomName, randomDate, h, h + 1))
            {
                freeStart = h;
                maxDuration = 1;
                for (int end = h + 1; end < 17; end++)
                {
                    if (BookingManager.Instance.IsSlotFree(randomRoom.roomName, randomDate, end, end + 1))
                        maxDuration++;
                    else
                        break;
                }
                break; 
            }
        }

        if (freeStart != -1) 
        {
            int targetDuration = Random.Range(1, maxDuration + 1); 
            int targetCapacity = Random.Range(2, randomRoom.capacity + 1);
            int targetEndHour = freeStart + targetDuration; 

            newAssignment.requireDate = true;
            newAssignment.targetDate = randomDate;
            newAssignment.requireStartTime = true;
            newAssignment.targetStartHour = freeStart;
            newAssignment.requireEndTime = true;
            newAssignment.targetEndHour = targetEndHour;
            newAssignment.requireCapacity = true;
            newAssignment.targetCapacity = targetCapacity;

            newAssignment.assignmentText = $"Boek een willekeurige kamer voor {targetCapacity} personen op {randomDate} van {freeStart:00}:00 tot {targetEndHour:00}:00.";
            
            activeAssignments.Add(newAssignment);
            return true;
        }
        return false;
    }

    public bool CheckStringAnswer(AssignmentData assignment, string playerInput)
    {
        if (playerInput.Trim().ToLower() == assignment.expectedStringAnswer.Trim().ToLower())
        {
            assignment.isCompleted = true;
            EnsureThreeActiveAssignments(); 
            return true;
        }
        return false;
    }

    public bool CheckIntAnswer(AssignmentData assignment, int playerInput)
    {
        if (playerInput == assignment.expectedIntAnswer)
        {
            assignment.isCompleted = true;
            EnsureThreeActiveAssignments(); 
            return true;
        }
        return false;
    }

    public void CheckActionBooking(string bookedDate, int bookedStart, int bookedEnd, int bookedCapacity)
    {
        bool anyCompleted = false;

        foreach (var assignment in activeAssignments)
        {
            if (assignment.isCompleted) continue;

            if (assignment.category == AssignmentCategory.Action)
            {
                bool isCorrect = true;

                if (assignment.requireDate && assignment.targetDate != bookedDate) isCorrect = false;
                if (assignment.requireStartTime && assignment.targetStartHour != bookedStart) isCorrect = false;
                if (assignment.requireEndTime && assignment.targetEndHour != bookedEnd) isCorrect = false; 
                if (assignment.requireCapacity && assignment.targetCapacity != bookedCapacity) isCorrect = false;

                if (isCorrect)
                {
                    assignment.isCompleted = true;
                    anyCompleted = true; 
                }
            }
        }

        if (anyCompleted)
        {
            EnsureThreeActiveAssignments(); 
        }
    }
}