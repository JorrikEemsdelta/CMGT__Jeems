using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems; 
using UnityEngine.InputSystem; 

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
    public TMP_Dropdown dateDropdown; 
    public TMP_InputField startTimeSearchInput;
    public TMP_InputField endTimeSearchInput;

    [Header("The Proceed Button")]
    public Button proceedButton; 

    [Header("Tooltip Settings")]
    public GameObject tooltipPanel; 
    public TextMeshProUGUI tooltipText; 

    [Header("Grid Visual Settings")]
    public Transform gridBackgroundContainer; 
    public GameObject gridLinePrefab; 
    
    [Tooltip("How wide is your Room Name text box? This pushes 09:00 to the right so it doesn't overlap the text.")]
    public float gridLeftOffset = 200f; 
    
    [Tooltip("How wide is one hour? Decrease this (e.g. to 100 or 80) to make the whole day fit on your screen without scrolling.")]
    public float baseWidthPerHour = 100f; 

    private string selectedRoom, selectedDate;
    private int selectedStart, selectedEnd;
    private Image lastSelectedImage;
    private bool dropdownInitialized = false;

    // This runs when the script instance is initialized. It registers listeners on the dropdowns and text inputs so the schedule grid updates immediately when filters are changed.
    void Awake()
    {
        if (dateDropdown != null) 
            dateDropdown.onValueChanged.AddListener(delegate { RefreshGrid(); });

        if (startTimeSearchInput != null)
            startTimeSearchInput.onEndEdit.AddListener((val) => { RefreshGrid(); });
            
        if (endTimeSearchInput != null)
            endTimeSearchInput.onEndEdit.AddListener((val) => { RefreshGrid(); });
    }

    // This runs when the schedule panel is enabled. It populates the calendar dates dropdown, configures the default search boundaries (9:00 - 17:00), hides old confirmation details, and draws the grid.
    void OnEnable() 
    { 
        if (dateDropdown != null && !dropdownInitialized)
        {
            dateDropdown.ClearOptions();
            List<string> options = new List<string>(BookingManager.Instance.weekDates);
            dateDropdown.AddOptions(options);
            dropdownInitialized = true; 
        }

        if (string.IsNullOrEmpty(startTimeSearchInput.text)) startTimeSearchInput.text = "9";
        if (string.IsNullOrEmpty(endTimeSearchInput.text)) endTimeSearchInput.text = "17";
        
        if (proceedButton != null) proceedButton.gameObject.SetActive(false);
        if (confirmationScript != null) confirmationScript.ClearSummary();
        
        HideTooltip(); 
        GenerateGrid(); 
    }

    // This runs every frame and forces the booking description tooltip panel to follow the user's cursor position.
    void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                tooltipPanel.transform.position = new Vector3(mousePos.x + 15f, mousePos.y - 15f, 0f);
            }
        }
    }

    // This triggers a complete refresh of the schedule grid.
    public void RefreshGrid() { GenerateGrid(); }

    // This is the core generator logic. It reads filters, draws hour markers on the vertical grid columns, instantiates row panels for each room, queries occupied reservations to render unclickable descriptions (attaching hover tooltips), and instantiates clickable green slots for bookable areas.
    void GenerateGrid()
    {
        string searchDate = dateDropdown.options[dateDropdown.value].text;
        
        int reqStart = 9, reqEnd = 17;
        if (!string.IsNullOrEmpty(startTimeSearchInput.text)) int.TryParse(startTimeSearchInput.text, out reqStart);
        if (!string.IsNullOrEmpty(endTimeSearchInput.text)) int.TryParse(endTimeSearchInput.text, out reqEnd);

        if (reqStart < 9) reqStart = 9;
        if (reqEnd > 17) reqEnd = 17;
        if (reqStart > 16) reqStart = 16;
        if (reqEnd <= reqStart) reqEnd = reqStart + 1;

        startTimeSearchInput.text = reqStart.ToString();
        endTimeSearchInput.text = reqEnd.ToString();

        if (proceedButton != null) proceedButton.gameObject.SetActive(false);
        if (lastSelectedImage != null) lastSelectedImage.color = Color.white;
        if (confirmationScript != null) confirmationScript.ClearSummary();

        foreach (Transform child in contentPanel) 
        {
            if (gridBackgroundContainer != null && child == gridBackgroundContainer) continue;
            Destroy(child.gameObject);
        }

        if (gridBackgroundContainer != null)
        {
            foreach (Transform child in gridBackgroundContainer) Destroy(child.gameObject);
        }

        if (gridBackgroundContainer != null && gridLinePrefab != null)
        {
            for (int h = 9; h <= 17; h++)
            {
                GameObject lineObj = Instantiate(gridLinePrefab, gridBackgroundContainer);
                RectTransform rect = lineObj.GetComponent<RectTransform>();
                float xPos = gridLeftOffset + ((h - 9) * baseWidthPerHour);
                rect.anchoredPosition = new Vector2(xPos, 0);

                TextMeshProUGUI lineText = lineObj.GetComponentInChildren<TextMeshProUGUI>();
                if (lineText != null) lineText.text = $"{h:00}:00";
            }
        }

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
                    
                    EventTrigger trigger = takenSlot.AddComponent<EventTrigger>();
                    EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                    
                    string timeString = $"{existing.startHour:00}:00 - {existing.endHour:00}:00";
                    
                    // This is where the 'amountOfPeople' is pulled and put into the tooltip
                    string hoverInfo = $"<b>{existing.description}</b>\n{existing.roomName} | {existing.date} | {timeString}\nDoor: {existing.bookerName}\nAantal: {existing.amountOfPeople} personen";
                    
                    enter.callback.AddListener((data) => { ShowTooltip(hoverInfo); });
                    trigger.triggers.Add(enter);

                    EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                    exit.callback.AddListener((data) => { HideTooltip(); });
                    trigger.triggers.Add(exit);

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

    // This handles selecting a slot. It clears old visual selections, highlights the clicked slot blue, saves the booking boundaries, and notifies the confirmation overlay panel.
    void SelectSlot(string room, string date, int start, int end, Image img)
    {
        if (lastSelectedImage != null) lastSelectedImage.color = Color.white;
        selectedRoom = room; selectedDate = date; selectedStart = start; selectedEnd = end;
        img.color = new Color(0.7f, 0.85f, 1f);
        lastSelectedImage = img;
        if (proceedButton != null) proceedButton.gameObject.SetActive(true);

        confirmationScript.SetupConfirmPage(selectedRoom, selectedDate, selectedStart, selectedEnd);
    }

    // This transitions the player directly to the confirmation screen.
    public void ProceedToConfirmation()
    {
        menuController.GoToConfirm();
    }

    // This makes the reservation information tooltip visible and dynamically structures its height and position next to the mouse.
    public void ShowTooltip(string info)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = info;
            tooltipPanel.SetActive(true);
            tooltipText.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());
            tooltipPanel.transform.SetAsLastSibling();
        }
    }

    // This hides the reservation information tooltip from the screen.
    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}