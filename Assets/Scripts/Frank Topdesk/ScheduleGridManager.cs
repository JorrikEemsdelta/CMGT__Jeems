using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScheduleGridManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rowPrefab;
    public GameObject freeSlotPrefab; 
    public GameObject takenSlotPrefab;
    public GameObject emptySpacePrefab; 
    public Transform contentPanel; 
    public BookingConfirmation confirmationScript; 
    public MenuController menuController; 

    [Header("Search Bar Inputs")]
    public TMP_InputField dateSearchInput;
    public TMP_InputField startTimeSearchInput;
    public TMP_InputField endTimeSearchInput;

    [Header("The Proceed Button")]
    public Button proceedButton; 

    public float baseWidthPerHour = 150f; 

    private string selectedRoom, selectedDate;
    private int selectedStart, selectedEnd;
    private Image lastSelectedImage;

    void OnEnable() 
    { 
        if (string.IsNullOrEmpty(dateSearchInput.text)) dateSearchInput.text = "11 mei 2026";
        if (string.IsNullOrEmpty(startTimeSearchInput.text)) startTimeSearchInput.text = "9";
        if (string.IsNullOrEmpty(endTimeSearchInput.text)) endTimeSearchInput.text = "17";
        
        if (proceedButton != null) proceedButton.gameObject.SetActive(false);
        if (confirmationScript != null) confirmationScript.ClearSummary();
        
        GenerateGrid(); 
    }

    public void RefreshGrid() { GenerateGrid(); }

    void GenerateGrid()
    {
        string searchDate = dateSearchInput.text;
        int reqStart = 9, reqEnd = 17;
        if (!string.IsNullOrEmpty(startTimeSearchInput.text)) int.TryParse(startTimeSearchInput.text, out reqStart);
        if (!string.IsNullOrEmpty(endTimeSearchInput.text)) int.TryParse(endTimeSearchInput.text, out reqEnd);

        reqStart = Mathf.Clamp(reqStart, 9, 16);
        reqEnd = Mathf.Clamp(reqEnd, reqStart + 1, 17);

        if (proceedButton != null) proceedButton.gameObject.SetActive(false);
        if (lastSelectedImage != null) lastSelectedImage.color = Color.white;
        if (confirmationScript != null) confirmationScript.ClearSummary();

        foreach (Transform child in contentPanel) Destroy(child.gameObject);

        foreach (RoomData room in BookingManager.Instance.rooms)
        {
            GameObject newRow = Instantiate(rowPrefab, contentPanel);
            newRow.GetComponentInChildren<TextMeshProUGUI>().text = $"{room.roomName} ({room.capacity})";
            Transform slotsParent = newRow.transform.Find("SlotsContainer"); 

            int currentHour = 9; 

            while (currentHour < 17)
            {
                Reservation existing = BookingManager.Instance.GetBookingStartingAt(room.roomName, searchDate, currentHour);

                if (existing != null)
                {
                    GameObject takenSlot = Instantiate(takenSlotPrefab, slotsParent);
                    takenSlot.GetComponentInChildren<TextMeshProUGUI>().text = existing.description;
                    int duration = existing.endHour - existing.startHour;
                    takenSlot.GetComponent<LayoutElement>().preferredWidth = duration * baseWidthPerHour;
                    currentHour = existing.endHour; 
                }
                else
                {
                    int nextObstacle = 17;
                    for (int h = currentHour + 1; h < 17; h++)
                    {
                        if (BookingManager.Instance.GetBookingStartingAt(room.roomName, searchDate, h) != null) { nextObstacle = h; break; }
                    }

                    int blockEnd;
                    bool canBookHere = false;
                    bool roomIsOccupiedInSearch = !BookingManager.Instance.IsSlotFree(room.roomName, searchDate, reqStart, reqEnd);

                    if (currentHour >= reqStart && currentHour < reqEnd)
                    {
                        blockEnd = Mathf.Min(nextObstacle, reqEnd);
                        canBookHere = !roomIsOccupiedInSearch;
                    }
                    else
                    {
                        blockEnd = (currentHour < reqStart) ? Mathf.Min(nextObstacle, reqStart) : nextObstacle;
                        canBookHere = false;
                    }

                    int duration = blockEnd - currentHour;

                    if (canBookHere)
                    {
                        GameObject freeSlot = Instantiate(freeSlotPrefab, slotsParent);
                        freeSlot.GetComponent<LayoutElement>().preferredWidth = duration * baseWidthPerHour;
                        int s = currentHour; int e = blockEnd; string r = room.roomName;
                        Image slotImage = freeSlot.GetComponent<Image>();
                        Button btn = freeSlot.GetComponent<Button>();
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SelectSlot(r, searchDate, s, e, slotImage));
                    }
                    else
                    {
                        GameObject emptySlot = Instantiate(emptySpacePrefab, slotsParent);
                        emptySlot.GetComponent<LayoutElement>().preferredWidth = duration * baseWidthPerHour;
                    }
                    currentHour = blockEnd; 
                }
            }
        }
    }

    void SelectSlot(string room, string date, int start, int end, Image img)
    {
        if (lastSelectedImage != null) lastSelectedImage.color = Color.white;
        selectedRoom = room; selectedDate = date; selectedStart = start; selectedEnd = end;
        img.color = new Color(0.7f, 0.85f, 1f);
        lastSelectedImage = img;
        if (proceedButton != null) proceedButton.gameObject.SetActive(true);

        confirmationScript.SetupConfirmPage(selectedRoom, selectedDate, selectedStart, selectedEnd);
    }

    public void ProceedToConfirmation()
    {
        menuController.GoToConfirm();
    }
}