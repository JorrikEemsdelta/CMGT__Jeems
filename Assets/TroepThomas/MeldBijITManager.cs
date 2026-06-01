using UnityEngine;

public class MeldBijITManager : MonoBehaviour
{
    private FolderListManager.FolderData gekozenVirusData;
    private FolderListManager.FolderContainer huidigeContainer;
    private FolderListManager listManager;

    public void Show(FolderListManager.FolderData data, FolderListManager.FolderContainer container, FolderListManager manager)
    {
        gekozenVirusData = data;
        huidigeContainer = container;
        listManager = manager;

        gameObject.SetActive(true);

        // Find the mouse position (so it summons the specific "melden bij IT" prefab and not the regular Context Menu Manager)
        Vector2 muisPositie = Vector2.zero;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            muisPositie = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }

        // Set the button to the specific place of where the mouse is
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = muisPositie;
        }
        else
        {
            transform.position = muisPositie;
        }
    }

    // Deze functie koppel je aan de UI-knop zelf via de Inspector
    public void MeldBijITKlik()
    {
        if (gekozenVirusData != null && huidigeContainer != null && listManager != null)
        {
            // Remove the file
            huidigeContainer.items.Remove(gekozenVirusData);
            
            //Debug.Log translation: Debug.Log [Specific Virus has been deleted and IT has been notified]
            Debug.Log($"{gekozenVirusData.itemName} is succesvol gemeld bij IT en verwijderd!");

            // Refresh so the virus effect stops
            listManager.DisplayCurrentList();
        }
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
