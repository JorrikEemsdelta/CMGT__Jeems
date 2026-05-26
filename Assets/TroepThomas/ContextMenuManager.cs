using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; // Nodig voor List

public class ContextMenuManager : MonoBehaviour
{
    private GameObject visualToDestroy;
    private FolderListManager.FolderData dataToRemove;
    private FolderListManager.FolderContainer sourceContainer;
    public QuestManager questManager;
    public FolderListManager listManager;

    void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Invoke("Hide", 0.2f);
        }
    }

    // We voegen de data en de container toe aan de Show functie
    public void Show(GameObject visual, FolderListManager.FolderData data, FolderListManager.FolderContainer container)
    {
        visualToDestroy = visual;
        dataToRemove = data;
        sourceContainer = container;

        gameObject.SetActive(true);

        if (Pointer.current != null)
        {
            Vector2 mousePos = Pointer.current.position.ReadValue();
            transform.position = mousePos;
        }
    }

    public void VerwijderItem()
    {
        if (dataToRemove != null && dataToRemove.kanVerwijderen)
        {
            // 1. Stuur de naam van het bestand naar de QuestManager
            if (questManager != null)
            {
                questManager.CheckOfNaamInLijstStaat(dataToRemove.itemName);
            }
            // Fallback als questManager op de ListManager gelinkt staat
            else if (listManager != null && listManager.questManager != null)
            {
                listManager.questManager.CheckOfNaamInLijstStaat(dataToRemove.itemName);
            }

            // 2. De standaard verwijder-logica
            if (sourceContainer != null)
            {
                sourceContainer.items.Remove(dataToRemove);
            }

            if (visualToDestroy != null)
            {
                Destroy(visualToDestroy);
            }

            Hide();
        }
        else
        {
            Debug.Log("Dit is een belangrijk bestand die je later nodig hebt!");
            Hide();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void KnipActie()
    {
        if (dataToRemove != null && listManager != null)
        {
            // Geef de data door aan de manager
            listManager.KnipItem(dataToRemove, sourceContainer, visualToDestroy);
        }
        Hide();
    }

    public void HernoemActie()
    {
        // Check eerst of het bestand hernoemd mag worden
        if (dataToRemove != null && !dataToRemove.kanHernoemen)
        {
            Debug.Log("De naam van dit bestand kan niet verandert worden!");
            Hide(); // Sluit alleen het menu
            return; // Stop de functie hier
        }

        if (visualToDestroy != null)
        {
            TMPro.TMP_InputField inputField = visualToDestroy.GetComponentInChildren<TMPro.TMP_InputField>(true);
            TMPro.TextMeshProUGUI statischeTekst = visualToDestroy.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (inputField != null && statischeTekst != null)
            {
                Hide();
                inputField.text = statischeTekst.text;
                statischeTekst.enabled = false;
                inputField.gameObject.SetActive(true);
                inputField.ActivateInputField();
                inputField.MoveTextEnd(false);

                inputField.onEndEdit.RemoveAllListeners();
                inputField.onEndEdit.AddListener((nieuweNaam) => {
                    BevestigHernoemen(nieuweNaam, inputField, statischeTekst);
                });
            }
        }
    }

    private void BevestigHernoemen(string nieuweNaam, TMPro.TMP_InputField input, TMPro.TextMeshProUGUI tekst)
    {
        if (!string.IsNullOrEmpty(nieuweNaam) && dataToRemove != null)
        {
            // 1. Onthoud de oude naam voor de check
            string oudeNaam = dataToRemove.itemName;

            // 2. Update de data en de UI
            dataToRemove.itemName = nieuweNaam;
            tekst.text = nieuweNaam;

            // 3. Stuur de oude en nieuwe naam naar de QuestManager
            if (questManager != null)
            {
                questManager.CheckHernoemOpdracht(oudeNaam, nieuweNaam);
            }
            else if (listManager != null && listManager.questManager != null)
            {
                listManager.questManager.CheckHernoemOpdracht(oudeNaam, nieuweNaam);
            }

            Debug.Log("Naam gewijzigd van " + oudeNaam + " naar " + nieuweNaam);
        }

        input.gameObject.SetActive(false);
        tekst.enabled = true;
    }
}
