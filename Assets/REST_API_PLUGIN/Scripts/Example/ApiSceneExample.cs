using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace REST_API_HANDLER
{
	public class ApiSceneExample : MonoBehaviour
	{

		public static ApiSceneExample instance;

		[Header("Employee List")]
		[SerializeField] private GameObject loadingPanel;
		[SerializeField] private GameObject itemPrefab;
		[SerializeField] private GameObject listItemPrefab;

		[Header("Input")]
		[SerializeField] private PanelBase titleInputPanel;
		[SerializeField] private PanelBase bodyInputPanel;
		[SerializeField] private PanelBase userIDInputPanel;


		[SerializeField] private InputField titleInput;
		[SerializeField] private InputField bodyInput;
		[SerializeField] private InputField userIDInput;

		[SerializeField] private Button submitButton;

		[Header("UI Panels")]
		[SerializeField] private PanelBase inputPanel;
		[SerializeField] private PanelBase listPanel;

		private List<PanelBase> _panelList = new List<PanelBase>();


		public enum REQUEST_TYPE { GET, POST, PUT, DELETE }
		private Post _data;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
		}

		void Start()
		{
			CreateList();
			loadingPanel.SetActive(false);
			GetPosts();
		}

		private void CreateList()
		{
			_panelList.Add(titleInputPanel);
			_panelList.Add(bodyInputPanel);
			_panelList.Add(userIDInputPanel);
		}

		public void GetPosts()
		{
			TopToolbarPanel.instance.setUp("Post Lists (Get)", () =>
			{
				// This is the first screen so no back action will apply here..
			});

			loadingPanel.SetActive(true);
			listPanel.Show();
			inputPanel.Hide();

			ApiCall.instance.GetRequest<GetPostsResponseModel>(ApiConfig.instance.API_GET_EMPLOYEES, null, (result => {
				//Debug.Log("field1 url is  " + result.data.Count);
				for (int i = 0; i < result.data.Count; i++)
				{
					InstantiateItem(result.data[i]);
				}
				loadingPanel.SetActive(false);
			}),
			(error => {
				loadingPanel.SetActive(false);
				Debug.Log("eror is -- " + error);
			}), "data");


		}

		public void OnAddButtonPressed()
		{
			OpenInput(REQUEST_TYPE.POST, null);
		}

		public void OpenInput(REQUEST_TYPE reqType, Post data)
		{
			//_currentReqType = reqType;
			_data = data;
			//HideAllInputField();
			ShowAlInputField();
			SetUI();
			inputPanel.Show();
			listPanel.Hide();
			switch (reqType)
			{
				case REQUEST_TYPE.GET:
					GetPosts();
					break;
				case REQUEST_TYPE.POST:

					TopToolbarPanel.instance.setUp("Add Post (POST)", () =>
					{
						// When Back button Pressed then show the Post List.. 
						GetPosts();
					});
					submitButton.onClick.AddListener(AddPost);
					break;
				case REQUEST_TYPE.PUT:

					TopToolbarPanel.instance.setUp("Update Post (Put)", () =>
					{
						// When Back button Pressed then show the Post List.. 
						GetPosts();
					});
					submitButton.onClick.AddListener(PutPost);
					break;
				case REQUEST_TYPE.DELETE:
					inputPanel.Hide();
					DeletePost();
					//submitButton.onClick.AddListener(DeletePost);
					break;

			}
		}

		private void AddPost()
		{
			loadingPanel.SetActive(true);


			string title = titleInput.text;
			string body = bodyInput.text;
			AddPostRequestModel reqModel = new AddPostRequestModel(title, body, getUserIdInput());

			ApiCall.instance.PostRequest<AddPostResponseModel>(ApiConfig.instance.API_CREATE_POST, reqModel.ToCustomHeader(), null, reqModel.ToBody(), (result =>
			{
				loadingPanel.SetActive(false);
				Debug.Log("Create Post Success  " + result.data.id + "  " + result.data.title);
				DialogPanel.instance.ShowMessage("Add Success", () => {
					GetPosts();
				});

			}), (error =>
			{
				loadingPanel.SetActive(false);
				Debug.Log("Create Post Error - " + error);
			}), "data");

		}

		private void PutPost()
		{
			_data.title = titleInput.text;
			_data.body = bodyInput.text;
			_data.userId = getUserIdInput();

			PutPostRequestModel reqModel = new PutPostRequestModel(_data);

			ApiCall.instance.PutRequest<AddPostResponseModel>(reqModel.ToUrl(), reqModel.ToCustomHeader(), null, reqModel.ToBody(), (result =>
			{
				Debug.Log("Create Post Success  " + result.data.id + "  " + result.data.title);

				DialogPanel.instance.ShowMessage("Update Success", () => {
					GetPosts();
				});

			}), (error =>
			{
				Debug.Log("Create Post Error - " + error);
			}), "data");

		}


		private void DeletePost()
		{

			DeletePostRequestModel reqModel = new DeletePostRequestModel(_data);

			Debug.Log("Del Url " + reqModel.ToUrl());
			ApiCall.instance.DeleteRequest<string>(reqModel.ToUrl(), reqModel.ToCustomHeader(), (result =>
			{
				//Debug.Log("Create Post Success  " + result.data.id + "  " + result.data.title);

				DialogPanel.instance.ShowMessage("Delete Success", () => {
					GetPosts();
				});

			}), (error =>
			{
				if (error == ApiCall.CAN_NOT_DECODE_JSON)
				{

				}

				Debug.Log("Create Post Error - " + error);
			}), "data");

		}


		private void SetUI()
		{
			if (_data != null)
			{
				titleInput.text = _data.title;
				bodyInput.text = _data.body;
				userIDInput.text = _data.userId + "";
			}
		}

		private void InstantiateItem(Post data)
		{
			GameObject field_1_item = Instantiate(itemPrefab, listItemPrefab.transform);
			field_1_item.transform.parent = listItemPrefab.transform;
			PostItem script = field_1_item.GetComponent<PostItem>();
			script.Setup(data);

		}

		private void DeleteAllItems()
		{
			PostItem[] items = FindObjectsOfType<PostItem>();
			if (items != null && items.Length > 0)
			{
				for (int i = 0; i < items.Length; i++)
				{
					Destroy(items[i].gameObject);
				}
			}
		}

		private void HideAllInputField()
		{
			for (int i = 0; i < _panelList.Count; i++)
			{
				_panelList[i].Hide();
			}

			listPanel.Hide();
			inputPanel.Hide();

			titleInput.text = "";
			bodyInput.text = "";
			userIDInput.text = "";

		}
		private void ShowAlInputField()
		{
			for (int i = 0; i < _panelList.Count; i++)
			{
				_panelList[i].Show();
			}
		}

		private int getUserIdInput()
		{
			string userId = userIDInput.text;
			try
			{
				int id = int.Parse(userId);
				return id;
			}
			catch
			{
				return 0;
			}
		}
	}

}

