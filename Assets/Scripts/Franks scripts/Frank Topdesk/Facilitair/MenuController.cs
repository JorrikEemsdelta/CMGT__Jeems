using UnityEngine;
using UnityEngine.SceneManagement; // Nodig om van scene te wisselen

public class MenuController : MonoBehaviour
{
    [Header("Main Dashboard")]
    public GameObject panelHome;

    [Header("Facilitair Management")]
    public GameObject panelFacilitaire; 
    public GameObject panelSchedule; 
    public GameObject panelConfirm; 
    public GameObject panelMyBookings;

    [Header("Security Management")]
    public GameObject panelSecuritySubMenu;
    public GameObject panelIncidentForm;
    public GameObject panelDataBreachForm;

    [Header("General Reports")]
    public GameObject panelGeneralReport;

    [Header("My Requests Overview")]
    public GameObject panelMyRequests;

    private void HideAll()
    {
        if(panelHome) panelHome.SetActive(false); 
        if(panelFacilitaire) panelFacilitaire.SetActive(false); 
        if(panelSchedule) panelSchedule.SetActive(false); 
        if(panelConfirm) panelConfirm.SetActive(false); 
        if(panelMyBookings) panelMyBookings.SetActive(false);

        if(panelSecuritySubMenu) panelSecuritySubMenu.SetActive(false);
        if(panelIncidentForm) panelIncidentForm.SetActive(false);
        if(panelDataBreachForm) panelDataBreachForm.SetActive(false);
        
        if(panelGeneralReport) panelGeneralReport.SetActive(false);
        
        if(panelMyRequests) panelMyRequests.SetActive(false);
    }

    // --- Bestaande Navigatie ---
    public void GoToHome() { HideAll(); panelHome.SetActive(true); }
    public void GoToFacilitaire() { HideAll(); panelFacilitaire.SetActive(true); }
    public void GoToSchedule() { HideAll(); panelSchedule.SetActive(true); } 
    public void GoToConfirm() { HideAll(); panelConfirm.SetActive(true); } 
    public void GoToMyBookings() { HideAll(); panelMyBookings.SetActive(true); }

    public void GoToSecuritySubMenu() { HideAll(); panelSecuritySubMenu.SetActive(true); }
    public void GoToIncidentForm() { HideAll(); panelIncidentForm.SetActive(true); }
    public void GoToDataBreachForm() { HideAll(); panelDataBreachForm.SetActive(true); }
    public void GoToGeneralReport() { HideAll(); panelGeneralReport.SetActive(true); }
    public void GoToMyRequests() { HideAll(); panelMyRequests.SetActive(true); }

    // --- NIEUWE FUNCTIE: Terug naar StartMenu scene ---
    public void GoToMainMenu()
    {
        // Laadt de scene genaamd StartMenu
        SceneManager.LoadScene("StartMenu"); 
    }
}