using System.Collections;
using UnityEngine;

public class QuestionButtonHover : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject startMenuCanvas; // Sleep hier je Startmenu Canvas in
    public GameObject chatCanvas;      // Sleep hier je Chat Canvas in

    /// <summary>
    /// Schakelt van het Startmenu naar de Chat.
    /// Koppel deze functie aan de 'On Click()' van je start/chat-knop.
    /// </summary>
    public void OpenChat()
    {
        if (startMenuCanvas != null) startMenuCanvas.SetActive(false);
        if (chatCanvas != null) chatCanvas.SetActive(true);
    }

    /// <summary>
    /// Schakelt van de Chat terug naar het Startmenu.
    /// Koppel deze functie aan de 'On Click()' van je 'X' (Sluit) knop.
    /// </summary>
    public void CloseChat()
    {
        if (chatCanvas != null) chatCanvas.SetActive(false);
        if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
    }
}