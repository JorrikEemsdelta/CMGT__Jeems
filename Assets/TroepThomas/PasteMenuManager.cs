using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PasteMenuManager : MonoBehaviour
{
    public Button plakKnop;

    public FolderListManager listManager;

    void Awake()
    {
        // Koppel de knop aan de functie
        if (plakKnop != null)
        {
            plakKnop.onClick.AddListener(PlakActie);
        }
        Hide(); // Begin onzichtbaar
    }

    void Update()
    {
        // Als het menu aan staat en je klikt met links...
        if (gameObject.activeSelf && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            // ...dan verdwijnt hij. 
            // De delay voorkomt dat hij sluit door de klik die hem opent.
            Invoke("Hide", 0.1f);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        plakKnop.interactable = true;
        if (Pointer.current != null)
        {
            // Zet het menu op de muispositie
            Vector2 mousePos = Pointer.current.position.ReadValue();
            transform.position = mousePos;
        }
    }

    public void PlakActie()
    {
        if (listManager != null)
        {
            listManager.PlakItem();
        }
        Hide();
        // Voeg dit toe om dubbelklikken te voorkomen:
        plakKnop.interactable = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
