using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace REST_API_HANDLER
{
	public class HttpClient : MonoBehaviour
	{
		public static HttpClient instance;
		private readonly int TIMEOUT = 50000;




		private void Awake()
		{
			if (instance == null)
				instance = this;
		}

		//public IEnumerator Get(string url, string jsonPrefix, Dictionary<string, string> headers, Action<HttpResult> action)
		//{
		//	if (Application.internetReachability == NetworkReachability.NotReachable)
		//	{
		//		action.Invoke(new HttpResult("No internet"));
		//		yield break;
		//	}

		//	UnityWebRequest www = UnityWebRequest.Get(url);

		//       if(headers != null)
		//       {
		//		foreach (KeyValuePair<string, string> header in headers)
		//		{
		//			www.SetRequestHeader(header.Key, header.Value);
		//		}
		//	}

		//	yield return Send(www);
		//	action.Invoke(BuildResult(www, jsonPrefix));
		//}

		//public IEnumerator PostWithFormData(string url, string jsonPrefix, string jsonPerameter, Dictionary<string, string> headers, WWWForm formData, Action<HttpResult> action)
		//{
		//	if (Application.internetReachability == NetworkReachability.NotReachable)
		//	{
		//		action.Invoke(new HttpResult("No Internet"));
		//		yield break;
		//	}
		//	UnityWebRequest request = UnityWebRequest.Post(url, formData);

		//	if (jsonPerameter != null && jsonPerameter != "")
		//	{
		//		byte[] bytes = Encoding.UTF8.GetBytes(jsonPerameter);

		//		UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
		//		//uH.contentType = "application/json"; //this is ignored?
		//		request.uploadHandler = uH;

		//	}

		//	foreach (KeyValuePair<string, string> header in headers)
		//	{
		//		try
		//		{
		//			request.SetRequestHeader(header.Key, header.Value);
		//		}
		//		catch (Exception e)
		//		{
		//			//Debug.Log("UNITY>> Post req error - " + e.Message);
		//		}
		//	}
		//	yield return Send(request);
		//	action.Invoke(BuildResult(request,jsonPrefix));
		//}


		public async void GetAsync(string url, string jsonPrefix, Dictionary<string, string> headers, Action<HttpResult> action)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				action.Invoke(new HttpResult("No internet"));
				return;
			}

			UnityWebRequest request = UnityWebRequest.Get(url);

			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					request.SetRequestHeader(header.Key, header.Value);
				}
			}

			await Send(request);
			action.Invoke(BuildResult(request, jsonPrefix));
			request.Dispose();
			//var operation = request.SendWebRequest();
			//while (!operation.isDone) await Task.Yield();
			//action.Invoke(BuildResult(request, jsonPrefix));
		}

		public async void PostAsync(string url, string jsonPrefix, string jsonPerameter, Dictionary<string, string> headers, WWWForm formData, Action<HttpResult> action)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				action.Invoke(new HttpResult("No Internet"));
				return;
			}
			UnityWebRequest request = UnityWebRequest.Post(url, formData);

			if (jsonPerameter != null && jsonPerameter != "")
			{
				byte[] bytes = Encoding.UTF8.GetBytes(jsonPerameter);

				UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
				//uH.contentType = "application/json"; //this is ignored?
				request.uploadHandler = uH;

			}

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

			await Send(request);
			action.Invoke(BuildResult(request, jsonPrefix));
			request.Dispose();
		}
		


		public async void PutAsync(string url, string jsonPrefix, string bodyJson, Dictionary<string, string> headers, WWWForm formData, Action<HttpResult> action)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				action.Invoke(new HttpResult("No Internet"));
				return;
			}

			UnityWebRequest request = UnityWebRequest.Put(url, bodyJson);

			if (headers.Count > 0)
				foreach (var header in headers)
					try
					{
						request.SetRequestHeader(header.Key, header.Value);
					}
					catch (Exception)
					{
					}


			await Send(request);
			action.Invoke(BuildResult(request, jsonPrefix));
			request.Dispose();
		}


		public async void DeleteAsync(string url, string jsonPrefix, Dictionary<string, string> headers, Action<HttpResult> action)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				action.Invoke(new HttpResult("No Internet"));
				return;
			}

			UnityWebRequest request = UnityWebRequest.Delete(url);

			if (headers.Count > 0)
				foreach (var header in headers)
					try
					{
						request.SetRequestHeader(header.Key, header.Value);
					}
					catch (Exception)
					{
					}


			await Send(request);
			action.Invoke(BuildResult(request, jsonPrefix));
			request.Dispose();
		}



		private async Task Send(UnityWebRequest request)
		{
			float addTime = 0f;

			var op = request.SendWebRequest();
			while (true)
			{
				if (!op.isDone)
				{
					await Task.Yield();
				}
				addTime += Time.deltaTime;
				if ((int)addTime >= TIMEOUT)
				{
					request.Abort();
					break;
				}
			}
		}

		//private IEnumerator Send(UnityWebRequest www)
		//{
		//	float addTime = 0f;
		//	UnityWebRequestAsyncOperation op = www.SendWebRequest();

		//	while (true)
		//	{
		//		if (!op.isDone)
		//		{
		//			yield return null;
		//		}
		//		addTime += Time.deltaTime;
		//		if ((int)addTime >= TIMEOUT)
		//		{
		//			www.Abort();
		//			break;
		//		}
		//	}
		//}

		private HttpResult BuildResult(UnityWebRequest www, string jsonPrefix = null)
		{
			string resText = "";
			if (www.downloadHandler != null && www.downloadHandler.text != null)
			{
				if (jsonPrefix == null)
				{
					resText = www.downloadHandler.text;

				}
				else
				{
					resText = "{\"" + jsonPrefix + "\":" + www.downloadHandler.text + "}";
				}
			}



			Debug.Log("UNITY>> Res tExt " + resText);
			long resCode = www.responseCode;
			string url = www.url;
			Dictionary<string, string> headers = www.GetResponseHeaders();
			//Debug.Log("Res Code " + resCode);
			if (200 <= resCode && resCode <= 299) // 200 is success
			{
				//Debug.Log("In res");
				return new HttpResult(headers, url, resText);
			}

			return new HttpResult("UNITY>> Can not get result from Api");
		}


		public interface Delegate
		{
			void OnNetworkCallComplete(bool isSuccess, string responseJson);
		}

	}

}

