using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class AnswerOption
{
    public string answerText;
    public bool isCorrect;
}

[System.Serializable]
public class QuizQuestion
{
    public QuestionType questionType;
    public string title;
    [TextArea(2, 5)] public string questionText;

    public AnswerOption[] multipleChoiceOptions;
    public bool correctTrueFalseAnswer;
    public string checkmarkLabel = "I have completed this task";
    public bool correctCheckmarkState = true;

    [TextArea(2, 4)] public string explanationWhenWrong = "Incorrect.";
    [TextArea(2, 4)] public string explanationWhenRight = "Correct!";

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

    [Header("UI References")]
    public QuestionUIController[] uiSlots;
    public GameObject endOfDayPanel;

    [Header("Animation/Visuals")]
    public CharacterActionManager resultAnimator;
    public TMPro.TMP_Text mainStreakDisplay;

    [Header("Question Library")]
    public List<QuizQuestion> level1_VeryEasy = new List<QuizQuestion>();
    public List<QuizQuestion> level2_Easy = new List<QuizQuestion>();
    public List<QuizQuestion> level3_Medium = new List<QuizQuestion>();
    public List<QuizQuestion> level4_Hard = new List<QuizQuestion>();
    public List<QuizQuestion> level5_VeryHard = new List<QuizQuestion>();

    private List<QuizQuestion> todayQuestions = new List<QuizQuestion>();

    void Start()
    {
        // Must use Coroutine for WebGL to download the JSON from the server
        StartCoroutine(LoadDataAndInitializeRoutine());
    }

    private IEnumerator LoadDataAndInitializeRoutine()
    {
        string clientJsonPath = Path.Combine(Application.streamingAssetsPath, "client_questions.json");
        string masterJson = "";

        // 1. Fetch JSON (Web or Local)
        if (clientJsonPath.Contains("://") || Application.platform == RuntimePlatform.WebGLPlayer)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(clientJsonPath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    masterJson = www.downloadHandler.text;
                    Debug.Log("Master JSON loaded successfully.");
                }
                else
                {
                    Debug.LogError($"JSON LOAD FAILED! Check file name case-sensitivity. Path: {clientJsonPath} Error: {www.error}");
                }
            }
        }
        else
        {
#if !UNITY_WEBGL
            if (File.Exists(clientJsonPath)) masterJson = File.ReadAllText(clientJsonPath);
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

        // 2. Load Save Data
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

        // 3. Kick off Game Logic
        InitializeDailyQuiz();
    }

    private void InitializeDailyQuiz()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string lastDate = PlayerPrefs.GetString("LastPlayedDate", "");

        if (today != lastDate)
        {
            GenerateDailyList(today);
            PlayerPrefs.SetString("LastPlayedDate", today);
            PlayerPrefs.Save();
        }
        else
        {
            LoadCurrentDailyProgress(today);
        }

        UpdateStreakUI();
        RefreshUISlots();
    }

    private void LoadCurrentDailyProgress(string todayString)
    {
        var allQuestions = GetAllQuestions();

        // CIRCUIT BREAKER: If no questions exist, stop initialization to prevent recursion crash
        if (allQuestions.Count == 0)
        {
            Debug.LogWarning("No questions available to load for today. Check JSON content.");
            return;
        }

        todayQuestions = allQuestions.Where(q => q.assignedDate == todayString).ToList();

        // Only generate if we don't have any assigned for today yet
        if (todayQuestions.Count == 0)
        {
            GenerateDailyList(todayString);
        }
    }

    private void GenerateDailyList(string todayString)
    {
        todayQuestions.Clear();
        var allQuestions = GetAllQuestions();
        if (allQuestions.Count == 0) return;

        // Carry over failures
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

    private void SavePlayerProgress()
    {
        QuizDataWrapper wrapper = new QuizDataWrapper
        {
            level1 = this.level1_VeryEasy,
            level2 = this.level2_Easy,
            level3 = this.level3_Medium,
            level4 = this.level4_Hard,
            level5 = this.level5_VeryHard
        };

        PlayerPrefs.SetString("PlayerQuizSave", JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    private void RefreshUISlots()
    {
        var activeQuestions = todayQuestions.Where(q => !q.isCompleted).ToList();

        if (activeQuestions.Count == 0 && todayQuestions.Count > 0)
        {
            HandleEndOfDay();
            return;
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

    private void HandleEndOfDay()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string lastStreakDate = PlayerPrefs.GetString("LastStreakDate", "");

        if (today != lastStreakDate)
        {
            int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0) + 1;
            PlayerPrefs.SetInt("TotalDaysPlayed", currentStreak);
            PlayerPrefs.SetString("LastStreakDate", today);
            PlayerPrefs.Save();
            UpdateStreakUI();
        }

        if (endOfDayPanel != null) endOfDayPanel.SetActive(true);
        foreach (var slot in uiSlots) slot.gameObject.SetActive(false);
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

    private List<QuizQuestion> GetAllQuestions()
    {
        List<QuizQuestion> combined = new List<QuizQuestion>();
        combined.AddRange(level1_VeryEasy ?? new List<QuizQuestion>());
        combined.AddRange(level2_Easy ?? new List<QuizQuestion>());
        combined.AddRange(level3_Medium ?? new List<QuizQuestion>());
        combined.AddRange(level4_Hard ?? new List<QuizQuestion>());
        combined.AddRange(level5_VeryHard ?? new List<QuizQuestion>());
        return combined;
    }

    private void UpdateStreakUI()
    {
        if (mainStreakDisplay != null)
        {
            int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0);
            mainStreakDisplay.text = "Streak: " + currentStreak;
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

    [ContextMenu("Reset Save Data")]
    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey("PlayerQuizSave");
        PlayerPrefs.DeleteKey("LastPlayedDate");
        PlayerPrefs.DeleteKey("LastStreakDate");
        PlayerPrefs.DeleteKey("TotalDaysPlayed");
        PlayerPrefs.Save();
        Debug.Log("Save data cleared!");
    }
}