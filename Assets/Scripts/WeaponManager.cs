using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

namespace Ascendant
{
    public class WeaponManager : MonoBehaviour
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
        private GameObject target;

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
            target = GameObject.Find("Target");
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
            /*
            if (playerMovementController.isSprinting && stance != 1)
            {
                weaponModel.transform.localPosition = currentWeapon.sprintPosition;
                weaponModel.transform.localRotation = Quaternion.Euler(currentWeapon.sprintRotation);
                stance = 1;
            }
            if (playerMovementController.isSliding || playerMovementController.crouchInput > 0 && stance != 2)
            {
                weaponModel.transform.localPosition = currentWeapon.crouchPosition;
                weaponModel.transform.localRotation = Quaternion.Euler(currentWeapon.crouchRotation);
                stance = 2;
            }
            if (!playerMovementController.isSliding && !playerMovementController.isSprinting && playerMovementController.crouchInput == 0 && stance != 0)
            {
                weaponModel.transform.localPosition = currentWeapon.hipFirePosition;
                weaponModel.transform.localRotation = Quaternion.Euler(currentWeapon.hipFireRotation);
                stance = 0;
            }




            AimingDownSights();
            */
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
                // Handling camera movement for recoil (produces actual recoil) and weapon visual recoil (purely visual).
                //cameraRecoil.RecoilFire();
                //WeaponRecoil();

                // Instantiating a projectile that moves towards the target.
                var projectile = Instantiate(currentWeapon.projectile, muzzleTransform.position, Quaternion.LookRotation(target.transform.position - muzzleTransform.position, Vector3.up));
                lastFired = fireDelay;
                effect.GetComponent<VisualEffect>().Play();
                PlayFireAudio();
            }
            // Handling weapon sway.
            //WeaponSway();
        }

        public void AimingDownSights()
        {
            if (wantsToAim == 0 && toAdsCoroutine == null && toHipCoroutine == null && !isAiming)
            {
                return;
            }

            // Player is aiming down the sights but hasn't reached ADS yet.
            if (wantsToAim > 0 && toAdsCoroutine == null && !isAiming)
            {
                toAdsCoroutine = AnimateAiming(weaponModel.transform.localPosition, currentWeapon.aimingDownSightsPosition, currentWeapon.adsDelay, currentWeapon.aimDownSightsAnimationCurve);
                StartCoroutine(toAdsCoroutine);
            }
            // Player was aiming and wants to stop ADS.
            if (wantsToAim == 0 && isAiming)
            {
                toHipCoroutine = AnimateHip(weaponModel.transform.localPosition, currentWeapon.hipFirePosition, currentWeapon.adsDelay, currentWeapon.aimDownSightsAnimationCurve);
                StartCoroutine(toHipCoroutine);
                isAiming = false;
            }
            // Player has stopped aiming mid-transition to ADS
            if (wantsToAim == 0 && toAdsCoroutine != null && !isAiming)
            {
                // Stopping current animation to ADS.
                StopCoroutine(toAdsCoroutine);
                toAdsCoroutine = null;
                // Going back to hip fire.
                if (toHipCoroutine != null)
                {
                    StopCoroutine(toHipCoroutine);
                    toHipCoroutine = null;
                }

                toHipCoroutine = AnimateHip(weaponModel.transform.localPosition, currentWeapon.hipFirePosition, currentWeapon.adsDelay * coroutinePercent, currentWeapon.aimDownSightsAnimationCurve);
                StartCoroutine(toHipCoroutine);
            }
            // Player was going through transition to hip fire but wants ADS again.
            if (wantsToAim > 0 && toHipCoroutine != null && !isAiming)
            {
                // Stopping current animation to hip fire.
                StopCoroutine(toHipCoroutine);
                toHipCoroutine = null;
                // Going backs to ADS.
                if (toAdsCoroutine != null)
                {
                    StopCoroutine(toAdsCoroutine);
                    toAdsCoroutine = null;
                }
                toAdsCoroutine = AnimateAiming(weaponModel.transform.localPosition, currentWeapon.aimingDownSightsPosition, currentWeapon.adsDelay * coroutinePercent, currentWeapon.aimDownSightsAnimationCurve);
                StartCoroutine(toAdsCoroutine);
            }

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

        // Weapon Sway method. Moves the weapon based on movement inputs.
        public void WeaponSway()
        {
            if (isAiming || wantsToAim > 0)
            {
                return;
            }
            if (isFiring > 0)
            {
                return;
            }
            // Translation.
            float moveX = lookInput.x * currentWeapon.translationSwayFactor;
            float moveY = lookInput.y * currentWeapon.translationSwayFactor;
            moveX = Mathf.Clamp(moveX, -currentWeapon.maxTranslationSway, currentWeapon.maxTranslationSway);
            moveY = Mathf.Clamp(moveY, -currentWeapon.maxTranslationSway, currentWeapon.maxTranslationSway);
            Vector3 finalPosition = new Vector3(moveX, moveY, 0);
            weaponModel.transform.localPosition =
                Vector3.Lerp(
                    weaponModel.transform.localPosition,
                    finalPosition + currentWeapon.hipFirePosition,
                    Time.deltaTime * currentWeapon.translationSwaySmoothingFactor);
            // Rotation
            float rotateX = lookInput.y * currentWeapon.rotationSwayFactor;
            float rotateY = lookInput.x * currentWeapon.rotationSwayFactor;
            rotateX = Mathf.Clamp(rotateX, -currentWeapon.maxRotationSway, currentWeapon.maxRotationSway);
            rotateY = Mathf.Clamp(rotateY, -currentWeapon.maxRotationSway, currentWeapon.maxRotationSway);
            Quaternion finalRotation = Quaternion.Euler(new Vector3(-rotateX, rotateY, rotateY));
            weaponModel.transform.localRotation =
                Quaternion.Slerp(
                    weaponModel.transform.localRotation,
                    finalRotation * Quaternion.Euler(currentWeapon.hipFireRotation),
                    Time.deltaTime * currentWeapon.rotationSwaySmoothingFactor);
        }

        // Weapon recoil.
        public void WeaponRecoil()
        {
            //weaponModel.GetComponent<VisualRecoil>().Recoil();
        }

        // Coroutine Methods.
        IEnumerator AnimateAiming(Vector3 origin, Vector3 target, float duration, AnimationCurve curve)
        {
            float journey = 0f;
            while (journey < duration)
            {
                journey += Time.deltaTime;
                float percent = Mathf.Clamp01(journey / duration);
                float curvePercent = curve.Evaluate(percent);
                coroutinePercent = curvePercent;
                weaponModel.transform.localPosition = Vector3.Lerp(origin, target, curvePercent);
                yield return null;
            }
            isAiming = true;
            toAdsCoroutine = null;
        }

        IEnumerator AnimateHip(Vector3 origin, Vector3 target, float duration, AnimationCurve curve)
        {
            float journey = 0f;
            while (journey < duration)
            {
                journey += Time.deltaTime;
                float percent = Mathf.Clamp01(journey / duration);
                float curvePercent = curve.Evaluate(percent);
                coroutinePercent = curvePercent;
                weaponModel.transform.localPosition = Vector3.Lerp(origin, target, curvePercent);
                yield return null;
            }
            toHipCoroutine = null;
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
