using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{

    // Update is called once per frame
    public void LoadDailyQuiz()
    {
        SceneManager.LoadScene("ThrillerDance");
    }
    public void QuitGame()
    {
        Application.Quit();
    }


}
