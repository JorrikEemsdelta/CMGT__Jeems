using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    public class VerwijderOpdracht
    {
        public string doelwitNaam;
        public bool isVoltooid = false;
    }

    [System.Serializable]
    public class PlakOpdracht
    {
        public string bestandsNaam;
        public string doelFolderNaam;
        public bool isVoltooid = false;
    }

    [System.Serializable]
    public class HernoemOpdracht
    {
        public string oudeNaam;
        public string nieuweNaam;
        public bool isVoltooid = false;
    }

    [System.Serializable]
    public class MetadataOpdracht
    {
        public string oudeCategorie;
        public string nieuweCategorie;
        public bool isVoltooid = false;
    }

    [Header("Instellingen")]
    public FolderListManager listManager;

    [Header("UI Referenties")]
    public Transform questContentParent; 
    public GameObject questTekstPrefab;  

    [Header("Opdrachten Lijsten")]
    public List<VerwijderOpdracht> verwijderOpdrachten;
    public List<PlakOpdracht> plakOpdrachten;
    public List<HernoemOpdracht> hernoemOpdrachten;
    public List<MetadataOpdracht> metadataOpdrachten;

    void Start()
    {
        // Als we vergeten zijn de listManager te slepen, zoekt de computer hem nu zelf!
        if (listManager == null)
        {
            listManager = FindFirstObjectByType<FolderListManager>();
        }

        // Zorg dat de listManager DIRECT weet wie deze QuestManager is
        if (listManager != null)
        {
            listManager.questManager = this;
        }

        CheckOpdrachtConfiguratie();
        UpdateQuestUI();
    }


    //All of these three are very similar, as they check the assignments, whether they are done or not
    public void CheckOfNaamInLijstStaat(string verwijderdeNaam)
    {
        foreach (var opdracht in verwijderOpdrachten)
        {
            if (!opdracht.isVoltooid && opdracht.doelwitNaam == verwijderdeNaam)
            {
                opdracht.isVoltooid = true;
                //Debug.Log trnaslation: a deletion exercise has been completed! (Specific File)
                Debug.Log($"Een verwijder opdracht is voltooid! ({verwijderdeNaam})");
                UpdateQuestUI();
                return;
            }
        }
    }

    public void CheckHernoemOpdracht(string oudeNaam, string nieuweNaam)
    {
        foreach (var opdracht in hernoemOpdrachten)
        {
            if (!opdracht.isVoltooid && opdracht.oudeNaam == oudeNaam && opdracht.nieuweNaam == nieuweNaam)
            {
                opdracht.isVoltooid = true;
                //Debug.Log translation: a name change exercise has been completed! (Old name -> New Name)
                Debug.Log($"Een naam wijzigen opdracht is zojuist voltooid! ({oudeNaam} -> {nieuweNaam})");
                UpdateQuestUI();
                return;
            }
        }
    }

    public void CheckPlakOpdracht(string geplakteNaam, string huidigeFolderNaam)
    {
        foreach (var opdracht in plakOpdrachten)
        {
            if (!opdracht.isVoltooid && opdracht.bestandsNaam == geplakteNaam && opdracht.doelFolderNaam == huidigeFolderNaam)
            {
                opdracht.isVoltooid = true;
                //Debug.Log translation: a copy and pasta exercise has been completed! (Old name -> New Name)
                Debug.Log($"Een knip en plak opdracht is zojuist behaald! ({geplakteNaam} naar {huidigeFolderNaam})");
                UpdateQuestUI();
                return;
            }
        }
    }

    public void CheckMetadataOpdracht(string oudeCat, string nieuweCat)
    {
        foreach (var opdracht in metadataOpdrachten)
        {
            if (!opdracht.isVoltooid && opdracht.oudeCategorie == oudeCat && opdracht.nieuweCategorie == nieuweCat)
            {
                opdracht.isVoltooid = true;
                Debug.Log($"Een metadata opdracht is voltooid! ({oudeCat} -> {nieuweCat})");
                UpdateQuestUI();
                return;
            }
        }
    }

    // --- UI GENERATOR ---

    public void UpdateQuestUI()
        //this method here is for updating the text in the scrollrect.
        //all text here is automatically written Dutch that gives you basic instructions.
    {
        if (questContentParent == null || questTekstPrefab == null) return;

        foreach (Transform child in questContentParent)
        {
            Destroy(child.gameObject);
        }

        // 1. Verwijder Opdrachten tonen
        foreach (var o in verwijderOpdrachten)
        {
            if (!o.isVoltooid) MaakQuestTekstObject($"<color=white>Verwijder {o.doelwitNaam}</color>");
        }

        // 2. Knip en Plak Opdrachten tonen
        foreach (var o in plakOpdrachten)
        {
            if (!o.isVoltooid) MaakQuestTekstObject($"<color=white>Knip en plak {o.bestandsNaam} naar {o.doelFolderNaam}</color>");
        }

        // 3. Naam Wijzigen Opdrachten tonen
        foreach (var o in hernoemOpdrachten)
        {
            if (!o.isVoltooid) MaakQuestTekstObject($"<color=white>Wijzig naam van {o.oudeNaam} naar {o.nieuweNaam}</color>");
        }
    }

    private void MaakQuestTekstObject(string tekst)
    {
        GameObject nieuwTekstObj = Instantiate(questTekstPrefab, questContentParent);
        TextMeshProUGUI tmp = nieuwTekstObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = tekst;
        }
    }

  //This section of the code checks for possible problems. If an exercise is created where you can
  //delete a file that you need for later, the game will notify you.

    public void CheckOpdrachtConfiguratie()
    {
        if (listManager == null || listManager.mainList == null) return;

        List<string> kritiekeNamen = new List<string>();
        foreach (var p in plakOpdrachten) kritiekeNamen.Add(p.bestandsNaam);
        foreach (var h in hernoemOpdrachten) kritiekeNamen.Add(h.oudeNaam);

        ScanMappenVoorGevaar(listManager.mainList, kritiekeNamen);
    }

    private void ScanMappenVoorGevaar(FolderListManager.FolderContainer container, List<string> kritiekeNamen)
    {
        foreach (var item in container.items)
        {
            if (kritiekeNamen.Contains(item.itemName) && item.kanVerwijderen)
            {
                Debug.LogWarning($"<color=yellow>LET OP:</color> Het bestand <b>'{item.itemName}'</b> is essentieel voor een opdracht, maar de speler kan het verwijderen.");
                //warns you that the name can be deleted but is needed in a later assignemnt
            }

            if (kritiekeNamen.Contains(item.itemName) && item.kanHernoemen)
            {
                bool isHernoemOpdracht = false;
                foreach (var h in hernoemOpdrachten)
                {
                    if (h.oudeNaam == item.itemName) { isHernoemOpdracht = true; break; }
                }

                if (!isHernoemOpdracht)
                {
                    Debug.LogWarning($"<color=orange>LOGISCHE FOUT:</color> De naam van <b>'{item.itemName}'</b> is vereist voor een toekomstige opdracht, maar mag nu gewijzigd worden.");
                    //warns you that the name can be changed but the name is needed in a later assignment
                }
            }

            if (item.type == FolderListManager.ItemType.Folder && item.targetContainer != null)
            {
                ScanMappenVoorGevaar(item.targetContainer, kritiekeNamen);
            }
        }
    }
}
