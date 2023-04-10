using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using Ascendant.Networking;
using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Ascendant
{
    public sealed class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SyncObject]
        public readonly SyncList<Player> players = new();


        public GameObject playerPrefab;
        public Dictionary<ushort, GameObject> connectedPlayers = new Dictionary<ushort, GameObject>();
        public ushort localPlayerId;
        
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
        }
        // Start is called before the first frame update
        void Start()
        {
            //localPlayer = GameObject.Find("NetworkedPlayer");
            
            //localPlayerId = ConnectionManager.Instance.Client.ID;
            //Debug.Log("Local Player ID set to: " + localPlayerId);
            //ConnectionManager.Instance.SpawnPlayerOnServerRequest();
            
        }

        private void Update()
        {
            if (!IsServer) return;
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

        internal void SyncOtherPlayer(PlayerStateData playerStateData)
        {
            Debug.Log("Syncing another player with id: " + playerStateData.id);
            if (localPlayerId == playerStateData.id) return;
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
        }
    }
}
