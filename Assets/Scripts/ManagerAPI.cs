using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Newtonsoft.Json;

public class ManagerAPI : MonoBehaviour
{
    public string apiKey = "Ytf6eSQOsfrMccW54pfRn6KbfqXX85jSD"; // Remplace par ta cl� API Mistral
    private string apiUrl = "https://api.mistral.ai/v1/chat/completions"; // URL de l'API
    [TextArea(20, 20)]
    public string basePrompt;
    public string prompt;

    public MusicPlayer musicPlayer;
    public string receivedMessage;
    [TextArea(20, 20)]
    public string sentMessage;

    public TMPro.TextMeshProUGUI inputPrompt;
    public UnityEngine.UI.Button askButton;

    public LoadingButton loadingButton;

    public InstrumentSelection instrumentSelection;

    public void Ask()
    {
        string s = inputPrompt.text;
        inputPrompt.text = "";

        prompt = "(" + s + ")";

        StartCoroutine(SendChatRequest());
    }

    void Start()
    {

    }

    IEnumerator SendChatRequest()
    {
        // Construire le contenu du message avec basePrompt et prompt
        string fullPrompt = basePrompt + " " + prompt ;

        string selectedInstruments = "";

        for(int i = 0; i < instrumentSelection.selectedInstruments.Count; i++)
        {
            selectedInstruments += instrumentSelection.selectedInstruments[i].instrument.name;

            if(i < instrumentSelection.selectedInstruments.Count - 1)
            {
                selectedInstruments += " ET ";
            }
           
        }

        selectedInstruments += " donc il faut " + instrumentSelection.selectedInstruments.Count.ToString() + " tracks seulement";

        fullPrompt += "Aussi, voici quelques informations importantes concernant la cr�ativit� musicale : Les instruments s�l�ctionn� sont : " + selectedInstruments;

        // Construire les donn�es de la requ�te JSON
        string jsonData = "{\"model\":\"mistral-large-latest\",\"messages\":[{\"role\":\"user\",\"content\":\"" + fullPrompt + "\"}]}";

        sentMessage = jsonData;
        
        // Cr�er la requ�te
        UnityWebRequest request = UnityWebRequest.Put(apiUrl, jsonData);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        // Envoyer la requ�te et attendre la r�ponse
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }

        else
        {
            Debug.LogError("Request failed: " + request.error);
        }

        string jsonResponse = "{\"id\":\"07db81c8f4f841e9a45efd187ec88522\",\"object\":\"chat.completion\",...}";
        string messageContent = ApiUtils.DecodeResponse(request.downloadHandler.text);

        receivedMessage = messageContent.Substring(8);
        receivedMessage = receivedMessage.Substring(0, receivedMessage.Length - 4);
        musicPlayer.PlayPartition(receivedMessage);

        loadingButton.TurnButtonBack();
    }

    [Serializable]
    public class MistralChatResponseModel
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class Message
    {
        public string content;
    }

    public class ApiUtils
    {
        public static string DecodeResponse(string jsonResponse)
        {
            try
            {
                jsonResponse = jsonResponse.Trim();
                MistralChatResponseModel responseModel = JsonUtility.FromJson<MistralChatResponseModel>(jsonResponse);

                if (responseModel?.choices != null && responseModel.choices.Length > 0)
                {
                    return responseModel.choices[0].message.content;
                }
                return "Erreur: r�ponse invalide";
            }
            catch (Exception e)
            {
                Debug.LogError("Erreur de parsing JSON: " + e.Message);
                return "Erreur de parsing JSON";
            }
        }
    }
}
