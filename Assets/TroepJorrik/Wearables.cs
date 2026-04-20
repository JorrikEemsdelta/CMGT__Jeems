using UnityEngine;

public class WearableManager : MonoBehaviour
{
    [System.Serializable]
    public class WearableItem
    {
        public string name;         // Just for organization in Inspector
        public string saveKey;      // e.g., "Hat_Equipped"
        public GameObject prefab;   // The object on the character
    }

    public WearableItem[] wearables;

    void Start()
    {
        // On start, set all items based on saved data
        for (int i = 0; i < wearables.Length; i++)
        {
            bool isEquipped = PlayerPrefs.GetInt(wearables[i].saveKey, 0) == 1;
            if (wearables[i].prefab != null)
            {
                wearables[i].prefab.SetActive(isEquipped);
            }
        }
    }

    // Pass the index number from the UI Button
    public void ToggleByIndex(int index)
    {
        // Safety check
        if (index < 0 || index >= wearables.Length) return;

        WearableItem item = wearables[index];
        if (item.prefab == null) return;

        // Toggle logic
        bool newState = !item.prefab.activeSelf;
        item.prefab.SetActive(newState);

        // Save
        PlayerPrefs.SetInt(item.saveKey, newState ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"{item.name} is now {(newState ? "Equipped" : "Unequipped")}");
    }
}