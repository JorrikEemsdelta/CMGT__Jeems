using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TextColorHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    // This runs when the mouse cursor enters the area of the UI button, changing the text color to the highlight color (like yellow).
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = highlightColor;
    }

    // This runs when the mouse cursor leaves the area of the UI button, changing the text color back to the normal color (like white).
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }
}