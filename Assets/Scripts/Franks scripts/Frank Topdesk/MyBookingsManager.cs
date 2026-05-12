using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyBookingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bookingEntryPrefab; 
    public Transform contentContainer; 

    void OnEnable()
    {
        // Refresh the list automatically when this screen is opened
        RefreshList();
    }

    public void RefreshList()
    {
        // 1. Clear out the old list so we don't get duplicates
        foreach (Transform child in contentContainer) 
        {
            Destroy(child.gameObject);
        }

        // 2. Loop through every reservation in the main memory
        foreach (Reservation res in BookingManager.Instance.allReservations)
        {
            // Only show bookings made by the player
            if (res.isPlayerBooking)
            {
                // Spawn your specific visual prefab
                GameObject newEntry = Instantiate(bookingEntryPrefab, contentContainer);
                
                // 3. Find your specific Text objects and fill them
                Transform roomTextObj = newEntry.transform.Find("RoomText");
                Transform timeTextObj = newEntry.transform.Find("TimeText");
                Transform descTextObj = newEntry.transform.Find("DescText");

                if (roomTextObj != null) roomTextObj.GetComponent<TextMeshProUGUI>().text = $"{res.roomName} | {res.date}";
                if (timeTextObj != null) timeTextObj.GetComponent<TextMeshProUGUI>().text = $"{res.startHour:00}:00 - {res.endHour:00}:00";
                if (descTextObj != null) descTextObj.GetComponent<TextMeshProUGUI>().text = res.description;

                // 4. Find and set up the Delete Button
                Transform deleteBtnTransform = newEntry.transform.Find("Btn_Delete");
                
                if (deleteBtnTransform != null)
                {
                    Button deleteBtn = deleteBtnTransform.GetComponent<Button>();
                    
                    // Save the current loop item into a temporary variable
                    Reservation tempRes = res; 
                    
                    deleteBtn.onClick.RemoveAllListeners();
                    deleteBtn.onClick.AddListener(() => 
                    {
                        BookingManager.Instance.DeleteBooking(tempRes);
                        RefreshList(); // Update the UI immediately after deleting
                    });
                }
                else
                {
                    Debug.LogWarning("Could not find a button named 'Btn_Delete' in the prefab!");
                }
            }
        }
    }
}