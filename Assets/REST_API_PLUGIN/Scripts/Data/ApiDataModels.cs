using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace REST_API_HANDLER
{
    //Source - https://json2csharp.com/
    public class ApiDataModels
    {
    }

    [Serializable]
    public class GetPostsResponseModel
    {
        public List<Post> data;
    }

    [Serializable]
    public class Post
    {
        public int userId;
        public int id;
        public string title;
        public string body;
    }

    //----------- Add Post Start -----------------

    #region AddPost

    [Serializable]
    public class AddPostRequestModel
    {
        public int userId;
        public string title;
        public string body;

        public AddPostRequestModel(string _title, string _body, int _userId)
        {
            title = _title;
            body = _body;
            userId = _userId;
        }

        public string ToBody()
        {
            var jsonString = JsonUtility.ToJson(this);
            Debug.Log("STR >> " + jsonString);
            return jsonString;
        }

        public WWWForm ToFormData()
        {
            WWWForm formData = new WWWForm();
            formData.AddField("title", title);
            return formData;
        }

        public Dictionary<string, string> ToCustomHeader()
        {
            return ApiConfig.GetHeaders();
        }
    }

    [Serializable]
    public class AddPostResponseModel
    {
        public Post data;
    }

    #endregion AddPost

    //----------- Add Post End -----------------


    //----------- Put Post Start ----------------

    #region PutPost

    [Serializable]
    public class PutPostRequestModel
    {
        public Post postData;


        public PutPostRequestModel(Post _postData)
        {
            postData = _postData;
        }

        public string ToBody()
        {
            var jsonString = JsonUtility.ToJson(postData);
            Debug.Log("STR >> " + jsonString);
            return jsonString;
        }

        public string ToUrl()
        {
            return ApiConfig.instance.API_CREATE_POST + "/1"; //+ postData.id;
        }

        public Dictionary<string, string> ToCustomHeader()
        {
            return ApiConfig.GetHeaders();
        }
    }

    #endregion PutPost

    //-----------Put Post End -----------------


    //----------- Delete Post Start ----------------

    #region DeletePost

    [Serializable]
    public class DeletePostRequestModel
    {
        public Post postData;


        public DeletePostRequestModel(Post _postData)
        {
            postData = _postData;
        }

        public string ToUrl()
        {
            return ApiConfig.instance.API_DELETE_POST + "/" + postData.id; //+ postData.id;
        }

        public Dictionary<string, string> ToCustomHeader()
        {
            return ApiConfig.GetHeaders();
        }
    }

    #endregion DeletePost

    //-----------Delete Post End -----------------
}
