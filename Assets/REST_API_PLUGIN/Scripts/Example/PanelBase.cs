using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace REST_API_HANDLER
{
    public class PanelBase : MonoBehaviour
    {
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
    }
}


