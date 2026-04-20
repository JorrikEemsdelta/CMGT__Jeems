using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking; // <--- REQUIRED FOR WEBGL FILE READING

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
    public QuestionType questionType; 
    public string title;              
    [TextArea(2, 5)] public string questionText; 
    
    [Space(10)]
    [Header("Multiple Choice Settings")]
    public AnswerOption[] multipleChoiceOptions; 

    [Space(10)]
    [Header("True/False Settings")]
    public bool correctTrueFalseAnswer; 

    [Space(10)]
    [Header("Checkmark Task Settings")]
    public string checkmarkLabel = "I have completed this task"; 
    public bool correctCheckmarkState = true; 

    [Space(10)]
    [Header("Feedback")]
    [TextArea(2, 4)]
    public string explanationWhenWrong = "Incorrect. Please review the material and try again tomorrow.";

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
    [Range(1, 100)] public int dailyQuestionLimit = 5; 

    [Space(10)]
    [Header("UI References")]
    public QuestionUIController[] uiSlots; 
    public GameObject endOfDayPanel; 

    [Space(10)]
    [Header("Animation References")]
    public CharacterActionManager resultAnimator; // Re-linked to your new unified script

    [Space(10)]
    [Header("Gamification")]
    public TMPro.TMP_Text mainStreakDisplay;

    [Space(10)]
    [Header("Question Library (Populated from JSON)")]
    public List<QuizQuestion> level1_VeryEasy = new List<QuizQuestion>();
    public List<QuizQuestion> level2_Easy = new List<QuizQuestion>();
    public List<QuizQuestion> level3_Medium = new List<QuizQuestion>();
    public List<QuizQuestion> level4_Hard = new List<QuizQuestion>();
    public List<QuizQuestion> level5_VeryHard = new List<QuizQuestion>();

    private List<QuizQuestion> todayQuestions = new List<QuizQuestion>(); 

    void Start()
    {
        // For WebGL, we MUST start with a Coroutine so the game can "wait" for the web request to download the JSON
        StartCoroutine(LoadDataAndInitializeRoutine());
    }

    // --- NEW WEBGL-SAFE LOADING ROUTINE ---
    private IEnumerator LoadDataAndInitializeRoutine()
    {
        string clientJsonPath = Path.Combine(Application.streamingAssetsPath, "client_questions.json");
        string masterJson = "";

        // 1. Fetch the Master JSON from StreamingAssets (Works for WebGL and Editor)
        if (clientJsonPath.Contains("://") || clientJsonPath.Contains(":///"))
        {
            // If it's a URL (WebGL or Android), use UnityWebRequest to download it
            using (UnityWebRequest www = UnityWebRequest.Get(clientJsonPath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    masterJson = www.downloadHandler.text;
                }
                else
                {
                    Debug.LogError("WebGL File Error: Could not find client_questions.json in StreamingAssets. " + www.error);
                }
            }
        }
        else
        {
            // If it's a local PC/Editor build, use standard File IO
#if !UNITY_WEBGL
            if (File.Exists(clientJsonPath))
            {
                masterJson = File.ReadAllText(clientJsonPath);
            }
            else
            {
                Debug.LogWarning("Client JSON not found! Generating a template...");
                SaveMasterTemplate(clientJsonPath);
            }
#endif
        }

        // Apply Master JSON to lists
        if (!string.IsNullOrEmpty(masterJson))
        {
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

        // 2. Load the Player's Save Data directly from the Browser's Local Storage (PlayerPrefs)
        string saveJson = PlayerPrefs.GetString("PlayerQuizSave", "");
        if (!string.IsNullOrEmpty(saveJson))
        {
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

        // 3. Now that data is fully loaded, build the UI!
        InitializeDailyQuiz();
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

    // --- NEW WEBGL-SAFE SAVING ---
    private void SavePlayerProgress()
    {
        QuizDataWrapper wrapper = new QuizDataWrapper 
        { 
            level1 = this.level1_VeryEasy, level2 = this.level2_Easy,
            level3 = this.level3_Medium, level4 = this.level4_Hard, level5 = this.level5_VeryHard
        };
        
        string json = JsonUtility.ToJson(wrapper, true);
        
        // Instead of writing to a file, we save the JSON string directly into the browser cache
        PlayerPrefs.SetString("PlayerQuizSave", json);
        PlayerPrefs.Save();
    }

    private void SaveMasterTemplate(string path)
    {
#if !UNITY_WEBGL
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        QuizDataWrapper wrapper = new QuizDataWrapper 
        { 
            level1 = this.level1_VeryEasy, level2 = this.level2_Easy,
            level3 = this.level3_Medium, level4 = this.level4_Hard, level5 = this.level5_VeryHard
        };
        
        string json = JsonUtility.ToJson(wrapper, true); 
        File.WriteAllText(path, json);
#endif
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

            uiSlots[slotIndex].ShowExplanation(q.explanationWhenWrong, () => 
            {
                uiSlots[slotIndex].HideExplanation();
                RefreshUISlots(); 
            });
        }
    }

    [ContextMenu("1. Reset All Progress (Back to Start)")]
    private void ClearSaveData()
    {
        PlayerPrefs.DeleteKey("LastPlayedDate");
        PlayerPrefs.DeleteKey("LastStreakDate"); 
        PlayerPrefs.DeleteKey("TotalDaysPlayed"); 
        PlayerPrefs.DeleteKey("PlayerQuizSave"); // Clear the new web-safe save
        
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