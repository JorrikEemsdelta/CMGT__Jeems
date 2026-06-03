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

    // This runs automatically when the script starts. It starts a Coroutine to load quiz data asynchronously so the game doesn't freeze.
    void Start()
    {
        // Must use Coroutine for WebGL to download the JSON from the server
        StartCoroutine(LoadDataAndInitializeRoutine());
    }

    // This downloads the question database (from a local file or web server for WebGL), parses the JSON data, restores the player's saved progress, and kicks off today's quiz.
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

    // This initializes today's quiz. It checks if the day has changed to reset daily questions, loads progress, updates the UI streak, and draws the questions.
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

    // This loads the progress of already assigned questions for today. If none are found, it generates a fresh daily question list.
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

    // This creates a new set of questions for today. It will carry over any questions failed previously so the player can re-try them, and fills the remaining slots up to the daily limit with new, uncompleted questions.
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

    // This saves the player's level progress and question completion states into local storage (PlayerPrefs) so they don't lose progress when closing the game.
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

    // This updates the quiz slots on the screen. It hides completed questions and shows active ones. If all questions for today are done, it triggers the end-of-day sequence.
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

    // This handles the end-of-day sequence. It increments the streak counter (if not already done today) and activates the end-of-day UI panel to celebrate completion.
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

    // This binds a question's data (type, title, answers) to a UI slot and registers click handlers for the option buttons or checkbox toggle based on the question type.
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

    // This checks if the clicked multiple-choice answer is correct, highlights the button, and processes the outcome.
    private void HandleMultipleChoice(int slotIndex, QuizQuestion q, int selectedIndex)
    {
        bool isCorrect = q.multipleChoiceOptions[selectedIndex].isCorrect;
        uiSlots[slotIndex].SetMultipleChoiceColor(selectedIndex, isCorrect);
        ProcessAnswer(slotIndex, q, isCorrect);
    }

    // This checks if the selected True/False option is correct, highlights the button, and processes the outcome.
    private void HandleTrueFalse(int slotIndex, QuizQuestion q, bool playerAnswer)
    {
        bool isCorrect = (playerAnswer == q.correctTrueFalseAnswer);
        uiSlots[slotIndex].SetTrueFalseColor(playerAnswer, isCorrect);
        ProcessAnswer(slotIndex, q, isCorrect);
    }

    // This checks if the checked state of a checkmark question is correct and processes the outcome.
    private void HandleCheckmark(int slotIndex, QuizQuestion q, bool isChecked)
    {
        bool isCorrect = (isChecked == q.correctCheckmarkState);
        ProcessAnswer(slotIndex, q, isCorrect);
    }

    // This processes the answer given by the player. It marks the question completed, saves progress, triggers a victory/fail animation, and shows the corresponding correct/wrong explanation panel.
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

    // This aggregates all quiz questions from level 1 to level 5 into one combined flat list for easier filtering and lookups.
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

    // This reads the streak counter from player save data and refreshes the streak indicator text on the UI screen.
    private void UpdateStreakUI()
    {
        if (mainStreakDisplay != null)
        {
            int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0);
            mainStreakDisplay.text = "Streak: " + currentStreak;
        }
    }

    // This restores the completion and failure states from a loaded save game list back onto our primary master question list.
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

    // This clears all the player's saved data from local storage, resetting their streak, progress, and daily question limits.
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