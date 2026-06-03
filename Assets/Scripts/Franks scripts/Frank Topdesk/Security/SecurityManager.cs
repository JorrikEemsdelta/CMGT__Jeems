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

    // This runs when the script is loaded. It configures the Singleton Instance and loads previously submitted incident and breach database records.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadData(); 
    }

    // This creates and logs a new security incident record, checks with the AssignmentManager to complete any active task goals, and saves progress.
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

    // This creates and logs a new dataleak report, checks with the AssignmentManager to complete any active task goals, and saves progress.
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
    // This serializes the reported incidents and breaches lists into JSON and stores it in player preferences.
    public void SaveData()
    {
        SecuritySaveData data = new SecuritySaveData();
        data.incidents = reportedIncidents;
        data.breaches = reportedBreaches;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SecuritySaveData", json);
        PlayerPrefs.Save();
    }

    // This loads the submitted incidents and breaches database from JSON format inside player preferences.
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

    // This clears the active incidents and breaches lists, and deletes the security data save key from preferences.
    public void ClearSaveData()
    {
        reportedIncidents.Clear();
        reportedBreaches.Clear();
        PlayerPrefs.DeleteKey("SecuritySaveData");
        PlayerPrefs.Save();
        Debug.Log("🗑️ Security Data Verwijderd!");
    }
}