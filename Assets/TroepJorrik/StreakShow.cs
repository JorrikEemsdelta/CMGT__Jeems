using UnityEngine;
using TMPro;

public class StreakDisplay : MonoBehaviour
{
    public TMP_Text streakText;
    public string prefix = "Day Streak: ";
    public string suffix = "";

    void Start()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (streakText == null) return;

        // Pull the "TotalDaysPlayed" key from your QuizManager
        int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0);

        // Combines: Prefix + Number + Suffix
        streakText.text = prefix + currentStreak.ToString() + suffix;
    }

    private void OnEnable()
    {
        UpdateDisplay();
    }
}