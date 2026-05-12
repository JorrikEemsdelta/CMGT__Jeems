using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject panelHome, panelFacilitaire, panelSchedule, panelConfirm, panelMyBookings;

    private void HideAll()
    {
        if(panelHome) panelHome.SetActive(false); 
        if(panelFacilitaire) panelFacilitaire.SetActive(false); 
        if(panelSchedule) panelSchedule.SetActive(false); 
        if(panelConfirm) panelConfirm.SetActive(false); 
        if(panelMyBookings) panelMyBookings.SetActive(false);
    }

    public void GoToHome() { HideAll(); panelHome.SetActive(true); }
    public void GoToFacilitaire() { HideAll(); panelFacilitaire.SetActive(true); }
   public void GoToSchedule()
{
    panelHome.SetActive(false);
    panelSchedule.SetActive(true);
    panelConfirm.SetActive(false);
}
    public void GoToConfirm() { HideAll(); panelConfirm.SetActive(true); } // <-- The missing line!
    public void GoToMyBookings() { HideAll(); panelMyBookings.SetActive(true); }
}