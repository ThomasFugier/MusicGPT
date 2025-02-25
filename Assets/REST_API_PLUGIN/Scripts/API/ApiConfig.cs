using System.Collections.Generic;
using UnityEngine;

namespace REST_API_HANDLER
{

	public class ApiConfig : MonoBehaviour
	{
		public static ApiConfig instance;


		//Dummy Url site - https://dummy.restapiexample.com/
		private string API_BASE_URL = "http://dummy.restapiexample.com/api/v1/";


		[HideInInspector]
		public string API_GET_EMPLOYEES;
		[HideInInspector]
		public string API_CREATE_POST;
		[HideInInspector]
		public string API_DELETE_POST;
		[HideInInspector]
		public string API_UPLOAD_VIDEO;
		[HideInInspector]
		public string API_UPDATE_VIDEO;

		void Awake()
		{
			if (instance == null)
				instance = this;

			API_GET_EMPLOYEES = "https://jsonplaceholder.typicode.com/posts";//API_BASE_URL + "employees";
			API_CREATE_POST = "https://jsonplaceholder.typicode.com/posts";
			API_DELETE_POST = "https://jsonplaceholder.typicode.com/posts";
		}


		public static Dictionary<string, string> GetHeaders()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Content-type", "application/json; charset=UTF-8");

			return headers;
		}

	}

}


