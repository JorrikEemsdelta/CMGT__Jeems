using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTopDesk : MonoBehaviour
{
    // This loads the scene named "TopDesk".
    public void LoadTopDesk()
    {
        SceneManager.LoadScene("TopDesk");
    }

    // This loads any Unity scene by passing in its name as a string parameter.
    public void LoadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}