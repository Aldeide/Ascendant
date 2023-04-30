using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;
using FishNet.Object;
using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;

namespace Ascendant.Controllers
{
    public enum WeaponSlot
    {
        Unarmed = 0,
        Primary = 1,
        Secondary = 2
    }


    public class WeaponController : NetworkBehaviour
    {
        // Equipped weapons.
        [Header("Equipped weapons")]
        public Weapon primaryWeapon;
        public Weapon secondaryWeapon;

        // Weapon location.
        [Header("Weapon location")]
        public Transform hand;

        // Currently equipped weapon.
        [Header("Current weapon")]
        public Weapon weaponData;
        [SyncVar]
        public WeaponSlot weaponSlot = WeaponSlot.Primary;

        [Header("Controller")]
        public PlayerMovementController playerMovementController;
        public PlayerInputController playerInputController;
        public PlayerStateController playerStateController;

        public float isFiring;
        public bool isAiming;
        public float wantsToAim;

        public GameObject weaponModel;
        public float lastFired = 0;
        public float fireDelay = 100;
        public Transform muzzleTransform;
        public VisualEffect effect;

        // Target
        public GameObject target;

        // Recoil.
        public CameraRecoil cameraRecoil;

        // Stance.
        public int stance = 0;

        void Start()
        {
            playerMovementController = GetComponent<PlayerMovementController>();
            playerInputController = GetComponent<PlayerInputController>();
            playerStateController = GetComponent<PlayerStateController>();

            weaponSlot = WeaponSlot.Primary;
            playerStateController.entityStateModel.weaponTypeState = Models.EntityWeaponTypeState.Rifle;
            SwitchWeapon(weaponSlot);
        }

        // Update is called once per frame
        void Update()
        {
            if (weaponModel != null && weaponModel.active == false)
            {
                weaponModel.SetActive(true);

            }
            if (muzzleTransform != null && muzzleTransform.gameObject.active == false)
            {
                muzzleTransform.gameObject.SetActive(true);
            }

            if (!IsOwner) return;

            // If the player is incapacitated, they can't use weapons.
            if (playerMovementController.stateController.entityStateModel.aliveState == Models.EntityAliveState.Dead) return;

            // Handling weapon switching.
            HandleWeaponSwitching();

            // If the player is unarmed, they can't fire.
            if (weaponSlot == WeaponSlot.Unarmed)
            {
                return;
            }

            if (lastFired > 0)
            {
                lastFired -= Time.deltaTime;
            }
            if (weaponData == null)
            {
                Debug.Log("No Weapon equipped");
                return;
            }
            if (playerInputController.inputData.fireInput > 0 && lastFired <= 0)
            {
                Fire();
            }
        }

