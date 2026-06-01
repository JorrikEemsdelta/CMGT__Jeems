using UnityEngine;
using UnityEngine.UI; // NODIG VOOR DE SCROLLRECT
using TMPro;

public class VirusManager : MonoBehaviour
{
    public FolderListManager listManager;

    [Header("Draai Instellingen")]
    public float draaiSnelheid = 50f;

    [Header("Tekst Glitch Instellingen")]
    public TextMeshProUGUI doelwitTekst;
    public float glitchInterval = 0.1f;

    [Header("Scroll Manipulator Instellingen")]
    public ScrollRect doelwitScrollRect; 
    public float schokInterval = 2f;   

    private string origineleTekst;
    private float glitchTimer;
    private bool wasGlitchActief = false;
    private string glitchTekens = "!@#$%^&*()";

    private float scrollTimer;

    void Start()
    {
        if (doelwitTekst != null)
        {
            origineleTekst = doelwitTekst.text;
        }
    }

    void Update()
    {
        if (listManager == null) return;

       //this update method deals with the spinning background virus
        if (listManager.IsVirusTypeActief(FolderListManager.VirusType.DraaiAchtergrond))
        {
            transform.Rotate(0, 0, draaiSnelheid * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }


        //this update method deals with the scrambled text virus
        if (doelwitTekst != null)
        {
            bool isGlitchVirusActief = listManager.IsVirusTypeActief(FolderListManager.VirusType.GlitchTekst);

            if (isGlitchVirusActief)
            {
                if (!wasGlitchActief)
                {
                    origineleTekst = doelwitTekst.text;
                    wasGlitchActief = true;
                }

                glitchTimer += Time.deltaTime;
                if (glitchTimer >= glitchInterval)
                {
                    doelwitTekst.text = GenereerGlitchTekst(origineleTekst.Length);
                    glitchTimer = 0f;
                }
            }
            else
            {
                if (wasGlitchActief)
                {
                    doelwitTekst.text = origineleTekst;
                    wasGlitchActief = false;
                }
            }
        }

        //This part deals with the scroll manipulator virus
        if (doelwitScrollRect != null)
        {
            if (listManager.IsVirusTypeActief(FolderListManager.VirusType.ScrollManipulator))
            {
                scrollTimer += Time.deltaTime;
                if (scrollTimer >= schokInterval)
                {
                    //Chooses the positions for the fake scrolling
                    float randomScrollPositie = Random.value;

                    //Makes the list twitch, to simulate someone else controlling your pc
                    doelwitScrollRect.verticalNormalizedPosition = randomScrollPositie;

                    scrollTimer = 0f;
                }
            }
            else
            {
                scrollTimer = 0f;
            }
        }
    }

    //glitchy text string
    private string GenereerGlitchTekst(int lengte)
    {
        char[] resultaat = new char[lengte];
        for (int i = 0; i < lengte; i++)
        {
            int randomIndex = Random.Range(0, glitchTekens.Length);
            resultaat[i] = glitchTekens[randomIndex];
        }
        return new string(resultaat);
    }
}
