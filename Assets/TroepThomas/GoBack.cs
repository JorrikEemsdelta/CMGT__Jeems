using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBack : MonoBehaviour
{

    public void GotoMain()
    {
  
        SceneManager.LoadScene("StartMenu");
    }

    public void GotoFileTraining()
    {

        SceneManager.LoadScene("ThomasScene");
    }
}
