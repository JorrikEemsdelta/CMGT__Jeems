using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AssignmentUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject assignmentPrefab;
    public Transform contentContainer;

    // This runs when the UI screen becomes active and automatically triggers the assignment list redraw.
    void OnEnable()
    {
        RefreshAssignments();
    }

    // This completely redrafts the task card checklist. It clears old card gameobjects, fetches assignments, generates a card prefab for each task, styles the status text based on completion (green/red), toggles answer inputs, and attaches event listeners to submit inputs and dismiss tasks.
    public void RefreshAssignments()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (AssignmentManager.Instance == null) return;

        foreach (var task in AssignmentManager.Instance.assignments)
        {
            GameObject newCard = Instantiate(assignmentPrefab, contentContainer, false);
            newCard.transform.localScale = Vector3.one;

            Transform titleObj = newCard.transform.Find("Text_Title");
            Transform descObj = newCard.transform.Find("Text_Description");
            Transform statusObj = newCard.transform.Find("Text_Status"); 
            
            Transform inputObj = newCard.transform.Find("InputField_Answer");
            Transform btnObj = newCard.transform.Find("Button_Submit");
            
            // --- NEW: The Remove Button ---
            Transform removeBtnObj = newCard.transform.Find("Button_Remove");

            if (titleObj != null) titleObj.GetComponent<TextMeshProUGUI>().text = task.title;
            if (descObj != null) descObj.GetComponent<TextMeshProUGUI>().text = task.description;

            Assignment currentTask = task; // Prevents closure bug

            if (task.isCompleted)
            {
                if (statusObj != null)
                {
                    TextMeshProUGUI statusText = statusObj.GetComponent<TextMeshProUGUI>();
                    statusText.text = "VOLTOOID";
                    statusText.color = new Color(0.1f, 0.6f, 0.1f);
                }
                
                // Hide input fields when completed
                if (inputObj != null) inputObj.gameObject.SetActive(false);
                if (btnObj != null) btnObj.gameObject.SetActive(false);

                // Show the remove button and hook up click event
                if (removeBtnObj != null)
                {
                    removeBtnObj.gameObject.SetActive(true);
                    Button removeBtn = removeBtnObj.GetComponent<Button>();
                    removeBtn.onClick.RemoveAllListeners();
                    removeBtn.onClick.AddListener(() => 
                    {
                        AssignmentManager.Instance.DismissTask(currentTask);
                    });
                }
            }
            else
            {
                if (statusObj != null)
                {
                    TextMeshProUGUI statusText = statusObj.GetComponent<TextMeshProUGUI>();
                    statusText.text = "NOG TE DOEN";
                    statusText.color = new Color(0.8f, 0.2f, 0.2f);
                }

                // Hide the remove button as long as the task is active
                if (removeBtnObj != null) removeBtnObj.gameObject.SetActive(false);

                bool isManual = task.type == AssignmentType.ManualQuestionText || task.type == AssignmentType.ManualQuestionNumber;
                
                if (inputObj != null) inputObj.gameObject.SetActive(isManual);
                if (btnObj != null) btnObj.gameObject.SetActive(isManual);

                if (isManual && inputObj != null && btnObj != null)
                {
                    TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
                    Button submitBtn = btnObj.GetComponent<Button>();
                    
                    submitBtn.onClick.RemoveAllListeners();
                    submitBtn.onClick.AddListener(() => 
                    {
                        if (currentTask.type == AssignmentType.ManualQuestionText)
                        {
                            bool correct = AssignmentManager.Instance.CheckStringAnswer(currentTask, inputField.text);
                            if (correct) RefreshAssignments(); 
                        }
                        else if (currentTask.type == AssignmentType.ManualQuestionNumber)
                        {
                            if (int.TryParse(inputField.text, out int result))
                            {
                                bool correct = AssignmentManager.Instance.CheckIntAnswer(currentTask, result);
                                if (correct) RefreshAssignments(); 
                            }
                        }
                    });
                }
            }
        }
    }
}