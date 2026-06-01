using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; 

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

    public void VerwijderItem() //This is the method that deletes an item
    {
        if (dataToRemove != null && dataToRemove.kanVerwijderen)
        {
            //Sends the name to the Quest Manager script to check
            if (questManager != null)
            {
                questManager.CheckOfNaamInLijstStaat(dataToRemove.itemName);
            }
            //Fallback check, sends it to the list to check
            else if (listManager != null && listManager.questManager != null)
            {
                listManager.questManager.CheckOfNaamInLijstStaat(dataToRemove.itemName);
            }

            //This here is the deletion process
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
            //Translation of debug log: "This is an important file! You'll need this later!
            Debug.Log("Dit is een belangrijk bestand die je later nodig hebt!");
            Hide();
        }
    }

    public void Hide()
    {
        //hides the prefab whenever an action is done or a click outside the prefab area is done
        gameObject.SetActive(false);
    }

    public void KnipActie()
    {
        if (dataToRemove != null && listManager != null)
        {
            //Removes item from the List in ListManager
            listManager.KnipItem(dataToRemove, sourceContainer, visualToDestroy);
        }
        Hide();
    }

    public void HernoemActie()
    {
        // Quick check if the file is allowed to be renamed
        if (dataToRemove != null && !dataToRemove.kanHernoemen)
        {
            //Translation of debug log: "The name of this file cannot be changed!"
            Debug.Log("De naam van dit bestand kan niet verandert worden!");
            Hide(); 
            return;
        }

        //Summon the renaming prefab and Input Field to rename files
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

    //confirms renaming the file
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

            //Debug log translation: "Name changed from + oudeNaam, nieuweNaam);
            Debug.Log("Naam gewijzigd van " + oudeNaam + " naar " + nieuweNaam);
        }

        input.gameObject.SetActive(false);
        tekst.enabled = true;
    }
}
