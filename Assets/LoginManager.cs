using System.Collections;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Client;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Ascendant.Networking
{
    public class LoginManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject loginWindow;
        [SerializeField]
        private TMPro.TMP_Text nameInput;
        [SerializeField]
        private UnityEngine.UI.Button submitLoginButton;
        // Start is called before the first frame update
        void Start()
        {
            Ascendant.Networking.ConnectionManager.Instance.OnConnected += StartLoginProcess;
            submitLoginButton.onClick.AddListener(OnSubmitLogin);
            loginWindow.SetActive(false);
            ConnectionManager.Instance.Client.MessageReceived += OnMessage;
        }

        public void OnSubmitLogin()
        {
            ConnectionManager.Instance.Client.MessageReceived += OnMessage;
            if (!String.IsNullOrEmpty(nameInput.text))
            {
                loginWindow.SetActive(false);

                using (Message message = Message.Create((ushort)Tags.LoginRequest, new LoginRequestData(nameInput.text)))
                {
                    ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                switch ((Tags)message.Tag)
                {
                    case Tags.LoginRequestRejected:
                        OnLoginRequestRejected();
                        break;
                    case Tags.LoginRequestAccepted:
                        OnLoginRequestAccepted(message.Deserialize<LoginInfoData>());
                        break;
                }
            }
        }

        private void OnLoginRequestAccepted(LoginInfoData data)
        {
            ConnectionManager.Instance.clientId = data.clientId;
            SceneManager.LoadScene("TPS");
            throw new NotImplementedException();
        }

        private void OnLoginRequestRejected()
        {
            loginWindow.SetActive(true);
        }

        public void StartLoginProcess()
        {
            loginWindow.SetActive(true);
        }

        void OnDestroy()
        {
            ConnectionManager.Instance.OnConnected -= StartLoginProcess;
            ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
        }
    }
}


