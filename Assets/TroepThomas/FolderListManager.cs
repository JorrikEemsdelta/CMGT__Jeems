using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Nodig voor rechtsklik detectie
using TMPro;

public class FolderListManager : MonoBehaviour
{
    public enum ItemType { Folder, Bestand }

    public QuestManager questManager;

    private bool isBezigMetPlakken = false;

    [System.Serializable]
    public class FolderData
    {
        public string itemName;
        public ItemType type;

        public bool kanVerwijderen = true;
        public bool kanHernoemen = true;

        [Header("Categorie Instellingen")]
        public string categorieNaam = "Ongecategoriseerd";
        public Color categorieKleur = Color.gray;

        [Header("Alleen invullen bij Folder")]
        public FolderContainer targetContainer;
    }

    [System.Serializable]
    public class FolderContainer
    {
        public string containerName;
        public List<FolderData> items;
    }

    [Header("Prefabs")]
    public GameObject folderPrefab;
    public GameObject bestandPrefab;
    public ContextMenuManager contextMenu; // Sleep hier je contextmenu-object naartoe

    [Header("UI Referenties")]
    public Transform contentParent;
    public Button terugKnop;
    public TextMeshProUGUI titelTekst;

    [Header("Instellingen")]
    public FolderContainer mainList;

    private Stack<FolderContainer> geschiedenis = new Stack<FolderContainer>();
    private FolderContainer huidigeContainer;

    [Header("Overig")]
    public PasteMenuManager pasteMenu; // Sleep je nieuwe plak-prefab hierheen
    public CategoryColorManager colorManager;

    private FolderData gekniptItemData;
    private FolderContainer bronContainer;

    void Start()
    {
        if (terugKnop != null) terugKnop.onClick.AddListener(GaTerug);
        if (contextMenu != null) contextMenu.Hide(); // Start verborgen

        OpenNieuweLijst(mainList);
    }

    public void OpenNieuweLijst(FolderContainer nieuweContainer)
    {
        if (nieuweContainer == null) return;
        if (huidigeContainer != null) geschiedenis.Push(huidigeContainer);
        DisplayList(nieuweContainer);
    }

    public void GaTerug()
    {
        if (geschiedenis.Count > 0)
        {
            FolderContainer vorigeContainer = geschiedenis.Pop();
            DisplayList(vorigeContainer);
        }
    }

    private Transform ZoekChildRecursief(Transform parent, string naam)
    {
        foreach (Transform child in parent)
        {
            if (child.name == naam) return child;
            Transform gevonden = ZoekChildRecursief(child, naam);
            if (gevonden != null) return gevonden;
        }
        return null;
    }

    private void DisplayList(FolderContainer container)
    {
        huidigeContainer = container;
        if (titelTekst != null) titelTekst.text = container.containerName;
        if (terugKnop != null) terugKnop.gameObject.SetActive(geschiedenis.Count > 0);

        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (FolderData data in container.items)
        {
            GameObject prefabToSpawn = (data.type == ItemType.Folder) ? folderPrefab : bestandPrefab;
            GameObject newItem = Instantiate(prefabToSpawn, contentParent);
            newItem.GetComponentInChildren<TextMeshProUGUI>().text = data.itemName;

            // 1. Zoek de componenten via de recursieve methode
            Transform tekstTransform = ZoekChildRecursief(newItem.transform, "CategorieTekst");
            Transform balkTransform = ZoekChildRecursief(newItem.transform, "CategorieBalk");

            // 2. Pas de tekst aan (zonder nieuwe variabelen aan te maken die al bestaan)
            if (tekstTransform != null)
            {
                var tComp = tekstTransform.GetComponent<TextMeshProUGUI>();
                if (tComp != null) tComp.text = data.categorieNaam;
            }

            // 3. Pas de kleur aan
            // 3. Pas de kleur aan
            if (balkTransform != null)
            {
                var bComp = balkTransform.GetComponent<RawImage>();
                if (bComp != null)
                {
                    Color definitieveKleur = data.categorieKleur;

                    // Check of er een centrale override is
                    if (colorManager != null)
                    {
                        definitieveKleur = colorManager.GetColorForCategory(data.categorieNaam, data.categorieKleur);
                    }

                    bComp.color = definitieveKleur;
                }
            }

            // 4. EventTrigger logica
            EventTrigger trigger = newItem.GetComponent<EventTrigger>();
            if (trigger == null) trigger = newItem.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => {
                OnPointerClick((PointerEventData)eventData, data, newItem, container);
            });

            trigger.triggers.Add(entry);
        }
    }

    private void OnPointerClick(PointerEventData eventData, FolderData data, GameObject itemVisual, FolderContainer container)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Alleen als het een bestand is
            if (data.type == ItemType.Bestand && contextMenu != null)
            {
                // HIER GAAT HET OM: Je moet nu 3 dingen meesturen tussen de haakjes!
                contextMenu.Show(itemVisual, data, container);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (data.type == ItemType.Folder)
            {
                OpenNieuweLijst(data.targetContainer);
            }
        }
    }

 //   public void OnBackgroundClick(BaseEventData eventData)
 //   {
 //       
 //       PointerEventData pointerData = (PointerEventData)eventData;
//
  //      if (pointerData.button == PointerEventData.InputButton.Right)
  //      {
 //           // Sluit het andere menu als dat nog open stond
 //           if (contextMenu != null) contextMenu.Hide();
 //
            // Open het plak menu
   //         if (pasteMenu != null) pasteMenu.Show();
   //     }
  //  }

    public void OpenPlakMenu()
    {
        // Sluit eerst het andere menu (verwijderen/knippen)
        if (contextMenu != null) contextMenu.Hide();

        // Toon het plak menu
        if (pasteMenu != null) pasteMenu.Show();
    }

    public void KnipItem(FolderData data, FolderContainer vanContainer, GameObject visual)
    {
        gekniptItemData = data;
        bronContainer = vanContainer;

        // Optioneel: Vernietig direct het visuele object in de huidige map
        Destroy(visual);
        Debug.Log("Item geknipt: " + data.itemName);
    }

    public void PlakItem()
    {
        // Als we al aan het plakken zijn, negeer deze tweede klik
        if (isBezigMetPlakken) return;

        if (gekniptItemData == null)
        {
            Debug.Log("Je probeert te plakken zonder iets te plakken hebben. Schaam je.");
            return;
        }

        isBezigMetPlakken = true; // Zet het slotje erop

        // --- Je bestaande logica ---
        string naamVanBestand = gekniptItemData.itemName;
        string naamVanMap = huidigeContainer.containerName;

        if (bronContainer != null) bronContainer.items.Remove(gekniptItemData);
        huidigeContainer.items.Add(gekniptItemData);

        if (questManager != null)
        {
            questManager.CheckPlakOpdracht(naamVanBestand, naamVanMap);
        }

        DisplayList(huidigeContainer);

        gekniptItemData = null;
        bronContainer = null;
        Debug.Log("Succesvol geplakt: " + naamVanBestand);

        // Haal het slotje er na een korte pauze weer af
        Invoke("ResetPlakSlot", 0.2f);
    }

    private void ResetPlakSlot()
    {
        isBezigMetPlakken = false;
    }

}
