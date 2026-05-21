using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
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
    }

    [System.Serializable]
    public class MetadataOpdracht
    {
        public string oudeCategorie;
        public string nieuweCategorie;
    }

    [Header("Instellingen")]
    public FolderListManager listManager;

    [Header("Verwijder Opdrachten")]
    public List<string> doelwitNamen;

    [Header("Knip en Plak Opdrachten")]
    public List<PlakOpdracht> plakOpdrachten;

    [Header("Naam Wijzigen Opdrachten")]
    public List<HernoemOpdracht> hernoemOpdrachten;

    [Header("Metadata Opdrachten")]
    public List<MetadataOpdracht> metadataOpdrachten;

    public void CheckOfNaamInLijstStaat(string verwijderdeNaam)
    {
        if (doelwitNamen.Contains(verwijderdeNaam))
        {
            Debug.Log("Een verwijder opdracht is voltooid!");
        }
    }

    public void CheckHernoemOpdracht(string oudeNaam, string nieuweNaam)
    {
        foreach (var opdracht in hernoemOpdrachten)
        {
            // We controleren of de oude naam klopt EN of de nieuwe naam klopt
            if (opdracht.oudeNaam == oudeNaam && opdracht.nieuweNaam == nieuweNaam)
            {
                Debug.Log("Een naam wijzigen opdracht is zojuist voltooid!");
                // Optioneel: Je zou hier ook een 'isVoltooid' bool kunnen toevoegen 
                // aan HernoemOpdracht als je wilt dat het maar één keer telt.
            }
        }
    }

    public void CheckPlakOpdracht(string geplakteNaam, string huidigeFolderNaam)
    {
        // TIJDELIJKE DEBUG: Wat ziet de computer?
        Debug.Log("CheckPlakOpdracht gestart! Ik plak nu: " + geplakteNaam + " in map: " + huidigeFolderNaam);

        foreach (var opdracht in plakOpdrachten)
        {
            if (!opdracht.isVoltooid)
            {
                // Laat per opdracht zien wat de eisen zijn
                Debug.Log("Check tegen opdracht: " + opdracht.bestandsNaam + " in " + opdracht.doelFolderNaam);

                if (opdracht.bestandsNaam == geplakteNaam && opdracht.doelFolderNaam == huidigeFolderNaam)
                {
                    opdracht.isVoltooid = true;
                    Debug.Log("Een knip en plak opdracht is zojuist behaald!");
                }
            }
        }
    }

    void Start()
    {
        // Voer de scan uit zodra het spel begint
        CheckOpdrachtConfiguratie();
    }

    public void CheckOpdrachtConfiguratie()
    {
        if (listManager == null || listManager.mainList == null) return;

        // Maak een lijst van alle namen die in opdrachten voorkomen
        List<string> kritiekeNamen = new List<string>();
        foreach (var p in plakOpdrachten) kritiekeNamen.Add(p.bestandsNaam);
        foreach (var h in hernoemOpdrachten) kritiekeNamen.Add(h.oudeNaam);

        // Scan alle mappen (we beginnen bij de mainList)
        ScanMappenVoorGevaar(listManager.mainList, kritiekeNamen);
    }

    private void ScanMappenVoorGevaar(FolderListManager.FolderContainer container, List<string> kritiekeNamen)
    {
        foreach (var item in container.items)
        {
            // 1. Check voor Verwijderen (Geldt voor ALLE opdrachten)
            if (kritiekeNamen.Contains(item.itemName) && item.kanVerwijderen)
            {
                Debug.LogWarning($"<color=yellow>LET OP:</color> Het bestand <b>'{item.itemName}'</b> is essentieel voor een opdracht, maar de speler kan het verwijderen. Dit kan de voortgang blokkeren!");
            }

            // 2. Check voor Hernoemen
            if (kritiekeNamen.Contains(item.itemName) && item.kanHernoemen)
            {
                // We kijken specifiek of dit item in de HERNOEM-lijst staat
                bool isHernoemOpdracht = false;
                foreach (var h in hernoemOpdrachten)
                {
                    if (h.oudeNaam == item.itemName)
                    {
                        isHernoemOpdracht = true;
                        break;
                    }
                }

                // Alleen waarschuwen als het een PLAK-bestand is dat hernoemd mag worden.
                // Als het een hernoem-opdracht is, negeren we de waarschuwing (want dat MOET hernoemd worden).
                if (!isHernoemOpdracht)
                {
                    Debug.LogWarning($"<color=orange>LOGISCHE FOUT:</color> De naam van <b>'{item.itemName}'</b> is vereist voor een toekomstige opdracht (zoals plakken), maar mag nu gewijzigd worden. Dit maakt de opdracht onuitvoerbaar!");
                }
            }

            // Recursie voor submappen
            if (item.type == FolderListManager.ItemType.Folder && item.targetContainer != null)
            {
                ScanMappenVoorGevaar(item.targetContainer, kritiekeNamen);
            }
        }
    }

}
