using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace REST_API_HANDLER
{
    public class TopToolbarPanel : PanelBase
    {
        public static TopToolbarPanel instance;

        [SerializeField] private Text titleText;
        [SerializeField] Button backButton;

        private Action _action;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        public void setUp(string message, Action backAction)
        {
            titleText.text = message;
            this._action = backAction;
        }

        public void OnBackButtonPressed()
        {
            _action?.Invoke();
        }
    }

}

