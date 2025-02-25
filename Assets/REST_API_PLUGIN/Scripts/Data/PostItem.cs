using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace REST_API_HANDLER
{
    public class PostItem : MonoBehaviour
    {

        public Text titleText;

        private Post _data;

        public void Setup(Post data)
        {
            _data = data;

            titleText.text = _data.title;
        }

        public void OnPutButtonClicked()
        {
            ApiSceneExample.instance.OpenInput(ApiSceneExample.REQUEST_TYPE.PUT, _data);
        }

        public void OnPatchButtonClicked()
        {
            ApiSceneExample.instance.OpenInput(ApiSceneExample.REQUEST_TYPE.PUT, _data);
        }

        public void OnDeleteButtonClicked()
        {
            ApiSceneExample.instance.OpenInput(ApiSceneExample.REQUEST_TYPE.DELETE, _data);
        }
    }

}
