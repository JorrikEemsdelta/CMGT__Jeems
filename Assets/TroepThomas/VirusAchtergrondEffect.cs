using UnityEngine;
using UnityEngine.UI; // NODIG VOOR DE SCROLLRECT
using TMPro;

public class VirusAchtergrondEffect : MonoBehaviour
{
    public FolderListManager listManager;

    [Header("Draai Instellingen")]
    public float draaiSnelheid = 50f;

    [Header("Tekst Glitch Instellingen")]
    public TextMeshProUGUI doelwitTekst;
    public float glitchInterval = 0.1f;

    [Header("Scroll Manipulator Instellingen")]
    public ScrollRect doelwitScrollRect; // Sleep hier je ScrollView / ScrollRect naartoe
    public float schokInterval = 2f;     // Om de hoeveel seconden hij schokt

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

        // ==========================================
        // EFFECT 1: DRAAIEN VAN DE ACHTERGROND
        // ==========================================
        if (listManager.IsVirusTypeActief(FolderListManager.VirusType.DraaiAchtergrond))
        {
            transform.Rotate(0, 0, draaiSnelheid * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        // ==========================================
        // EFFECT 2: TEKST GLITCH
        // ==========================================
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

        // ==========================================
        // EFFECT 3: SCROLL MANIPULATOR (NIEUW!)
        // ==========================================
        if (doelwitScrollRect != null)
        {
            if (listManager.IsVirusTypeActief(FolderListManager.VirusType.ScrollManipulator))
            {
                scrollTimer += Time.deltaTime;
                if (scrollTimer >= schokInterval)
                {
                    // Kies een compleet willekeurige scroll-positie tussen 0 (onderaan) en 1 (bovenaan)
                    float randomScrollPositie = Random.value;

                    // Geef de schok aan de lijst
                    doelwitScrollRect.verticalNormalizedPosition = randomScrollPositie;

                    // Reset de timer
                    scrollTimer = 0f;
                }
            }
            else
            {
                // Als het virus niet actief is, resetten we de timer netjes naar 0
                scrollTimer = 0f;
            }
        }
    }

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
