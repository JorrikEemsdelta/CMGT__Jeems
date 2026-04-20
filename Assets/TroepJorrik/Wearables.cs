using UnityEngine;
using UnityEngine.UI;

public class WearableManager : MonoBehaviour
{
    [System.Serializable]
    public class WearableItem
    {
        public string name;
        public string saveKey;
        public GameObject prefab;
        public int requiredStreak = 3; // New: Set the requirement per item
    }

    public WearableItem[] wearables;

    void Start()
    {
        RefreshAllItems();
    }

    public void RefreshAllItems()
    {
        int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0);

        for (int i = 0; i < wearables.Length; i++)
        {
            // Only show/enable if the streak is high enough
            if (currentStreak >= wearables[i].requiredStreak)
            {
                bool isEquipped = PlayerPrefs.GetInt(wearables[i].saveKey, 0) == 1;
                if (wearables[i].prefab != null)
                {
                    wearables[i].prefab.SetActive(isEquipped);
                }
            }
            else
            {
                // Force off if they lost their streak
                if (wearables[i].prefab != null) wearables[i].prefab.SetActive(false);
            }
        }
    }

    public void ToggleByIndex(int index)
    {
        if (index < 0 || index >= wearables.Length) return;

        WearableItem item = wearables[index];
        int currentStreak = PlayerPrefs.GetInt("TotalDaysPlayed", 0);

        // --- STREAK CHECK ---
        if (currentStreak < item.requiredStreak)
        {
            Debug.LogWarning($"Streak too low! You need {item.requiredStreak} days, but you have {currentStreak}.");
            // Optional: Trigger a UI "Shake" or a "Locked" sound effect here
            return;
        }

        if (item.prefab == null) return;

        bool newState = !item.prefab.activeSelf;
        item.prefab.SetActive(newState);

        PlayerPrefs.SetInt(item.saveKey, newState ? 1 : 0);
        PlayerPrefs.Save();
    }
}