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

        // Haal de muispositie op de juiste manier op voor het New Input System
        Vector2 muisPositie = Vector2.zero;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            muisPositie = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }

        // Zet de knop op de opgevraagde muispositie
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
            // Verwijder het virusbestand definitief uit de mappenstructuur
            huidigeContainer.items.Remove(gekozenVirusData);

            Debug.Log($"[ANTIVIRUS] {gekozenVirusData.itemName} is succesvol gemeld bij IT en verwijderd!");

            // Ververs de lijst direct zodat het virus verdwijnt en de effecten stoppen
            listManager.DisplayCurrentList();
        }
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
