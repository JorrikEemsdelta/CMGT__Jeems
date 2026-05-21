using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTopDesk : MonoBehaviour
{
    // This specific method loads the TopDesk scene
    public void LoadTopDesk()
    {
        SceneManager.LoadScene("TopDesk");
    }

    // Optional: A flexible method where you can type the scene name in the Inspector
    public void LoadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}