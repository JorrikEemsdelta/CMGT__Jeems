using UnityEngine;
using TMPro;

public class MyRequestsController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject requestPrefab; 
    public Transform contentContainer; 

    // This runs automatically every time the player clicks the "Mijn aanvragen" tile
    void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        // 1. Clear out the old list so we don't get duplicates
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Fetch all Security Incidents
        if (SecurityManager.Instance != null)
        {
            foreach (var incident in SecurityManager.Instance.reportedIncidents)
            {
                CreateEntry($"Beveiligingsincident: {incident.category}", incident.date, incident.shortDescription);
            }

            // 3. Fetch all Data Breaches
            foreach (var breach in SecurityManager.Instance.reportedBreaches)
            {
                string dateStr = breach.isDateUnknown ? "Datum: Onbekend" : $"Datum: {breach.dateOfBreach}";
                CreateEntry("Datalek Gemeld", dateStr, breach.description);
            }
        }

        // 4. Fetch all General Reports
        if (GeneralReportManager.Instance != null)
        {
            foreach (var report in GeneralReportManager.Instance.reportedItems)
            {
                // General reports don't have a date field in our setup, so we put a dash
                CreateEntry($"Algemene Melding: {report.category}", "-", report.description);
            }
        }
    }

    // Helper function to actually spawn the UI card and fill in the text
    private void CreateEntry(string title, string date, string description)
    {
        GameObject newEntry = Instantiate(requestPrefab, contentContainer, false);
        newEntry.transform.localScale = Vector3.one;

        // Find the specific text boxes inside the prefab
        Transform titleObj = newEntry.transform.Find("Text_Title");
        Transform dateObj = newEntry.transform.Find("Text_Date");
        Transform descObj = newEntry.transform.Find("Text_Description");

        // Fill them with the data
        if (titleObj != null) titleObj.GetComponent<TextMeshProUGUI>().text = $"<b>{title}</b>";
        if (dateObj != null) dateObj.GetComponent<TextMeshProUGUI>().text = date;
        
        if (descObj != null) 
        {
            // Truncate the description if it's too long, so the cards don't get massive
            string shortDesc = description.Length > 50 ? description.Substring(0, 50) + "..." : description;
            descObj.GetComponent<TextMeshProUGUI>().text = shortDesc;
        }
    }
}