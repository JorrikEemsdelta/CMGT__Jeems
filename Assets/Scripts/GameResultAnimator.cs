using UnityEngine;

public class GameResultAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerRandomVictory()
    {
        animator.ResetTrigger("PlayFail");
        animator.ResetTrigger("PlayVictory");

        // Picks 0, 1, or 2
        int randomAnimation = Random.Range(0, 3);
        
        // This will print a message in your Unity Console
        Debug.Log("Playing Victory Animation Index: " + randomAnimation);

        animator.SetInteger("VictoryIndex", randomAnimation);
        animator.SetTrigger("PlayVictory");
    }

    public void TriggerRandomFail()
    {
        animator.ResetTrigger("PlayVictory");
        animator.ResetTrigger("PlayFail");

        // Picks 0, 1, or 2
        int randomAnimation = Random.Range(0, 3);
        
        // This will print a message in your Unity Console
        Debug.Log("Playing Fail Animation Index: " + randomAnimation);

        animator.SetInteger("FailIndex", randomAnimation);
        animator.SetTrigger("PlayFail");
    }
}