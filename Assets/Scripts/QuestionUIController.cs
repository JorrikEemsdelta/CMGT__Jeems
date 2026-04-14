using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    Checkmark
}

public class QuestionUIController : MonoBehaviour
{
    [Header("Header Elements")]
    public TMP_Text titleText;    
    public TMP_Text questionText; 

    [Header("Layout Panels")]
    public GameObject multipleChoicePanel;
    public GameObject trueFalsePanel;
    public GameObject checkmarkPanel;

    [Header("Multiple Choice Setup")]
    public Button[] multipleChoiceButtons;       
    public TMP_Text[] multipleChoiceButtonTexts; 

    [Header("True/False Setup")]
    public Button trueButton;
    public Button falseButton;

    [Header("Checkmark Setup")]
    public Toggle singleCheckmark;      
    public TMP_Text checkmarkLabelText; 

    [Header("Explanation Overlay (Shows when wrong)")]
    public GameObject explanationPanel;      
    public TMP_Text explanationTextDisplay;  
    public Button continueButton;            

    public void SetupQuestion(QuestionType type, string title, string question, string[] answers = null)
    {
        if (titleText != null) titleText.text = title;
        if (questionText != null) questionText.text = question;

        if (multipleChoicePanel != null) multipleChoicePanel.SetActive(false);
        if (trueFalsePanel != null) trueFalsePanel.SetActive(false);
        if (checkmarkPanel != null) checkmarkPanel.SetActive(false);
        
        HideExplanation(); 

        switch (type)
        {
            case QuestionType.MultipleChoice:
                multipleChoicePanel.SetActive(true); 
                for (int i = 0; i < multipleChoiceButtons.Length; i++)
                {
                    if (answers != null && i < answers.Length)
                    {
                        multipleChoiceButtons[i].gameObject.SetActive(true);
                        if (multipleChoiceButtonTexts[i] != null) 
                            multipleChoiceButtonTexts[i].text = answers[i];
                    }
                    else multipleChoiceButtons[i].gameObject.SetActive(false);
                }
                break;
                
            case QuestionType.TrueFalse:
                trueFalsePanel.SetActive(true);
                break;
                
            case QuestionType.Checkmark:
                checkmarkPanel.SetActive(true);
                if (singleCheckmark != null) singleCheckmark.isOn = false; 
                if (answers != null && answers.Length > 0 && checkmarkLabelText != null) 
                    checkmarkLabelText.text = answers[0];
                break;
        }
    }

    public void ResetColors()
    {
        foreach (Button btn in multipleChoiceButtons) btn.GetComponent<Image>().color = Color.white;
        trueButton.GetComponent<Image>().color = Color.white;
        falseButton.GetComponent<Image>().color = Color.white;
    }

    public void SetMultipleChoiceColor(int buttonIndex, bool isCorrect)
    {
        multipleChoiceButtons[buttonIndex].GetComponent<Image>().color = isCorrect ? Color.green : Color.red;
    }

    public void SetTrueFalseColor(bool isTrueButton, bool isCorrect)
    {
        if (isTrueButton) trueButton.GetComponent<Image>().color = isCorrect ? Color.green : Color.red;
        else falseButton.GetComponent<Image>().color = isCorrect ? Color.green : Color.red;
    }
    
    public void ShowExplanation(string explanation, UnityEngine.Events.UnityAction onContinueClicked)
    {
        if (explanationPanel != null) explanationPanel.SetActive(true);
        if (explanationTextDisplay != null) explanationTextDisplay.text = explanation;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners(); 
            continueButton.onClick.AddListener(onContinueClicked); 
        }
    }

    public void HideExplanation()
    {
        if (explanationPanel != null) explanationPanel.SetActive(false);
    }
}