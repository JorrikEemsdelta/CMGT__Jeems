using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine.UI; 

[System.Serializable]
public class ChatData
{
    public string tekst;
}

[System.Serializable]
public class ChatResponse
{
    public string antwoord;
}

public class Chatbot : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;  // Drag your Input Field here
    public TMP_Text outputText;        // Drag your Response Text here
    public Button sendButton;          // Drag your Button here
    // Optional: assign the ScrollRect that contains the chat output so we can auto-scroll
    public ScrollRect chatScrollRect;

    // Keep the chat history as a running log
    private StringBuilder chatHistory = new StringBuilder();

    [Header("API Settings")]
    public string apiUrl = "https://jorrinkie-eemsdelta-assistant.hf.space/vraag";

    void Start()
    {
        // Automatically add the listener so you don't have to do it in the Inspector
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClick);
        }
    }

    // Scroll the ScrollRect to the bottom to show the latest messages
    void ScrollToBottom()
    {
        if (chatScrollRect == null) return;

        // Ensure layout elements have updated first
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;

        // As a fallback, move to bottom on the next frame as well
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        if (chatScrollRect == null) yield break;
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    // This method is called when the button is clicked
    public void OnSendButtonClick()
    {
        string userQuestion = inputField.text;

        if (!string.IsNullOrEmpty(userQuestion))
        {
            // Append user's question to the chat history and show it immediately
            chatHistory.AppendLine("<b>Jij:</b> " + userQuestion);
            // Show history and a typing indicator
            if (outputText != null)
            {
                outputText.text = chatHistory.ToString() + "\nAan het typen...";
                ScrollToBottom();
            }

            StartCoroutine(VerstuurVraag(userQuestion));
        }
        else
        {
            outputText.text = "Typ eerst een vraag...";
        }
    }

    IEnumerator VerstuurVraag(string vraagTekst)
    {
        // Visual feedback for the user: show the full history and a typing indicator
        if (outputText != null)
        {
            outputText.text = chatHistory.ToString() + "\nAan het typen...";
            ScrollToBottom();
        }
        inputField.text = ""; // Clear input after sending

        ChatData data = new ChatData { tekst = vraagTekst };
        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
            
            // Append AI answer to history and display full chat
            chatHistory.AppendLine("<b>Jeems:</b> " + response.antwoord);
            chatHistory.AppendLine();
            if (outputText != null)
            {
                outputText.text = chatHistory.ToString();
                ScrollToBottom();
            }
            Debug.Log("AI Antwoord: " + response.antwoord);
        }
        else
        {
            string errorMsg = "Fout: " + request.error;
            chatHistory.AppendLine("<b>AI:</b> " + errorMsg);
            chatHistory.AppendLine();
            if (outputText != null)
            {
                outputText.text = chatHistory.ToString();
                ScrollToBottom();
            }
            Debug.LogError("Details: " + request.downloadHandler.text);
        }
    }
}