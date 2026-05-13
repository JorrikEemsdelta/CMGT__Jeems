using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AssignmentUIController : MonoBehaviour
{
    public static AssignmentUIController Instance;

    [Header("UI References")]
    public GameObject assignmentPrefab; 
    public Transform contentContainer; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Give the AssignmentManager a moment to generate the first 3 assignments, then build the UI
        Invoke("RefreshUI", 0.6f);
    }

    public void RefreshUI()
    {
        // 1. Clear out the old UI list
        foreach (Transform child in contentContainer) 
        {
            Destroy(child.gameObject);
        }

        if (AssignmentManager.Instance == null) return;

        // 2. Loop through all assignments
        foreach (AssignmentData assignment in AssignmentManager.Instance.activeAssignments)
        {
            // We only show assignments that are NOT completed
            if (assignment.isCompleted) continue;

            // Spawn the prefab
            GameObject newEntry = Instantiate(assignmentPrefab, contentContainer);

            // Find the elements inside the prefab
            Transform textObj = newEntry.transform.Find("Text_Instruction");
            Transform inputObj = newEntry.transform.Find("InputField_Answer");
            Transform submitObj = newEntry.transform.Find("Button_Submit");

            // Set the instruction text
            if (textObj != null) 
            {
                textObj.GetComponent<TextMeshProUGUI>().text = assignment.assignmentText;
            }

            // Handle UI based on Question vs Action
            if (assignment.category == AssignmentCategory.Question)
            {
                // Ensure the input field and button are visible
                if (inputObj != null) inputObj.gameObject.SetActive(true);
                if (submitObj != null) submitObj.gameObject.SetActive(true);

                Button submitBtn = submitObj.GetComponent<Button>();
                TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();

                // Save this specific assignment to a temporary variable for the button click
                AssignmentData tempAssignment = assignment;

                submitBtn.onClick.RemoveAllListeners();
                submitBtn.onClick.AddListener(() => 
                {
                    bool isCorrect = false;

                    // Check Amount of People (Integer)
                    if (tempAssignment.questionType == AssignmentQuestionType.AmountOfPeople) 
                    {
                        if (int.TryParse(inputField.text, out int result)) 
                        {
                            isCorrect = AssignmentManager.Instance.CheckIntAnswer(tempAssignment, result);
                        }
                    } 
                    // Check Name/Booker (String)
                    else 
                    {
                        isCorrect = AssignmentManager.Instance.CheckStringAnswer(tempAssignment, inputField.text);
                    }

                    // If wrong, clear the text box so they can try again
                    if (!isCorrect) 
                    {
                        inputField.text = "";
                    }
                });
            }
            else if (assignment.category == AssignmentCategory.Action)
            {
                // It's an action! They complete this by actually booking a room, so hide the input boxes
                if (inputObj != null) inputObj.gameObject.SetActive(false);
                if (submitObj != null) submitObj.gameObject.SetActive(false);
            }
        }
    }
}