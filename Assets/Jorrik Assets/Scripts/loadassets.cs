using System.Collections;
using UnityEngine;

public class loadassets : MonoBehaviour
{
    [Header("Instellingen")]
    public GameObject laadSchermCanvas; // Sleep hier eventueel een zwart UI-vlak in
    public float warmupTijd = 0.5f;     // Hoe lang de camera ronddraait

    private bool isWarmupKlaar = false;

    void Start()
    {
        // Start direct met de camera-spin setup
        StartCoroutine(DoCameraWarmup());
    }

    IEnumerator DoCameraWarmup()
    {
        // Toon het laadscherm zodat de speler de flitsende rotatie niet ziet
        if (laadSchermCanvas != null) laadSchermCanvas.SetActive(true);

        float timer = 0f;
        Quaternion startRotatie = transform.rotation;

        while (timer < warmupTijd)
        {
            timer += Time.deltaTime;
            // Laat de camera razendsnel om zijn as tollen om alles in beeld te dwingen
            transform.Rotate(Vector3.up, 720f * Time.deltaTime);
            yield return null;
        }

        // Herstel de originele rotatie van de camera
        transform.rotation = startRotatie;
        isWarmupKlaar = true;

        // Zet het laadscherm uit: de game kan nu vloeibaar beginnen!
        if (laadSchermCanvas != null) laadSchermCanvas.SetActive(false);

        // Activeer hier eventueel je startmenu canvas
    }
}