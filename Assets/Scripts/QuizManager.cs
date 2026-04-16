using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AnswerOption
{
    [Tooltip("The text to display for this answer.")]
    public string answerText; 
    [Tooltip("Is this the correct answer?")]
    public bool isCorrect;    
}

[System.Serializable]
public class QuizQuestion
{
    [Header("General Setup")]
    [Tooltip("What kind of question should be shown to the user?")]
    public QuestionType questionType; 
    
    [Tooltip("Short category or title for the question.")]
    public string title;              
    
    [Tooltip("The actual question prompt.")]
    [TextArea(2, 5)] public string questionText; 
    
    [Space(10)]
    [Header("Multiple Choice Settings")]
    [Tooltip("Set up multiple choices here. (Only used if questionType is MultipleChoice)")]
    public AnswerOption[] multipleChoiceOptions; 

    [Space(10)]
    [Header("True/False Settings")]
    [Tooltip("Check this if the correct answer is True. Leave unchecked for False.")]
    public bool correctTrueFalseAnswer; 

    [Space(10)]
    [Header("Checkmark Task Settings")]
    [Tooltip("The text label displayed next to the checkmark task.")]
    public string checkmarkLabel = "I have completed this task"; 
    [Tooltip("Does the checkmark need to be checked to be considered correct?")]
    public bool correctCheckmarkState = true; 

    [Space(10)]
    [Header("Feedback")]
    [Tooltip("Text shown to the user when they get this question wrong.")]
    [TextArea(2, 4)]
    public string explanationWhenWrong = "Incorrect. Please review the material and try again tomorrow.";

    [HideInInspector] public bool isCompleted = false; 
    [HideInInspector] public bool isFailed = false;    
    [HideInInspector] public string assignedDate = ""; 
}

[System.Serializable]
public class QuizDataWrapper
{
    public string lastPlayedDate;
    public List<QuizQuestion> level1;
    public List<QuizQuestion> level2;
    public List<QuizQuestion> level3;
    public List<QuizQuestion> level4;
    public List<QuizQuestion> level5;
}

public class QuizManager : MonoBehaviour
{
    [Header("Daily Settings")]
    [Tooltip("How many questions should the player answer per day?")]
    [Range(1, 100)] public int dailyQuestionLimit = 5; 

    [Space(10)]
    [Header("UI References")]
    [Tooltip("Drag the Question UI instances from the scene here.")]
    public QuestionUIController[] uiSlots; 

    [Tooltip("The panel to display when all daily questions are completed.")]
    public GameObject endOfDayPanel; 

    [Space(10)]
    [Header("Animation References")]
    [Tooltip("Drag your GameResultAnimator object here to trigger win/fail animations.")]
    public GameResultAnimator resultAnimator;

    [Space(10)]
    [Header("Gamification")]
    [Tooltip("Drag your static Top-Left Streak Text here to show gamification progress.")]
    public TMPro.TMP_Text mainStreakDisplay;

    [Space(10)]
    [Header("Question Library (Pulls daily questions from here)")]
    [Tooltip("Difficulty 1: Very Easy")]
    public List<QuizQuestion> level1_VeryEasy = new List<QuizQuestion>();
    [Tooltip("Difficulty 2: Easy")]      
    public List<QuizQuestion> level2_Easy = new List<QuizQuestion>();
    [Tooltip("Difficulty 3: Medium")]    
    public List<QuizQuestion> level3_Medium = new List<QuizQuestion>();
    [Tooltip("Difficulty 4: Hard")]      
    public List<QuizQuestion> level4_Hard = new List<QuizQuestion>();
    [Tooltip("Difficulty 5: Very Hard")] 
    public List<QuizQuestion> level5_VeryHard = new List<QuizQuestion>();

    private List<QuizQuestion> todayQuestions = new List<QuizQuestion>(); 
    private string savePath; 

    void Awake()
    {
        GetSavePath();
        LoadData();
    }

    void Start()
    {
        InitializeDailyQuiz();
    }

    private List<QuizQuestion> GetAllQuestions()
    {
        List<QuizQuestion> combined = new List<QuizQuestion>();
        if (level1_VeryEasy != null) combined.AddRange(level1_VeryEasy);
        if (level2_Easy != null) combined.AddRange(level2_Easy);
        if (level3_Medium != null) combined.AddRange(level3_Medium);
        if (level4_Hard != null) combined.AddRange(level4_Hard);
        if (level5_VeryHard != null) combined.AddRange(level5_VeryHard);
        return combined;
    }

