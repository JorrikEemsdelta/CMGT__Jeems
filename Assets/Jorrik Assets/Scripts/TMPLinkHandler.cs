using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI tmpText;
    [SerializeField] private Camera worldCamera;

    [Header("Document Links")]
    public string linkEemsdeltaDichtbij = "https://www.google.com/maps";
    public string linkHandboekDigitaal = "https://www.google.com/maps";
    public string linkAVG = "https://www.google.com/maps";
    public string linkOneGov = "https://www.google.com/maps";
    public string linkZoWerktEemsdelta = "https://www.google.com/maps";

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        if (worldCamera == null) worldCamera = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpText, eventData.position, worldCamera);

        if (linkIndex != -1)
        {
            // We halen de ID op en maken hem klein (lowercase)
            string fileID = tmpText.textInfo.linkInfo[linkIndex].GetLinkID().ToLower().Trim();
            Debug.Log("Systeem checkt nu ID: " + fileID);

            if (fileID.Contains("onegov"))
            {
                Debug.Log("Match gevonden: Opening OneGov link: " + linkOneGov);
                Application.OpenURL(linkOneGov);
            }
            else if (fileID.Contains("dichtbij"))
            {
                Application.OpenURL(linkEemsdeltaDichtbij);
            }
            else if (fileID.Contains("handboek") && fileID.Contains("digitaal"))
            {
                Application.OpenURL(linkHandboekDigitaal);
            }
            else if (fileID.Contains("avg") || fileID.Contains("informatiebeveiliging"))
            {
                Application.OpenURL(linkAVG);
            }
            else if (fileID.Contains("zo_werkt") || fileID.Contains("werkt_eemsdelta"))
            {
                Application.OpenURL(linkZoWerktEemsdelta);
            }
            else
            {
                Debug.LogWarning("Geen match gevonden in de IF-statements voor: " + fileID);
            }
        }
    }
}
 