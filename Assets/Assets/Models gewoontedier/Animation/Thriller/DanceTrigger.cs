using UnityEngine;
using UnityEngine.InputSystem; // We need this to use the New Input System!

[RequireComponent(typeof(Animator))]
public class DanceTrigger : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Make sure a keyboard is connected, then check if the spacebar was pressed
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            anim.SetTrigger("StartDance");
        }
    }
}