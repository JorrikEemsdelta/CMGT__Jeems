using UnityEngine;
using UnityEngine.SceneManagement; // Needed to switch scenes

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

    // This disables all the UI panels in the Topdesk interface so we can show a clean canvas.
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
    // This hides all active menus and navigates directly to the Home dashboard panel.
    public void GoToHome() { HideAll(); panelHome.SetActive(true); }

    // This hides all active menus and navigates directly to the Facilitair Management submenu panel.
    public void GoToFacilitaire() { HideAll(); panelFacilitaire.SetActive(true); }

    // This hides all active menus and navigates directly to the Room Schedule panel.
    public void GoToSchedule() { HideAll(); panelSchedule.SetActive(true); } 

    // This hides all active menus and navigates directly to the Booking Confirmation panel.
    public void GoToConfirm() { HideAll(); panelConfirm.SetActive(true); } 

    // This hides all active menus and navigates directly to the My Bookings panel.
    public void GoToMyBookings() { HideAll(); panelMyBookings.SetActive(true); }

    // This hides all active menus and navigates directly to the Security Management submenu panel.
    public void GoToSecuritySubMenu() { HideAll(); panelSecuritySubMenu.SetActive(true); }

    // This hides all active menus and navigates directly to the Incident Form panel.
    public void GoToIncidentForm() { HideAll(); panelIncidentForm.SetActive(true); }

    // This hides all active menus and navigates directly to the Data Breach Form panel.
    public void GoToDataBreachForm() { HideAll(); panelDataBreachForm.SetActive(true); }

    // This hides all active menus and navigates directly to the General Report form panel.
    public void GoToGeneralReport() { HideAll(); panelGeneralReport.SetActive(true); }

    // This hides all active menus and navigates directly to the My Requests list panel.
    public void GoToMyRequests() { HideAll(); panelMyRequests.SetActive(true); }

    // --- NEW FUNCTION: Return to StartMenu scene ---
    // This loads the StartMenu scene, returning the player to the main menu.
    public void GoToMainMenu()
    {
        // Loads the scene named StartMenu
        SceneManager.LoadScene("StartMenu"); 
    }
}