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
        Invoke("RefreshUI", 0.6f);
    }

    public void RefreshUI()
    {
        foreach (Transform child in contentContainer) 
        {
            Destroy(child.gameObject);
        }

        if (AssignmentManager.Instance == null) return;

        foreach (AssignmentData assignment in AssignmentManager.Instance.activeAssignments)
        {
            if (assignment.isCompleted) continue;

            GameObject newEntry = Instantiate(assignmentPrefab, contentContainer, false);
            newEntry.transform.localScale = Vector3.one;

            Transform textObj = newEntry.transform.Find("Text_Instruction");
            Transform inputObj = newEntry.transform.Find("InputField_Answer");
            Transform submitObj = newEntry.transform.Find("Button_Submit");

            if (textObj != null) 
            {
                string finalText = "";
                
                // --- FIXED: Removed the automatic <i> tags so you have full control using Rich Text! ---
                if (!string.IsNullOrEmpty(assignment.narrativeText))
                {
                    finalText += $"{assignment.narrativeText}\n\n";
                }
                
                finalText += $"<b>Opdracht:</b> {assignment.assignmentText}";
                
                textObj.GetComponent<TextMeshProUGUI>().text = finalText;
            }

            if (assignment.category == AssignmentCategory.Question)
            {
                if (inputObj != null) inputObj.gameObject.SetActive(true);
                if (submitObj != null) submitObj.gameObject.SetActive(true);

                Button submitBtn = submitObj.GetComponent<Button>();
                TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();

                AssignmentData tempAssignment = assignment;

                submitBtn.onClick.RemoveAllListeners();
                submitBtn.onClick.AddListener(() => 
                {
                    bool isCorrect = false;

                    if (tempAssignment.questionType == AssignmentQuestionType.AmountOfPeople) 
                    {
                        if (int.TryParse(inputField.text, out int result)) 
                        {
                            isCorrect = AssignmentManager.Instance.CheckIntAnswer(tempAssignment, result);
                        }
                    } 
                    else 
                    {
                        isCorrect = AssignmentManager.Instance.CheckStringAnswer(tempAssignment, inputField.text);
                    }

                    if (!isCorrect) 
                    {
                        inputField.text = ""; 
                    }
                });
            }
            else if (assignment.category == AssignmentCategory.Action)
            {
                if (inputObj != null) inputObj.gameObject.SetActive(false);
                if (submitObj != null) submitObj.gameObject.SetActive(false);
            }
        }
    }
}