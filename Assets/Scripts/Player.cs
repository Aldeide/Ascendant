using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.AddressableAssets;

namespace Ascendant
{
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public string username;

        [SyncVar]
        public Character controlledCharacter;

        private bool spawnRequested = false;

        public override void OnStartServer()
        {
            base.OnStartServer();
            GameManager.Instance.players.Add(this);
            
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (controlledCharacter == null && !spawnRequested)
            {
                ServerSpawnCharacter();
                spawnRequested = true;
            }
            if (GameManager.Instance.localPlayer == null)
            {
                GameManager.Instance.localPlayer = this.gameObject;
            }
        }

        [ServerRpc]
        private void ServerSpawnCharacter()
        {
            GameObject characterPrefab = Addressables.LoadAssetAsync<GameObject>("Character").WaitForCompletion();
            GameObject characterInstance = Instantiate(characterPrefab);
            
            this.controlledCharacter = characterInstance.GetComponent<Character>();
            controlledCharacter.controllingPlayer = this;
            Spawn(characterInstance, Owner);
        }

    }
}

