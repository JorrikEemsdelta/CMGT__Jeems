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
    
    [Tooltip("Short category or title for the question. MUST BE UNIQUE for saving progress!")]
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

    [Tooltip("Text shown to the user when they get this question RIGHT.")]
    [TextArea(2, 4)]
    public string explanationWhenRight = "Correct! Great job.";

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
    public CharacterActionManager resultAnimator;

    [Space(10)]
    [Header("Gamification")]
    [Tooltip("Drag your static Top-Left Streak Text here to show gamification progress.")]
    public TMPro.TMP_Text mainStreakDisplay;

    [Space(10)]
    [Header("Question Library (Populated from JSON)")]
    [Tooltip("Questions will be overwritten by the JSON file at runtime.")]
    public List<QuizQuestion> level1_VeryEasy = new List<QuizQuestion>();
    public List<QuizQuestion> level2_Easy = new List<QuizQuestion>();
    public List<QuizQuestion> level3_Medium = new List<QuizQuestion>();
    public List<QuizQuestion> level4_Hard = new List<QuizQuestion>();
    public List<QuizQuestion> level5_VeryHard = new List<QuizQuestion>();

    private List<QuizQuestion> todayQuestions = new List<QuizQuestion>(); 
    private string savePath; 
    private string clientJsonPath;

    void Awake()
    {
        GetPaths();
        LoadData();
    }

    void Start()
    {
        InitializeDailyQuiz();
    }

    private void GetPaths() 
    {
        if(string.IsNullOrEmpty(savePath)) {
            savePath = Path.Combine(Application.persistentDataPath, "quiz_save_data.json");
        }
        
        if(string.IsNullOrEmpty(clientJsonPath)) {
            clientJsonPath = Path.Combine(Application.streamingAssetsPath, "client_questions.json");
        }
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

        SavePlayerProgress(); 
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
            SavePlayerProgress();

            if (resultAnimator != null) resultAnimator.TriggerRandomVictory();

            // --- NEW: Show Explanation for Right Answer ---
            uiSlots[slotIndex].ShowRightExplanation(q.explanationWhenRight, () => 
            {
                uiSlots[slotIndex].HideRightExplanation();
                RefreshUISlots(); 
            });
        }
        else 
        {
            q.isFailed = true; 
            SavePlayerProgress();

            if (resultAnimator != null) resultAnimator.TriggerRandomFail();

            // Existing: Show Explanation for Wrong Answer
            uiSlots[slotIndex].ShowExplanation(q.explanationWhenWrong, () => 
            {
                uiSlots[slotIndex].HideExplanation();
                RefreshUISlots(); 
            });
        }
    }

    private void LoadData()
    {
        if (File.Exists(clientJsonPath))
        {
            string masterJson = File.ReadAllText(clientJsonPath);
            QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(masterJson);
            if (wrapper != null)
            {
                level1_VeryEasy = wrapper.level1 ?? new List<QuizQuestion>();
                level2_Easy = wrapper.level2 ?? new List<QuizQuestion>();
                level3_Medium = wrapper.level3 ?? new List<QuizQuestion>();
                level4_Hard = wrapper.level4 ?? new List<QuizQuestion>();
                level5_VeryHard = wrapper.level5 ?? new List<QuizQuestion>();
            }
        }
        else
        {
            SaveMasterTemplate();
        }

        if (File.Exists(savePath))
        {
            string saveJson = File.ReadAllText(savePath);
            QuizDataWrapper saveWrapper = JsonUtility.FromJson<QuizDataWrapper>(saveJson);
            
            if (saveWrapper != null)
            {
                ApplySaveState(level1_VeryEasy, saveWrapper.level1);
                ApplySaveState(level2_Easy, saveWrapper.level2);
                ApplySaveState(level3_Medium, saveWrapper.level3);
                ApplySaveState(level4_Hard, saveWrapper.level4);
                ApplySaveState(level5_VeryHard, saveWrapper.level5);
            }
        }
    }

    private void ApplySaveState(List<QuizQuestion> masterList, List<QuizQuestion> saveList)
    {
        if (masterList == null || saveList == null) return;

        foreach (var savedQ in saveList)
        {
            var masterQ = masterList.FirstOrDefault(q => q.title == savedQ.title);
            if (masterQ != null)
            {
                masterQ.isCompleted = savedQ.isCompleted;
                masterQ.isFailed = savedQ.isFailed;
                masterQ.assignedDate = savedQ.assignedDate;
            }
        }
    }

    private void SavePlayerProgress()
    {
        QuizDataWrapper wrapper = new QuizDataWrapper 
        { 
            level1 = this.level1_VeryEasy, level2 = this.level2_Easy,
            level3 = this.level3_Medium, level4 = this.level4_Hard, level5 = this.level5_VeryHard
        };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, json);
    }

    private void SaveMasterTemplate()
    {
        string dir = Path.GetDirectoryName(clientJsonPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        QuizDataWrapper wrapper = new QuizDataWrapper 
        { 
            level1 = this.level1_VeryEasy, level2 = this.level2_Easy,
            level3 = this.level3_Medium, level4 = this.level4_Hard, level5 = this.level5_VeryHard
        };
        
        string json = JsonUtility.ToJson(wrapper, true); 
        File.WriteAllText(clientJsonPath, json);
    }

    [ContextMenu("1. Reset All Progress (Back to Start)")]
    private void ClearSaveData()
    {
        PlayerPrefs.DeleteKey("LastPlayedDate");
        PlayerPrefs.DeleteKey("LastStreakDate"); 
        PlayerPrefs.DeleteKey("TotalDaysPlayed"); 
        
        GetPaths();
        if (File.Exists(savePath)) File.Delete(savePath);
        
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