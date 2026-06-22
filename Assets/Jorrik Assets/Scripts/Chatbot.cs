using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions; // Cruciaal voor de link-fix
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ChatData { public string tekst; }

[System.Serializable]
public class ChatResponse { public string antwoord; }

public class Chatbot : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public Button sendButton;
    public ScrollRect chatScrollRect;
    public CharacterActionManager characterActionManager;

    private StringBuilder chatHistory = new StringBuilder();

    [Header("API Settings")]
    public string apiUrl = "https://eemsdeltaspellen-eemsdelta-assistant.hf.space/vraag";

    void Start()
    {
        if (sendButton != null) sendButton.onClick.AddListener(OnSendButtonClick);
    }

    public void OnSendButtonClick()
    {
        string userQuestion = inputField.text;
        if (!string.IsNullOrEmpty(userQuestion))
        {
            chatHistory.AppendLine("<b>Jij:</b> " + userQuestion);
            if (outputText != null)
            {
                outputText.text = chatHistory.ToString() + "\nAan het typen...";
                ScrollToBottom();
            }
            StartCoroutine(VerstuurVraag(userQuestion));
        }
    }

    IEnumerator VerstuurVraag(string vraagTekst)
    {
        inputField.text = "";
        ChatData data = new ChatData { tekst = vraagTekst };
        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
            string rawAntwoord = response.antwoord;

            // --- DE SLIMME REGEX FIX ---
            // (?i)Bron:\s*(.*) zoekt naar 'Bron:' en pakt de rest van de regel
            string verwerktAntwoord = Regex.Replace(rawAntwoord, @"(?i)Bron:\s*(.*)", delegate (Match m) {
                string bronTekst = m.Groups[1].Value.Trim();

                // Als de zin eindigt met een punt (bijv. "handboek.pdf."), haal die punt dan weg voor de link
                if (bronTekst.EndsWith("."))
                {
                    bronTekst = bronTekst.Substring(0, bronTekst.Length - 1);
                }

                // Maak de blauwe link met de opgeschoonde tekst
                return $"<color=#00aaff><u><link=\"{bronTekst}\">Bron: {bronTekst}</link></u></color>";
            });
            // ---------------------------

            chatHistory.AppendLine("<b>Jeems:</b> " + verwerktAntwoord);
            chatHistory.AppendLine();

            if (outputText != null)
            {
                outputText.text = chatHistory.ToString();
                ScrollToBottom();
            }

            if (characterActionManager != null) characterActionManager.TriggerRandomTalking();
        }
        else
        {
            chatHistory.AppendLine("<b>AI:</b> Fout bij verbinden...");
            if (outputText != null) outputText.text = chatHistory.ToString();
        }
    }

    void ScrollToBottom()
    {
        if (chatScrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}