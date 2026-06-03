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
    [Tooltip("The text UI component that displays the title of the question or section.")]
    public TMP_Text titleText;    
    [Tooltip("The text UI component that displays the actual question.")]
    public TMP_Text questionText; 

    [Header("Layout Panels")]
    [Tooltip("The panel containing multiple choice specific UI elements.")]
    public GameObject multipleChoicePanel;
    [Tooltip("The panel containing true/false specific UI elements.")]
    public GameObject trueFalsePanel;
    [Tooltip("The panel containing checkmark specific UI elements.")]
    public GameObject checkmarkPanel;

    [Header("Multiple Choice Setup")]
    [Tooltip("Array of buttons used for multiple choice answers.")]
    public Button[] multipleChoiceButtons;       
    [Tooltip("Array of text components inside the multiple choice buttons. Ensure the order matches the buttons.")]
    public TMP_Text[] multipleChoiceButtonTexts; 

    [Header("True/False Setup")]
    [Tooltip("The button corresponding to the 'True' answer.")]
    public Button trueButton;
    [Tooltip("The button corresponding to the 'False' answer.")]
    public Button falseButton;

    [Header("Checkmark Setup")]
    [Tooltip("The toggle component for a single checkmark answer.")]
    public Toggle singleCheckmark;      
    [Tooltip("The text label displayed next to the checkmark.")]
    public TMP_Text checkmarkLabelText; 

    [Header("Explanation Overlay (Shows when WRONG)")]
    [Tooltip("The panel that appears to explain a wrong answer.")]
    public GameObject explanationPanel;      
    [Tooltip("The text component displaying the explanation context.")]
    public TMP_Text explanationTextDisplay;  
    [Tooltip("The button to dismiss the explanation and try again or continue.")]
    public Button continueButton;            

    [Header("Right Explanation Overlay (Shows when CORRECT)")]
    [Tooltip("The panel that appears to explain a right answer.")]
    public GameObject rightExplanationPanel;      
    [Tooltip("The text component displaying the right explanation context.")]
    public TMP_Text rightExplanationText;  
    [Tooltip("The button to dismiss the right explanation and continue.")]
    public Button closeRightExplanationButton; 

    // Cached image components to avoid expensive GetComponent calls during runtime
    private Image[] _multipleChoiceImages;
    private Image _trueButtonImage;
    private Image _falseButtonImage;

    // This runs before Start. It caches/saves references to the Image components on the buttons so we don't have to keep searching for them during the game (which makes the game run faster).
    private void Awake()
    {
        if (multipleChoiceButtons != null)
        {
            _multipleChoiceImages = new Image[multipleChoiceButtons.Length];
            for (int i = 0; i < multipleChoiceButtons.Length; i++)
            {
                if (multipleChoiceButtons[i] != null)
                {
                    _multipleChoiceImages[i] = multipleChoiceButtons[i].GetComponent<Image>();
                }
            }
        }

        if (trueButton != null) _trueButtonImage = trueButton.GetComponent<Image>();
        if (falseButton != null) _falseButtonImage = falseButton.GetComponent<Image>();
    }

    // This sets up a new question on the screen. It updates the title and question texts, enables only the panel for the specific question type (Multiple Choice, True/False, or Checkmark), hides explanation overlays, and fills in the answer choices.
    public void SetupQuestion(QuestionType type, string title, string question, string[] answers = null)
    {
        if (titleText != null) titleText.text = title;
        if (questionText != null) questionText.text = question;

        // Set active states directly to avoid redundant calls (disabling all then enabling one)
        if (multipleChoicePanel != null) multipleChoicePanel.SetActive(type == QuestionType.MultipleChoice);
        if (trueFalsePanel != null) trueFalsePanel.SetActive(type == QuestionType.TrueFalse);
        if (checkmarkPanel != null) checkmarkPanel.SetActive(type == QuestionType.Checkmark);
        
        // Hide both explanation panels when setting up a new question
        HideExplanation(); 
        HideRightExplanation();

        switch (type)
        {
            case QuestionType.MultipleChoice:
                if (multipleChoiceButtons != null)
                {
                    int answerCount = answers != null ? answers.Length : 0;
                    for (int i = 0; i < multipleChoiceButtons.Length; i++)
                    {
                        bool hasAnswer = i < answerCount;
                        multipleChoiceButtons[i].gameObject.SetActive(hasAnswer);

                        if (hasAnswer && multipleChoiceButtonTexts != null && i < multipleChoiceButtonTexts.Length)
                        {
                            if (multipleChoiceButtonTexts[i] != null) 
                                multipleChoiceButtonTexts[i].text = answers[i];
                        }
                    }
                }
                break;
                
            case QuestionType.TrueFalse:
                break;
                
            case QuestionType.Checkmark:
                if (singleCheckmark != null) singleCheckmark.isOn = false; 
                if (answers != null && answers.Length > 0 && checkmarkLabelText != null) 
                    checkmarkLabelText.text = answers[0];
                break;
        }
    }

    // This resets all the button colors back to their default white color so they look fresh for the next question.
    public void ResetColors()
    {
        if (_multipleChoiceImages != null)
        {
            for (int i = 0; i < _multipleChoiceImages.Length; i++)
            {
                if (_multipleChoiceImages[i] != null) _multipleChoiceImages[i].color = Color.white;
            }
        }
        if (_trueButtonImage != null) _trueButtonImage.color = Color.white;
        if (_falseButtonImage != null) _falseButtonImage.color = Color.white;
    }

    // This colors a specific multiple-choice button green if the chosen answer was correct, or red if it was wrong.
    public void SetMultipleChoiceColor(int buttonIndex, bool isCorrect)
    {
        if (_multipleChoiceImages != null && buttonIndex >= 0 && buttonIndex < _multipleChoiceImages.Length)
        {
            if (_multipleChoiceImages[buttonIndex] != null)
                _multipleChoiceImages[buttonIndex].color = isCorrect ? Color.green : Color.red;
        }
    }

    // This colors the True or False buttons green if the answer is correct, or red if the answer is wrong.
    public void SetTrueFalseColor(bool isTrueButton, bool isCorrect)
    {
        if (isTrueButton && _trueButtonImage != null) 
            _trueButtonImage.color = isCorrect ? Color.green : Color.red;
        else if (!isTrueButton && _falseButtonImage != null) 
            _falseButtonImage.color = isCorrect ? Color.green : Color.red;
    }
    
    // --- WRONG ANSWER EXPLANATION METHODS ---
    // This shows an overlay explaining why the selected answer was wrong, and attaches a function to the continue button to proceed when clicked.
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

    // This hides the wrong answer explanation overlay from the screen.
    public void HideExplanation()
    {
        if (explanationPanel != null) explanationPanel.SetActive(false);
    }

    // --- RIGHT ANSWER EXPLANATION METHODS ---
    // This shows an overlay explaining more details about the correct answer, and attaches a function to the close button to proceed when clicked.
    public void ShowRightExplanation(string explanation, UnityEngine.Events.UnityAction onContinueClicked)
    {
        if (rightExplanationPanel != null) rightExplanationPanel.SetActive(true);
        if (rightExplanationText != null) rightExplanationText.text = explanation;

        if (closeRightExplanationButton != null)
        {
            closeRightExplanationButton.onClick.RemoveAllListeners(); 
            closeRightExplanationButton.onClick.AddListener(onContinueClicked); 
        }
    }

    // This hides the correct answer explanation overlay from the screen.
    public void HideRightExplanation()
    {
        if (rightExplanationPanel != null) rightExplanationPanel.SetActive(false);
    }
}