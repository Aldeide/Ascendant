using System;
using System.Net;
using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ascendant
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;
        public UnityClient Client { get; private set; }
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }
        void Start()
        {
            Client = GetComponent<UnityClient>();
            //Client.ConnectInBackground(IPAddress.Loopback, Client.Port, false, ConnectCallback);
            Client.MessageReceived += OnMessage;
        }

        internal void SpawnPlayerOnServerRequest()
        {
            using (Message message = Message.CreateEmpty((ushort)Tags.SpawnLocalPlayerRequest))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
        }

        public void InitiateConnection()
        {
            Client.ConnectInBackground(IPAddress.Loopback, Client.Port, false, ConnectCallback);
        }

        private void ConnectCallback(Exception e)
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                Debug.Log("Connected to server!");
                OnConnectedToServer();
            }
            else
            {
                Debug.LogError($"Unable to connect to server. Reason: {e.Message} ");
            }
        }

        private void OnConnectedToServer()
        {
            using (Message message = Message.CreateEmpty((ushort)Tags.JoinGameRequest))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
        }
        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message m = e.GetMessage())
            {
                switch ((Tags)m.Tag)
                {
                    case Tags.JoinGameResponse:
                        OnJoinGameResponse(m.Deserialize<JoinGameResponseData>()); ;
                        break;
                    case Tags.SpawnLocalPlayerResponse:
                        OnSpawnLocalPlayerResponse(m.Deserialize<SpawnLocalPlayerResponseData>());
                        break;
                }
            }
        }

        private void OnSpawnLocalPlayerResponse(SpawnLocalPlayerResponseData data)
        {
            GameManager.Instance.SpawnLocalPlayer(data);
        }

        private void OnJoinGameResponse(JoinGameResponseData data)
        {
            if (!data.JoinGameRequestAccepted)
            {
                Debug.Log("houston we have a problem");
                return;
            }

            SceneManager.LoadScene("TPS"); // Make sure you add Using UnityEngine.SceneManagement
        }

        private void OnDestroy()
        {
            Client.MessageReceived -= OnMessage;
        }
    }
}