using UnityEngine;

public class LinkManager : MonoBehaviour
{
    // This method takes a string parameter so you can type the URL directly in the Unity Inspector
    public void OpenWebsite(string url)
    {
        // Unity's built-in command to open the default web browser
        Application.OpenURL(url);
    }
}