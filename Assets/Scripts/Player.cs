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
        [SyncVar]
        public GameObject aimTarget;

        private bool spawnRequested = false;

        [SerializeField]
        private GameObject targetPrefab;

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
                //ServerSpawnTarget();
                spawnRequested = true;
                GameManager.Instance.localPlayer = this.gameObject;
            }
            if (controlledCharacter != null)
            {
                GameManager.Instance.ActivateUI();
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

        [ServerRpc]
        private void ServerSpawnTarget()
        {
            GameObject target = Instantiate(targetPrefab);
            aimTarget = target;
            controlledCharacter.gameObject.GetComponent<Controllers.PlayerStateController>().aimTarget = target;
            controlledCharacter.gameObject.GetComponent<Controllers.WeaponController>().target = target;
            controlledCharacter.gameObject.GetComponent<RootMotion.FinalIK.AimIK>().solver.target = target.transform;
            Spawn(target, Owner);
        }

    }
}

