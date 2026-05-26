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
    public Transform questContentParent; // De 'Content' van je rechter Scroll View
    public GameObject questTekstPrefab;   // Een simpele prefab met een TextMeshProUGUI component

    [Header("Opdrachten Lijsten")]
    public List<VerwijderOpdracht> verwijderOpdrachten;
    public List<PlakOpdracht> plakOpdrachten;
    public List<HernoemOpdracht> hernoemOpdrachten;
    public List<MetadataOpdracht> metadataOpdrachten;

    void Start()
    {
        CheckOpdrachtConfiguratie();
        UpdateQuestUI(); // Bouw de lijst op bij het opstarten
    }

    // --- REPARATIE & LOGICA CHECKS ---

    public void CheckOfNaamInLijstStaat(string verwijderdeNaam)
    {
        foreach (var opdracht in verwijderOpdrachten)
        {
            if (!opdracht.isVoltooid && opdracht.doelwitNaam == verwijderdeNaam)
            {
                opdracht.isVoltooid = true;
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
    {
        if (questContentParent == null || questTekstPrefab == null) return;

        // Maak de huidige UI lijst leeg
        foreach (Transform child in questContentParent)
        {
            Destroy(child.gameObject);
        }

        // 1. Verwijder Opdrachten tonen
        foreach (var o in verwijderOpdrachten)
        {
            if (!o.isVoltooid) MaakQuestTekstObject($"Verwijder {o.doelwitNaam}");
        }

        // 2. Knip en Plak Opdrachten tonen
        foreach (var o in plakOpdrachten)
        {
            if (!o.isVoltooid) MaakQuestTekstObject($"Knip en plak {o.bestandsNaam} naar {o.doelFolderNaam}");
        }

        // 3. Naam Wijzigen Opdrachten tonen
        foreach (var o in hernoemOpdrachten)
        {
            if (!o.isVoltooid) MaakQuestTekstObject($"Wijzig naam van {o.oudeNaam} naar {o.nieuweNaam}");
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

    // --- GEVAREN SCANNER ---

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
                }
            }

            if (item.type == FolderListManager.ItemType.Folder && item.targetContainer != null)
            {
                ScanMappenVoorGevaar(item.targetContainer, kritiekeNamen);
            }
        }
    }
}
