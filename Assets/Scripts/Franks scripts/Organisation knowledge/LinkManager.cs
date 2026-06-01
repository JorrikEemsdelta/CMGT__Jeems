using UnityEngine;

public class LinkManager : MonoBehaviour
{
    // This method takes a URL string and uses Unity's Application.OpenURL to launch it in the device's default web browser.
    public void OpenWebsite(string url)
    {
        // Unity's built-in command to open the default web browser
        Application.OpenURL(url);
    }
}