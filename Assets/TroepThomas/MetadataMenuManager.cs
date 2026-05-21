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

        // Vul het tekstveld alvast met de huidige metadata (categorie)
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

            // Ververs de lijst zodat de nieuwe metadata direct zichtbaar is
            listManager.DisplayCurrentList();
        }
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
