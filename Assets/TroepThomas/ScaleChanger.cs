using System.Collections;
using UnityEngine;

public class ScaleChanger : MonoBehaviour
{
    [Header("Settings")]
    public float minScale = 1f;
    public float maxScale = 100f;
    public float duration = 0.5f; // Hoe lang de animatie duurt
    public float waitTime = 1f;   // Hoe lang hij wacht op elk punt

    private void Start()
    {
        // Start de oneindige loop
        StartCoroutine(ScaleRoutine());
    }

    IEnumerator ScaleRoutine()
    {
        while (true)
        {
            // Schaal omhoog naar 100
            yield return StartCoroutine(LerpScale(maxScale));
            yield return new WaitForSeconds(waitTime);

            // Schaal terug naar 1
            yield return StartCoroutine(LerpScale(minScale));
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator LerpScale(float targetY)
    {
        float time = 0;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(startScale.x, targetY, startScale.z);

        while (time < duration)
        {
            // Bereken de nieuwe schaal op basis van de verstreken tijd
            transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null; // Wacht tot de volgende frame
        }

        // Zet hem exact op de eindwaarde om afrondingsfouten te voorkomen
        transform.localScale = endScale;
    }
}