    private void InitializeDailyQuiz()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd"); 
        string lastDate = PlayerPrefs.GetString("LastPlayedDate", ""); 

        if (today != lastDate)
        {
            GenerateDailyList(today);
            PlayerPrefs.SetString("LastPlayedDate", today); 
        }
        else
        {
            LoadCurrentDailyProgress(today);
        }

        UpdateStreakUI();
        RefreshUISlots(); 
    }

    private void UpdateStreakUI()
    {
        if (mainStreakDisplay != null)
        {
            int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0);
            mainStreakDisplay.text = "Streak: " + currentStreak;
        }
    }

    private void GenerateDailyList(string todayString)
    {
        todayQuestions.Clear(); 
        var allQuestions = GetAllQuestions(); 

        var failedQuestions = allQuestions.Where(q => q.isFailed).ToList(); 
        foreach (var q in failedQuestions)
        {
            q.isFailed = false;    
            q.isCompleted = false; 
            q.assignedDate = todayString; 
            todayQuestions.Add(q); 
        }

        int amountNeeded = dailyQuestionLimit - todayQuestions.Count; 
        if (amountNeeded > 0)
        {
            var newQuestions = allQuestions
                .Where(q => !q.isCompleted && !todayQuestions.Contains(q)) 
                .Take(amountNeeded) 
                .ToList();
                
            foreach (var q in newQuestions) q.assignedDate = todayString; 
            todayQuestions.AddRange(newQuestions); 
        }

        SaveData(); 
    }

    private void LoadCurrentDailyProgress(string todayString)
    {
        var allQuestions = GetAllQuestions();
        todayQuestions = allQuestions.Where(q => q.assignedDate == todayString).ToList();
        if (todayQuestions.Count == 0) GenerateDailyList(todayString);
    }

    private void RefreshUISlots()
    {
        var activeQuestions = todayQuestions.Where(q => !q.isCompleted).ToList();
        
        // 1. Are they completely done with today's work?
        if (activeQuestions.Count == 0) 
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string lastStreakDate = PlayerPrefs.GetString("LastStreakDate", "");

            if (today != lastStreakDate)
            {
                int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0);
                currentStreak++;
                PlayerPrefs.SetInt("TotalDaysPlayed", currentStreak);
                PlayerPrefs.SetString("LastStreakDate", today); 
                
                UpdateStreakUI(); 
            }

            if (endOfDayPanel != null) endOfDayPanel.SetActive(true);
            foreach (var slot in uiSlots) slot.gameObject.SetActive(false);
            return; 
        }
        else
        {
            if (endOfDayPanel != null) endOfDayPanel.SetActive(false);
        }

        // --- NEW CODE: Force character back to Idle when a new question loads ---
       // if (resultAnimator != null)
      //  {
          //  resultAnimator.ReturnToIdle();
       // }
        // ------------------------------------------------------------------------

        // 2. Normal Setup
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < activeQuestions.Count)
            {
                uiSlots[i].gameObject.SetActive(true);
                uiSlots[i].ResetColors();
                SetupSlot(i, activeQuestions[i]); 
            }
            else
            {
                uiSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetupSlot(int slotIndex, QuizQuestion q)
    {
        string[] uiAnswers = null;
        if (q.questionType == QuestionType.MultipleChoice)
            uiAnswers = q.multipleChoiceOptions.Select(opt => opt.answerText).ToArray();
        else if (q.questionType == QuestionType.Checkmark)
            uiAnswers = new string[] { q.checkmarkLabel };

        uiSlots[slotIndex].SetupQuestion(q.questionType, q.title, q.questionText, uiAnswers);
        
        if (q.questionType == QuestionType.MultipleChoice)
        {
            for (int i = 0; i < uiSlots[slotIndex].multipleChoiceButtons.Length; i++)
            {
                uiSlots[slotIndex].multipleChoiceButtons[i].onClick.RemoveAllListeners(); 
                int choiceIndex = i; 
                uiSlots[slotIndex].multipleChoiceButtons[i].onClick.AddListener(() => HandleMultipleChoice(slotIndex, q, choiceIndex));
            }
        }
        else if (q.questionType == QuestionType.TrueFalse)
        {
            uiSlots[slotIndex].trueButton.onClick.RemoveAllListeners();
            uiSlots[slotIndex].falseButton.onClick.RemoveAllListeners();
            uiSlots[slotIndex].trueButton.onClick.AddListener(() => HandleTrueFalse(slotIndex, q, true));
            uiSlots[slotIndex].falseButton.onClick.AddListener(() => HandleTrueFalse(slotIndex, q, false));
        }
        else if (q.questionType == QuestionType.Checkmark)
        {
            uiSlots[slotIndex].singleCheckmark.onValueChanged.RemoveAllListeners();
            uiSlots[slotIndex].singleCheckmark.onValueChanged.AddListener((bool isChecked) => HandleCheckmark(slotIndex, q, isChecked));
        }
    }

    private void HandleMultipleChoice(int slotIndex, QuizQuestion q, int selectedIndex)
    {
        bool isCorrect = q.multipleChoiceOptions[selectedIndex].isCorrect;
        uiSlots[slotIndex].SetMultipleChoiceColor(selectedIndex, isCorrect);
        ProcessAnswer(slotIndex, q, isCorrect);
    }

    private void HandleTrueFalse(int slotIndex, QuizQuestion q, bool playerAnswer)
    {
        bool isCorrect = (playerAnswer == q.correctTrueFalseAnswer);
        uiSlots[slotIndex].SetTrueFalseColor(playerAnswer, isCorrect);
        ProcessAnswer(slotIndex, q, isCorrect);
    }

    private void HandleCheckmark(int slotIndex, QuizQuestion q, bool isChecked)
    {
        bool isCorrect = (isChecked == q.correctCheckmarkState);
        ProcessAnswer(slotIndex, q, isCorrect);
    }

    private void ProcessAnswer(int slotIndex, QuizQuestion q, bool isCorrect)
    {
        q.isCompleted = true; 
        
        if (isCorrect) 
        {
            q.isFailed = false; 
            SaveData();

            // Trigger Victory Animation
            if (resultAnimator != null)
            {
                resultAnimator.TriggerRandomVictory();
            }

            Invoke(nameof(RefreshUISlots), 1.5f); 
        }
        else 
        {
            q.isFailed = true; 
            SaveData();

            // Trigger Fail Animation
            if (resultAnimator != null)
            {
                resultAnimator.TriggerRandomFail();
            }

            uiSlots[slotIndex].ShowExplanation(q.explanationWhenWrong, () => 
            {
                uiSlots[slotIndex].HideExplanation();
                RefreshUISlots(); 
            });
        }
    }

    private void SaveData()
    {
        QuizDataWrapper wrapper = new QuizDataWrapper 
        { 
            level1 = this.level1_VeryEasy, level2 = this.level2_Easy,
            level3 = this.level3_Medium, level4 = this.level4_Hard, level5 = this.level5_VeryHard
        };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(GetSavePath(), json);
    }

    private void LoadData()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(json);
            if (wrapper != null)
            {
                if (wrapper.level1 != null && wrapper.level1.Count > 0) level1_VeryEasy = wrapper.level1;
                if (wrapper.level2 != null && wrapper.level2.Count > 0) level2_Easy = wrapper.level2;
                if (wrapper.level3 != null && wrapper.level3.Count > 0) level3_Medium = wrapper.level3;
                if (wrapper.level4 != null && wrapper.level4.Count > 0) level4_Hard = wrapper.level4;
                if (wrapper.level5 != null && wrapper.level5.Count > 0) level5_VeryHard = wrapper.level5;
            }
        }
    }
    
    private string GetSavePath() {
        if(string.IsNullOrEmpty(savePath)) {
            savePath = Path.Combine(Application.persistentDataPath, "quiz_data.json");
        }
        return savePath;
    }

    [ContextMenu("1. Reset All Progress (Back to Start)")]
    private void ClearSaveData()
    {
        PlayerPrefs.DeleteKey("LastPlayedDate");
        PlayerPrefs.DeleteKey("LastStreakDate"); 
        PlayerPrefs.DeleteKey("TotalDaysPlayed"); 
        
        string path = GetSavePath(); 
        if (File.Exists(path)) File.Delete(path);
        
        foreach (var q in GetAllQuestions())
        {
            q.isCompleted = false;
            q.isFailed = false;
            q.assignedDate = ""; 
        }
        
        UpdateStreakUI(); 
        Debug.Log("Progress and Streak Reset!");
    }

    [ContextMenu("2. Fast Forward to Tomorrow")]
    private void SimulateNextDay()
    {
        string fakeYesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        
        PlayerPrefs.SetString("LastPlayedDate", fakeYesterday);
        PlayerPrefs.SetString("LastStreakDate", fakeYesterday); 
        
        Debug.Log("<b>Time travel successful!</b> The game now fully thinks you last played yesterday.");
    }
}