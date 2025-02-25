using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Newtonsoft.Json;

public class ManagerAPI : MonoBehaviour
{
    public string apiKey = "Ytf6eSQOsfrMccW54pfRn6KbfqXX85jSD"; // Remplace par ta clé API Mistral
    private string apiUrl = "https://api.mistral.ai/v1/chat/completions"; // URL de l'API
    [TextArea(50, 50)]
    public string basePrompt;
    public string prompt;

    void Start()
    {
        StartCoroutine(SendChatRequest());
    }

    IEnumerator SendChatRequest()
    {
        // Construire le contenu du message avec basePrompt et prompt
        string fullPrompt = basePrompt + " " + prompt;

        // Construire les données de la requête JSON
        string jsonData = "{\"model\":\"mistral-large-latest\",\"messages\":[{\"role\":\"user\",\"content\":\"" + fullPrompt + "\"}]}";

        // Créer la requête
        UnityWebRequest request = UnityWebRequest.Put(apiUrl, jsonData);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        // Envoyer la requête et attendre la réponse
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
        }

        string jsonResponse = "{\"id\":\"07db81c8f4f841e9a45efd187ec88522\",\"object\":\"chat.completion\",...}"; // Ta réponse complète JSON
        string messageContent = ApiUtils.DecodeResponse(request.downloadHandler.text);

        Debug.Log(messageContent);  
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
                return "Erreur: réponse invalide";
            }
            catch (Exception e)
            {
                Debug.LogError("Erreur de parsing JSON: " + e.Message);
                return "Erreur de parsing JSON";
            }
        }
    }
}
