using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundRightClick : MonoBehaviour, IPointerClickHandler
{
    public FolderListManager manager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Vertel de hoofdmanager dat er op de achtergrond is geklikt
            manager.OpenPlakMenu();
        }
    }
}
