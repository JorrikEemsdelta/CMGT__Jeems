using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SecuritySaveData
{
    public List<SecurityIncident> incidents;
    public List<DataBreachReport> breaches;
}

[System.Serializable]
public class SecurityIncident
{
    public string date;
    public string category;
    public string shortDescription;
    [TextArea(3, 6)]
    public string detailedDescription;
}

[System.Serializable]
public class DataBreachReport
{
    public bool isDateUnknown;
    public string dateOfBreach;
    public string dateDiscovered;
    [TextArea(3, 6)]
    public string description;
}

public class SecurityManager : MonoBehaviour
{
    public static SecurityManager Instance;

    [Header("Databases")]
    public List<SecurityIncident> reportedIncidents = new List<SecurityIncident>();
    public List<DataBreachReport> reportedBreaches = new List<DataBreachReport>(); 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadData(); 
    }

    public void SubmitIncident(string date, string category, string shortDesc, string longDesc)
    {
        SecurityIncident newIncident = new SecurityIncident
        {
            date = date,
            category = category,
            shortDescription = shortDesc,
            detailedDescription = longDesc
        };

        reportedIncidents.Add(newIncident);
        Debug.Log($"✅ SUCCES: {category} incident gemeld op {date}.");

        // --- NEW: Tell Assignment Manager ---
        if (AssignmentManager.Instance != null) 
        {
            AssignmentManager.Instance.CheckActionSecurityIncident(category);
        }

        SaveData();
    }

    public void SubmitDataBreach(bool isUnknown, string breachDate, string discoveredDate, string description)
    {
        DataBreachReport newBreach = new DataBreachReport
        {
            isDateUnknown = isUnknown,
            dateOfBreach = breachDate,
            dateDiscovered = discoveredDate,
            description = description
        };

        reportedBreaches.Add(newBreach);
        
        string breachDateText = isUnknown ? "Onbekend" : breachDate;
        Debug.Log($"🚨 DATALEK GEMELD: Inbreuk was op: {breachDateText}. Ontdekt op: {discoveredDate}.");

        // --- NEW: Tell Assignment Manager ---
        if (AssignmentManager.Instance != null) 
        {
            AssignmentManager.Instance.CheckActionDataBreach();
        }

        SaveData();
    }

    // ==========================================
    // THE SAVE SYSTEM
    // ==========================================
    public void SaveData()
    {
        SecuritySaveData data = new SecuritySaveData();
        data.incidents = reportedIncidents;
        data.breaches = reportedBreaches;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SecuritySaveData", json);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("SecuritySaveData"))
        {
            string json = PlayerPrefs.GetString("SecuritySaveData");
            SecuritySaveData data = JsonUtility.FromJson<SecuritySaveData>(json);
            
            reportedIncidents = data.incidents != null ? data.incidents : new List<SecurityIncident>();
            reportedBreaches = data.breaches != null ? data.breaches : new List<DataBreachReport>();
        }
    }

    public void ClearSaveData()
    {
        reportedIncidents.Clear();
        reportedBreaches.Clear();
        PlayerPrefs.DeleteKey("SecuritySaveData");
        PlayerPrefs.Save();
        Debug.Log("🗑️ Security Data Verwijderd!");
    }
}