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
    public class WeaponController : NetworkBehaviour
    {
        public Weapon currentWeapon;
        public PlayerMovementController playerMovementController;
        public float isFiring;
        public bool isAiming;
        public float wantsToAim;

        // Player inputs.
        Vector2 lookInput;

        public GameObject weaponModel;
        public float lastFired = 0;
        public float fireDelay = 100;
        public Transform muzzleTransform;
        public GameObject effect;

        // Target
        public GameObject target;

        // Coroutines.
        private IEnumerator toAdsCoroutine;
        private IEnumerator toHipCoroutine;
        private float coroutinePercent;

        // Recoil.
        public CameraRecoil cameraRecoil;

        // Stance.
        public int stance = 0;

        void Start()
        {
            playerMovementController = GetComponent<PlayerMovementController>();

            lookInput = new Vector2(0, 0);
            if (currentWeapon == null)
            {
                Debug.Log("No Weapon equipped");
                return;
            }
            // A weapon is equipped. Instantiating the weapon and computing some values.
            //weaponModel = Instantiate(currentWeapon.weaponModel, transform.Find("First-Person").transform.Find("Head Location/Recoil").transform);
            //weaponModel = GameObject.Find("xbot/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RigthArm/mixamorig:RightForeArm/mixamorig:RightHand/Rifle World");
            lastFired = 0;
            fireDelay = 60.0f / currentWeapon.fireRate;
            muzzleTransform = weaponModel.transform.Find("Muzzle").transform;

            // Instantiating VFX.
            effect = Instantiate(currentWeapon.muzzleEffect, muzzleTransform);
            effect.GetComponent<VisualEffect>().Stop();

            // Setting up recoil.
            cameraRecoil = transform.Find("First-Person").transform.Find("Head Location").Find("Recoil").GetComponent<CameraRecoil>();

            // Setting hand effectors.
            var ik = transform.Find("First-Person").GetComponents<RootMotion.FinalIK.LimbIK>();
            ik[0].solver.target = weaponModel.transform.Find("Weapon Left Hand Effector").transform;
            //ik[1].solver.target = weaponModel.transform.Find("RightHandEffector").transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;
            if (lastFired > 0)
            {
                lastFired -= Time.deltaTime;
            }
            if (currentWeapon == null)
            {
                Debug.Log("No Weapon equipped");
                return;
            }
            if (isFiring > 0 && lastFired <= 0)
            {
                Fire();
            }
        }

        [Client]
        private void Fire()
        {
            // Gathering the tick the firing command was requested on the client.
            PreciseTick pt = base.TimeManager.GetPreciseTick(base.TimeManager.LastPacketTick);
            lastFired = fireDelay;
            PlayMuzzleVisualEffect();
            PlayFireAudio();
            // Calling Fire on the server.
            ServerFire(pt);
        }

        [ServerRpc]
        private void ServerFire(PreciseTick pt)
        {
            // TODO: implement rollback.
            var projectile = Instantiate(currentWeapon.projectile, muzzleTransform.position, Quaternion.LookRotation(target.transform.position - muzzleTransform.position, Vector3.up));
            Spawn(projectile, Owner);
            ObserversFire();
        }

        [ObserversRpc]
        private void ObserversFire()
        {
            if (IsOwner || IsServer)
            {
                return;
            }
            PlayFireAudio();
            PlayMuzzleVisualEffect();
        }


        public void PlayFireAudio()
        {
            if (weaponModel.GetComponent<AudioSource>() == null)
            {
                return;
            }
            weaponModel.GetComponent<AudioSource>().clip = currentWeapon.fireAudio;
            weaponModel.GetComponent<AudioSource>().Play();
        }

        public void PlayMuzzleVisualEffect()
        {
            effect.GetComponent<VisualEffect>().Play();
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
            this.lookInput = context.ReadValue<Vector2>();
        }

        #endregion
    }

}