        [Client]
        private void Fire()
        {
            lastFired = fireDelay;
            ServerFire();
            effect.Play();
            PlayFireAudio();
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void ObserversFire()
        {
            Debug.Log("Playing VFX on: " + OwnerId);
            effect.Play();
            PlayFireAudio();
        }

        [ServerRpc]
        private void ServerFire()
        {
            ObserversFire();
            // TODO: implement rollback.
            var projectile = Instantiate(weaponData.projectile, muzzleTransform.position, Quaternion.LookRotation(target.transform.position - muzzleTransform.position, Vector3.up));
            Spawn(projectile, Owner);
        }

        public void PlayFireAudio()
        {
            if (weaponModel.GetComponent<AudioSource>() == null)
            {
                return;
            }
            weaponModel.GetComponent<AudioSource>().clip = weaponData.fireAudio;
            weaponModel.GetComponent<AudioSource>().Play();
        }

        public void PlayMuzzleVisualEffect()
        {
            effect.GetComponent<VisualEffect>().Play();
        }

        // Determines whether weapons need to be switched.
        [Client]
        private void HandleWeaponSwitching()
        {
            if (playerInputController.inputData.primaryWeaponInput > 0 && weaponSlot != WeaponSlot.Primary) {
                SwitchWeapon(WeaponSlot.Primary);
                return;
            }
            if (playerInputController.inputData.secondaryWeaponInput > 0 && weaponSlot != WeaponSlot.Secondary)
            {
                SwitchWeapon(WeaponSlot.Secondary);
                return;
            }
            if (playerInputController.inputData.unarmedInput > 0 && weaponSlot != WeaponSlot.Unarmed)
            {
                SwitchWeapon(WeaponSlot.Unarmed);
            }

        }

        // Changes the currently equipped weapon on the client.
        private void SwitchWeapon(WeaponSlot slot)
        {
            if (slot == WeaponSlot.Primary)
            {
                weaponData = primaryWeapon;
            }
            if (slot == WeaponSlot.Secondary)
            {
                weaponData = secondaryWeapon;
            }
            if (slot == WeaponSlot.Unarmed)
            {
                Destroy(weaponModel);
                weaponSlot = slot;
                SwitchWeaponServer(slot);
                return;
            }
            Debug.Log("switching");
            //Despawn(weaponModel);
            Destroy(weaponModel);
            weaponModel = Instantiate(weaponData.weaponModel, hand);
            weaponModel.transform.localRotation = weaponData.rotation;
            weaponModel.transform.localPosition = weaponData.position;
            //Spawn(weaponModel, Owner);
            lastFired = 0;
            fireDelay = 60.0f / weaponData.fireRate;
            muzzleTransform = weaponModel.transform.Find("Muzzle").transform;

            effect = muzzleTransform.GetComponent<VisualEffect>();
            effect.Stop();
            muzzleTransform.GetComponent<VisualEffect>().enabled = true;
            weaponSlot = slot;

            if (weaponModel.transform.Find("LeftHandEffector") != null)
            {
                var effector = weaponModel.transform.Find("LeftHandEffector");
                GetComponent<RootMotion.FinalIK.LimbIK>().solver.target = effector.transform;
                if (weaponModel.transform.Find("LeftHandBend"))
                {
                    GetComponent<RootMotion.FinalIK.LimbIK>().solver.bendGoal = weaponModel.transform.Find("LeftHandBend").transform;
                }
                GetComponent<RootMotion.FinalIK.LimbIK>().enabled = true;
            } else
            {
                GetComponent<RootMotion.FinalIK.LimbIK>().enabled = false;
            }

            GetComponent<RootMotion.FinalIK.AimIK>().solver.transform = muzzleTransform;
            SwitchWeaponServer(slot);
        }

        // Changes the currently equipped weapon on the server.
        [ServerRpc]
        private void SwitchWeaponServer(WeaponSlot slot)
        {
            if (slot == WeaponSlot.Primary)
            {
                weaponData = primaryWeapon;
            }
            if (slot == WeaponSlot.Secondary)
            {
                weaponData = secondaryWeapon;
            }
            if (slot == WeaponSlot.Unarmed)
            {
                Destroy(weaponModel);
                weaponSlot = slot;
                playerStateController.entityStateModel.weaponTypeState = Models.EntityWeaponTypeState.Unarmed;
                SwitchWeaponObservers(slot);
                return;
            }
            //Despawn(weaponModel);
            Destroy(weaponModel);
            weaponModel = Instantiate(weaponData.weaponModel, hand);
            weaponModel.transform.localRotation = weaponData.rotation;
            weaponModel.transform.localPosition = weaponData.position;
            //Spawn(weaponModel, Owner);
            lastFired = 0;
            fireDelay = 60.0f / weaponData.fireRate;
            muzzleTransform = weaponModel.transform.Find("Muzzle").transform;

            effect = muzzleTransform.GetComponent<VisualEffect>();
            effect.Stop();
            muzzleTransform.GetComponent<VisualEffect>().enabled = true;
            weaponSlot = slot;
            if (slot == WeaponSlot.Primary)
            {
                playerStateController.entityStateModel.weaponTypeState = Models.EntityWeaponTypeState.Rifle;
            }
            if (slot == WeaponSlot.Secondary)
            {
                playerStateController.entityStateModel.weaponTypeState = Models.EntityWeaponTypeState.Handgun;
            }
            if (weaponModel.transform.Find("LeftHandEffector") != null)
            {
                var effector = weaponModel.transform.Find("LeftHandEffector");
                GetComponent<RootMotion.FinalIK.LimbIK>().solver.target = effector.transform;
                if (weaponModel.transform.Find("LeftHandBend"))
                {
                    GetComponent<RootMotion.FinalIK.LimbIK>().solver.bendGoal = weaponModel.transform.Find("LeftHandBend").transform;
                }
                GetComponent<RootMotion.FinalIK.LimbIK>().enabled = true;
            }
            else
            {
                GetComponent<RootMotion.FinalIK.LimbIK>().enabled = false;
            }

            GetComponent<RootMotion.FinalIK.AimIK>().solver.transform = muzzleTransform;
            SwitchWeaponObservers(slot);
        }

        // Changes the currently equipped weapon on observers.
        [ObserversRpc(BufferLast = true)]
        private void SwitchWeaponObservers(WeaponSlot slot)
        {
            if (slot == WeaponSlot.Primary)
            {
                weaponData = primaryWeapon;
            }
            if (slot == WeaponSlot.Secondary)
            {
                weaponData = secondaryWeapon;
            }
            if (slot == WeaponSlot.Unarmed)
            {
                Destroy(weaponModel);
                weaponSlot = slot;
                return;
            }
            //Despawn(weaponModel);
            Destroy(weaponModel);
            weaponModel = Instantiate(weaponData.weaponModel, hand);
            weaponModel.transform.localRotation = weaponData.rotation;
            weaponModel.transform.localPosition = weaponData.position;
            //Spawn(weaponModel, Owner);
            lastFired = 0;
            fireDelay = 60.0f / weaponData.fireRate;
            muzzleTransform = weaponModel.transform.Find("Muzzle").transform;

            effect = muzzleTransform.GetComponent<VisualEffect>();
            effect.Stop();
            muzzleTransform.GetComponent<VisualEffect>().enabled = true;
            weaponSlot = slot;

            if (weaponModel.transform.Find("LeftHandEffector") != null)
            {
                var effector = weaponModel.transform.Find("LeftHandEffector");
                GetComponent<RootMotion.FinalIK.LimbIK>().solver.target = effector.transform;
                if (weaponModel.transform.Find("LeftHandBend"))
                {
                    GetComponent<RootMotion.FinalIK.LimbIK>().solver.bendGoal = weaponModel.transform.Find("LeftHandBend").transform;
                }
                GetComponent<RootMotion.FinalIK.LimbIK>().enabled = true;
            }
            else
            {
                GetComponent<RootMotion.FinalIK.LimbIK>().enabled = false;
            }

            GetComponent<RootMotion.FinalIK.AimIK>().solver.transform = muzzleTransform;
        }
        // Input callbacks.
        #region Input Callbacks
        public void OnFire(InputAction.CallbackContext context)
        {
            isFiring = context.ReadValue<float>();
        }

        public void OnAiming(InputAction.CallbackContext context)
        {
            wantsToAim = context.ReadValue<float>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            //this.lookInput = context.ReadValue<Vector2>();
        }

        #endregion
    }

}
