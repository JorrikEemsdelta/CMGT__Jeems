using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GeneralSaveData
{
    public List<GeneralReport> reports;
}

[System.Serializable]
public class GeneralReport
{
    public string category;
    [TextArea(3, 6)]
    public string description;
}

public class GeneralReportManager : MonoBehaviour
{
    public static GeneralReportManager Instance;

    public List<GeneralReport> reportedItems = new List<GeneralReport>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadData();
    }

    public void SubmitReport(string category, string description)
    {
        GeneralReport newReport = new GeneralReport
        {
            category = category,
            description = description
        };

        reportedItems.Add(newReport);
        Debug.Log($"📝 ALGEMEEN GEMELD: [{category}] - {description}");

        // --- NEW: Tell Assignment Manager ---
        if (AssignmentManager.Instance != null) 
        {
            AssignmentManager.Instance.CheckActionGeneralReport(category);
        }

        SaveData();
    }

    // ==========================================
    // THE SAVE SYSTEM
    // ==========================================
    public void SaveData()
    {
        GeneralSaveData data = new GeneralSaveData();
        data.reports = reportedItems;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GeneralSaveData", json);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("GeneralSaveData"))
        {
            string json = PlayerPrefs.GetString("GeneralSaveData");
            GeneralSaveData data = JsonUtility.FromJson<GeneralSaveData>(json);
            
            reportedItems = data.reports != null ? data.reports : new List<GeneralReport>();
        }
    }

    public void ClearSaveData()
    {
        reportedItems.Clear();
        PlayerPrefs.DeleteKey("GeneralSaveData");
        PlayerPrefs.Save();
        Debug.Log("🗑️ General Report Data Verwijderd!");
    }
}