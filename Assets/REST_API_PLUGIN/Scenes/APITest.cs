using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APITest : MonoBehaviour
{

    string bUrl = "https://api.kucoin.com";
    string buySellEndPoint = "/api/v1/orders";
   // string dataQueryString = "?clientOid=CGH001&side=buy&symbol=BTC-USDT&price=19180&size=0.0001";
    
    string aKey = "63538c7fedd54c00010ce40c";
    string sKey = "ec9e1378-a224-48aa-a386-f118a70faea3";
    string passPhrase = "cryptorevenueexecutable";


    void Start()
    {
        StartCoroutine(callAPI());
    }


    public IEnumerator callAPI()
    {
       
        DateTime nowtime = DateTime.Now;
        long unixTime = ((DateTimeOffset)nowtime).ToUnixTimeMilliseconds();
        OrderPostModel orderModel = new OrderPostModel();
        string signInKey = CreateSignInKey(buySellEndPoint, unixTime.ToString(), "POST" , orderModel.ToBody(), sKey); 
        string encryptedPhrase = HmacSha256(passPhrase, sKey);

        string url = bUrl + buySellEndPoint;
        Debug.Log("Url--  " + url);

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("KC-API-SIGN", signInKey);
        headers.Add("KC-API-TIMESTAMP", unixTime.ToString());
        headers.Add("KC-API-KEY", aKey);
        headers.Add("KC-API-PASSPHRASE", encryptedPhrase);
        headers.Add("KC-API-KEY-VERSION", "2");
        headers.Add("Content-Type", "application/json");


        UnityWebRequest request = UnityWebRequest.Post(url, new WWWForm());
        byte[] bytes = Encoding.UTF8.GetBytes(orderModel.ToBody());
        UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
        request.uploadHandler = uH;
        
        // Add Form Values in WebRequest
        foreach (KeyValuePair<string, string> header in headers)
        {
            try
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            catch (Exception e)
            {
            }
        }

        yield return request.SendWebRequest();
        if (request.error != null)
        {
            Debug.Log("!ERROR! " + request.error + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("!Acccepted! " + request.downloadHandler.text);
        }
    }


  
    public string CreateSignInKey(string endPoint, string timeStamp, string methodName,string bodyJson,string secret)
    {
        string key = timeStamp + methodName + endPoint + bodyJson;
        string encryptedKey = HmacSha256(key, secret);
        return encryptedKey;
    }

    public string HmacSha256(string message, string secret)
    {
        var encoding = new ASCIIEncoding();
        var msgBytes = encoding.GetBytes(message);
        var secretBytes = encoding.GetBytes(secret);
        var hmac = new HMACSHA256(secretBytes);

        var hash = hmac.ComputeHash(msgBytes);

        return Convert.ToBase64String(hash);
    }


    [Serializable]
    public class OrderPostModel
    {
        public string clientOid, side, symbol, type, remark, stp, tradeType;

        // Create Class with some Defaut Data
        // Please see the documentation. I have attached a screenshot
        public OrderPostModel()
        {
            clientOid = "CGH001";
            side = "buy";
            symbol = "BTC-USDT";
            type = "limit";
            remark = "50";
            stp = "CN";
            tradeType = "TRADE";
        }

        public string ToBody()
        {
            var jsonString = JsonUtility.ToJson(this);
            Debug.Log("STR >> " + jsonString);
            return jsonString;
        }

    }
}
