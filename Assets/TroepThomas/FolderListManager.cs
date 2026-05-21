using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

using UnityEngine.EventSystems; // Nodig voor rechtsklik detectie

using TMPro;



public class FolderListManager : MonoBehaviour

{

    public enum ItemType { Folder, Bestand }



    public enum VirusType { DraaiAchtergrond, GlitchTekst, ScrollManipulator }



    public QuestManager questManager;



    private bool isBezigMetPlakken = false;



    [System.Serializable]

    public class FolderData

    {

        [Tooltip("De naam van het bestand of folder.")]

        public string itemName;

        [Tooltip("Kies hier uit of het een bestand of folder moet zijn. Folders kan je openen, en bestanden kun je bewerken of verwijderen.")]

        public ItemType type;



        [Tooltip("Klik dit aan als je wil dat het bestand verwjiderbaar is.")]

        public bool kanVerwijderen = true;

        [Tooltip("Klik dit aan als je wil dat de bestandsnaam gewijzigd kan worden.")]

        public bool kanHernoemen = true;

        [Tooltip("Klik dit aan als je wil dat de folder pas verschijnt na een aantal secondes. Dit kan bijvoorbeeld gebruikt worden voor virussen.")]

        public float verschijnVertraging = 0f;



        [Header("Categorie Instellingen")]

        [Tooltip("Voer hier de naam in van de categorie van de folder. Dit wordt 'metadata' genoemd in het spel.")]

        public string categorieNaam = "Ongecategoriseerd";

        [Tooltip("Voer hier de kleur in van de categorie van de folder. Dit wordt 'metadata' genoemd in het spel. Ga naar CategorieIndex aan de linkerkant om standaard kleuren in te stellen die dezen overschrijden, zodat je niet steeds dezelfde kleur hoeft te kiezen voor dezelfde categorie.")]

        public Color categorieKleur = Color.gray;



        [Header("Alleen invullen bij Folder")]

        public FolderContainer targetContainer;



        [Header("Virus Opties")]

        [Tooltip("Selecteer of dit bestand een virus is of niet.")]

        public bool isVirus = false;

        [Tooltip("Selecteer het soort virus. Draai Achtergrond draait de achtergrond, Glitch Text laat de tekst bovenaan het scherm veranderen, en Scroll Manipulator neemt de controle van het scrollwiel weg. Iets hier kiezen terwijl je Is Virus leeg laat zorgt ervoor dat er geen effecten plaatsnemen. Je kunt in Virus Manager aan de linkerkant deze effecten aanpassen.")]

        public VirusType typeVirus;



        [HideInInspector]

        public bool isZichtbaar = false;

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

    public MetadataMenuManager metadataMenu;



    private FolderData gekniptItemData;

    private FolderContainer bronContainer;



    void Start()

    {

        if (terugKnop != null) terugKnop.onClick.AddListener(GaTerug);

        if (contextMenu != null) contextMenu.Hide(); // Start verborgen



        OpenNieuweLijst(mainList);



        InitieerTijdgestuurdeBestanden(mainList);

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



            if (data.verschijnVertraging > 0f && !data.isZichtbaar)

            {

                continue;

            }

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

            // Rechtsklik op een BESTAND (Context Menu: Verwijderen, Hernoemen, Knippen)

            if (data.type == ItemType.Bestand && contextMenu != null)

            {

                contextMenu.Show(itemVisual, data, container);

            }

            // NIEUW: Rechtsklik op een FOLDER (Metadata Menu: Categorie aanpassen)

            else if (data.type == ItemType.Folder && metadataMenu != null)

            {

                metadataMenu.Show(data, this);

            }

        }

        else if (eventData.button == PointerEventData.InputButton.Left)

        {

            // Linksklik op een FOLDER (Map openen)

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



    public void DisplayCurrentList()

    {

        DisplayList(huidigeContainer);

    }



    private void InitieerTijdgestuurdeBestanden(FolderContainer container)

    {

        if (container == null || container.items == null) return;



        foreach (FolderData data in container.items)

        {

            if (data.verschijnVertraging > 0f)

            {

                data.isZichtbaar = false; // Verberg hem voor nu

                StartCoroutine(SpawnBestandNaTijd(data, data.verschijnVertraging));

            }

            else

            {

                data.isZichtbaar = true; // Gewone bestanden zijn meteen zichtbaar

            }



            // Als dit item een map is, moeten we ook de inhoud van die map checken (recursie)

            if (data.type == ItemType.Folder && data.targetContainer != null)

            {

                InitieerTijdgestuurdeBestanden(data.targetContainer);

            }

        }

    }



    private System.Collections.IEnumerator SpawnBestandNaTijd(FolderData data, float tijd)

    {

        yield return new WaitForSeconds(tijd);

        data.isZichtbaar = true; // Zet hem op zichtbaar!



        // Als de speler toevallig NU in de map kijkt waar dit bestand hoort,

        // moeten we het scherm meteen verversen zodat hij oppopt.

        DisplayCurrentList();

    }



    // Checkt of er �berhaupt een virus van een specifiek type actief is in de huidige map

    public bool IsVirusTypeActief(VirusType type)

    {

        if (huidigeContainer == null || huidigeContainer.items == null) return false;



        foreach (FolderData data in huidigeContainer.items)

        {

            // Check of het item gespawned is, een virus is, �n van het juiste type is

            if (data.isZichtbaar && data.isVirus && data.typeVirus == type)

            {

                return true;

            }

        }

        return false;

    }



    // Eventuele algemene check (handig voor als je wilt weten of er *iets* aan de hand is)

    public bool IsErEenVirusActief()

    {

        if (huidigeContainer == null || huidigeContainer.items == null) return false;

        foreach (FolderData data in huidigeContainer.items)

        {

            if (data.isZichtbaar && data.isVirus) return true;

        }

        return false;

    }

}

