using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PasteMenuManager : MonoBehaviour
{
    public Button plakKnop;

    public FolderListManager listManager;

    void Awake()
    {
        // Set the function to the button
        if (plakKnop != null)
        {
            plakKnop.onClick.AddListener(PlakActie);
        }
        Hide(); //Begin invisible
    }

    void Update()
    {
        // uses wasPressedThisFrame
        if (gameObject.activeSelf && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            //create a small delay so you can't open up another menu at the same time or create errors
            Invoke("Hide", 0.1f);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        plakKnop.interactable = true;
        if (Pointer.current != null)
        {
            // Spawn the prefab at the mouse's position
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
        // Add this to prevent double-clicking
        plakKnop.interactable = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
