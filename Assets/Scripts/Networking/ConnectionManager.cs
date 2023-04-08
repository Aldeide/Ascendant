using System;
using System.Net;
using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Ascendant.Networking
{
    // A singleton that manages the connection to the server.
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;
        public UnityClient Client { get; private set; }

        public uint clientId { get; set; }

        public delegate void OnConnectedDelegate();
        public event OnConnectedDelegate OnConnected;

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
            Client.ConnectInBackground(IPAddress.Loopback, Client.Port, false, ConnectCallback);
            Client.MessageReceived += OnMessage;
        }

        public bool IsConnected() => Client.ConnectionState == ConnectionState.Connected;

        internal void SpawnPlayerOnServerRequest()
        {
            Debug.Log("Sending Spawn Request");
            using (Message message = Message.CreateEmpty((ushort)Tags.SpawnLocalPlayerRequest))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
        }

        private void ConnectCallback(Exception e)
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                Debug.Log("Connected to server!");
                //OnConnected?.Invoke();
                OnConnectedToServer();
            }
            else
            {
                Debug.LogError($"Unable to connect to server. Reason: {e.Message} ");
            }
        }

        private void OnConnectedToServer()
        {
            using (Message message = Message.Create((ushort)Tags.LoginRequest, new LoginRequestData("Aldeide")))
            {
                ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
            }
            
            /*
            using (Message message = Message.CreateEmpty((ushort)Tags.JoinGameRequest))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
            */
        }


        private void OnDestroy()
        {
            Client.MessageReceived -= OnMessage;
        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message m = e.GetMessage())
            {
                switch ((Tags)m.Tag)
                {
                    case Tags.JoinGameResponse:
                        OnJoinGameResponse(m.Deserialize<JoinGameResponseData>());
                        break;
                    case Tags.SpawnLocalPlayerResponse:
                        OnSpawnLocalPlayerResponse(m.Deserialize<SpawnLocalPlayerResponseData>());
                        break;
                    case Tags.SyncPlayerStateResponse:
                        OnSyncPlayerResponse(m.Deserialize<PlayerStateData>());
                        break;
                    case Tags.LoginRequestRejected:
                        break;
                    case Tags.LoginRequestAccepted:
                        SpawnPlayerOnServerRequest();
                        break;
                    case Tags.SpawnPlayerNotification:
                        OnSpawnPlayer(m.Deserialize<PlayerClientId>());
                        break;
                    case Tags.SyncOtherPlayer:
                        OnSyncOtherPlayer(m.Deserialize<PlayerStateData>());
                        break;
                    case Tags.PlayerDisconnectedNotification:
                        OnPlayerDisconnectedNotification(m.Deserialize<PlayerClientId>());
                        break;
                    case Tags.SyncPlayerStatsNotification:
                        OnSyncStatsNotification(m.Deserialize<PlayerStatsData>());
                        break;
                }
            }
        }

        private void OnSyncStatsNotification(PlayerStatsData data)
        {
            GameManager.Instance.SyncOtherPlayerStats(data);
        }

        // Processes a notification that another player has disconnected. Removes the player
        // from the game manager and removes its GameObject.
        private void OnPlayerDisconnectedNotification(PlayerClientId playerClientId)
        {
            GameManager.Instance.RemovePlayer(playerClientId.id);
        }

        private void OnSyncOtherPlayer(PlayerStateData playerStateData)
        {
            GameManager.Instance.SyncOtherPlayer(playerStateData);
        }

        private void OnSpawnPlayer(PlayerClientId spawnPlayerData)
        {
            GameManager.Instance.SpawnOtherPlayer(spawnPlayerData.id);
        }

        private void OnSyncPlayerResponse(PlayerStateData data)
        {
            Debug.Log(data.position);
        }

        private void OnSpawnLocalPlayerResponse(SpawnLocalPlayerResponseData data)
        {
            Debug.Log("Spawing local player");
            GameManager.Instance.SpawnLocalPlayer(data);
        }

        private void OnJoinGameResponse(JoinGameResponseData data)
        {
            if (!data.JoinGameRequestAccepted)
            {
                Debug.Log("houston we have a problem");
                return;
            }
            SceneManager.LoadScene("TPS");
        }



    }
}