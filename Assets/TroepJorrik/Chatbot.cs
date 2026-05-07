using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro; // Required for TextMeshPro
using UnityEngine.UI; // Required for the Button

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

    // This method is called when the button is clicked
    public void OnSendButtonClick()
    {
        string userQuestion = inputField.text;

        if (!string.IsNullOrEmpty(userQuestion))
        {
            StartCoroutine(VerstuurVraag(userQuestion));
        }
        else
        {
            outputText.text = "Typ eerst een vraag...";
        }
    }

    IEnumerator VerstuurVraag(string vraagTekst)
    {
        // Visual feedback for the user
        outputText.text = "Aan het typen...";
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
            
            // Show the answer in the TMP Text box
            outputText.text = response.antwoord;
            Debug.Log("AI Antwoord: " + response.antwoord);
        }
        else
        {
            outputText.text = "Fout: " + request.error;
            Debug.LogError("Details: " + request.downloadHandler.text);
        }
    }
}