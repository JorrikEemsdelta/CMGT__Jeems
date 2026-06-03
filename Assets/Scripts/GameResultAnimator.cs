using UnityEngine;

public class GameResultAnimator : MonoBehaviour
{
    private Animator animator;

    // This runs automatically when the game object starts. It finds and connects to the Animator component so we can control animations.
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // This makes the character perform a victory animation. It resets other triggers and picks a random victory animation from 3 options (0, 1, or 2).
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

    // This makes the character perform a fail animation. It resets other triggers and picks a random fail animation from 3 options (0, 1, or 2).
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
}