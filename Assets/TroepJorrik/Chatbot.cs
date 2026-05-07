using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.InputSystem;

// Dit is nodig om de tekst van de vraag in JSON te zetten
[System.Serializable]
public class ChatData
{
    public string tekst;
}

// Dit vangt het antwoord op
[System.Serializable]
public class ChatResponse
{
    public string antwoord;

}

public class Chatbot : MonoBehaviour
{
    public string vraagstel;
    // PLAK HIER JE URL VAN HUGGING FACE (eindigend op /vraag)
    public string apiUrl = "https://jorrinkie-eemsdelta-assistant.hf.space/vraag";

    void Update()
    {
        // Test: druk op de Spatiebalk om een vraag te stellen
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StelVraag(vraagstel);
        }
    }

    public void StelVraag(string vraagTekst)
    {
        StartCoroutine(VerstuurVraag(vraagTekst));
    }

    IEnumerator VerstuurVraag(string vraagTekst)
    {
        ChatData data = new ChatData();
        data.tekst = vraagTekst;
        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Vraag verzonden aan AI...");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Zet het JSON antwoord om naar een leesbare string
            ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
            Debug.Log("AI Antwoord: " + response.antwoord);


        }
        else
        {
            Debug.LogError("Fout bij verbinden met AI: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
        }
    }
}