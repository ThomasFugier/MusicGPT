using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace REST_API_HANDLER
{
    public class DialogPanel : MonoBehaviour
    {
        public static DialogPanel instance;

        [SerializeField] private PanelBase dialogPanel;
        [SerializeField] private Text messageText;
        private Action _dialogAction;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }


        public void ShowMessage(string message, Action action)
        {
            dialogPanel.Show();
            messageText.text = message;
            _dialogAction = action;
        }

        public void OnOkButtonPressed()
        {
            dialogPanel.Hide();
            _dialogAction.Invoke();
        }
    }
}


