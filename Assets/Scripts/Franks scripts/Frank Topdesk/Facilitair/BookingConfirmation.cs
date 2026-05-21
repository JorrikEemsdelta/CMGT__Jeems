using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookingConfirmation : MonoBehaviour
{
    public TextMeshProUGUI summaryText; 
    public TextMeshProUGUI capacityWarningText; 
    public TMP_InputField descriptionInput, peopleInput;
    public Image peopleInputBackground; 
    public MenuController menuController; 
    
    private string activeRoom, activeDate;
    private int activeStart, activeEnd;
    private int activeCapacity;

    public void ClearSummary()
    {
        if (summaryText != null) summaryText.text = "";
        if (capacityWarningText != null) capacityWarningText.gameObject.SetActive(false);
    }

    public void SetupConfirmPage(string room, string date, int start, int end)
    {
        activeRoom = room; activeDate = date;
        activeStart = start; activeEnd = end;
        
        activeCapacity = BookingManager.Instance.GetRoomCapacity(room);
        
        string timeFormat = start.ToString("00") + ":00 - " + end.ToString("00") + ":00";
        summaryText.text = $"{room} | {date} | {timeFormat}\nMax capaciteit: {activeCapacity} personen";

        descriptionInput.text = ""; 
        peopleInput.text = "";
        peopleInputBackground.color = Color.white;
        
        if(capacityWarningText != null) 
        {
            capacityWarningText.text = ""; 
            capacityWarningText.gameObject.SetActive(false);
        }
    }

    public void SubmitBooking()
    {
        if (string.IsNullOrWhiteSpace(peopleInput.text)) 
        { 
            peopleInputBackground.color = Color.red; 
            return; 
        }

        if (!int.TryParse(peopleInput.text, out int peopleCount)) return;

        if (peopleCount > activeCapacity)
        {
            peopleInputBackground.color = new Color(1f, 0.5f, 0.5f);
            if(capacityWarningText != null) 
            {
                capacityWarningText.gameObject.SetActive(true);
                capacityWarningText.text = $"FOUT: Max {activeCapacity} personen!";
            }
            return;
        }

        BookingManager.Instance.AddPlayerBooking(activeRoom, activeDate, activeStart, activeEnd, descriptionInput.text, peopleCount);
        menuController.GoToHome(); 
    }

    public void GoBackToSchedule()
    {
        ClearSummary();
        menuController.GoToSchedule();
    }
}