using UnityEngine;
using TMPro;

public class MetadataMenuManager : MonoBehaviour
{
    public TMP_InputField categorieInput;
    private FolderListManager.FolderData gekozenFolder;
    private FolderListManager listManager;

    public void Show(FolderListManager.FolderData data, FolderListManager manager)
    {
        gekozenFolder = data;
        listManager = manager;

        gameObject.SetActive(true);
        transform.position = Input.mousePosition;

        // Fill in the text field with current metadata
        if (categorieInput != null)
        {
            categorieInput.text = data.categorieNaam;
        }
    }

    public void OpslaanMetadata()
    {
        if (gekozenFolder != null && categorieInput != null)
        {
            gekozenFolder.categorieNaam = categorieInput.text;

            // Refreshes the list
            listManager.DisplayCurrentList();
        }
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
