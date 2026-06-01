using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundRightClick : MonoBehaviour, IPointerClickHandler
{
    public FolderListManager manager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Vertel de FolderListManager dat er op de achtergrond is geklikt
            // Tells the FolderListManager that the background has been clicked on
            manager.OpenPlakMenu();
        }
    }
}
