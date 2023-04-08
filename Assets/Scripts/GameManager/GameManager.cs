using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using Ascendant.Networking;
using System;

namespace Ascendant
{
    public class GameManager : MonoBehaviour
    {
        public GameObject playerPrefab;
        public Dictionary<ushort, GameObject> connectedPlayers = new Dictionary<ushort, GameObject>();
        public ushort localPlayerId;
        public static GameManager Instance;
        public GameObject localPlayer;
        public GameObject networkedPlayerPrefab;

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
        // Start is called before the first frame update
        void Start()
        {
            //localPlayer = GameObject.Find("NetworkedPlayer");
            
            localPlayerId = ConnectionManager.Instance.Client.ID;
            Debug.Log("Local Player ID set to: " + localPlayerId);
            ConnectionManager.Instance.SpawnPlayerOnServerRequest();
            
        }


        void FixedUpdate()
        {
            if (localPlayer == null)
            {
                localPlayer = GameObject.Find("NetworkedPlayer");
                return;
            }
            using (Message message = Message.Create((ushort)Tags.SyncPlayerStateRequest, localPlayer.GetComponent<Controllers.PlayerStateController>().ToPlayerStateData()))
                {
                    ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
                }
            using (Message message = Message.Create((ushort)Tags.SyncPlayerStatsRequest, localPlayer.GetComponent<Controllers.PlayerStatsController>().ToPlayerStatsData()))
            {
                ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
            }
        }

        public void SpawnLocalPlayer(SpawnLocalPlayerResponseData data)
        {
            if (!connectedPlayers.ContainsKey(data.ID))
            {
                localPlayer = Instantiate(playerPrefab, playerPrefab.transform.position, Quaternion.identity);
                localPlayerId = data.ID;
            }
        }

        public void SpawnOtherPlayer(ushort id)
        {
            Debug.Log("Attempting to add another player: " + id);
            if (!connectedPlayers.ContainsKey(id))
            {
                Debug.Log("Adding other player: " + id);
                var otherPlayer = Instantiate(networkedPlayerPrefab, networkedPlayerPrefab.transform.position, Quaternion.identity);
                connectedPlayers.Add(id, otherPlayer);
                otherPlayer.name = id.ToString();
                
            }
        }

        public bool IsLocalPlayer(GameObject obj)
        {
            //return true;
            return UnityEngine.Object.Equals(obj, localPlayer);
        }

        internal void SyncOtherPlayer(PlayerStateData playerStateData)
        {
            Debug.Log("Syncing another player with id: " + playerStateData.id);
            if (localPlayerId == playerStateData.id) return;
            connectedPlayers[playerStateData.id].GetComponent<Controllers.EntityStateSyncController>().SyncState(playerStateData);
        }

        internal void RemovePlayer(ushort id)
        {
            if (!connectedPlayers.ContainsKey(id)) return;
            Destroy(connectedPlayers[id]);
            connectedPlayers.Remove(id);
        }

        internal void SyncOtherPlayerStats(PlayerStatsData data)
        {
            Debug.Log("Syncing stats from player with id:" + data.id);
            if (localPlayerId == data.id) return;
            connectedPlayers[data.id].GetComponent<Controllers.EntityStateSyncController>().SyncStats(data);
        }
    }
}
