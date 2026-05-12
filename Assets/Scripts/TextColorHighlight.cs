using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TextColorHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }
}