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
        
        Debug.Log("Playing Fail Animation Index: " + randomAnimation);

        animator.SetInteger("FailIndex", randomAnimation);
        animator.SetTrigger("PlayFail");
    }

    // NEW: Forces the animator back to the Idle state
    public void ReturnToIdle()
    {
        animator.ResetTrigger("PlayVictory");
        animator.ResetTrigger("PlayFail");
        
        // Note: Make sure your idle animation state is named exactly "Idle" in the Animator window!
        animator.Play("Idle"); 
    }
}