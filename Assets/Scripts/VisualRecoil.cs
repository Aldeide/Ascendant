using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant
{
    public class VisualRecoil : MonoBehaviour
    {
        private Quaternion targetRotation;
        private Quaternion currentRotation;
        private Vector3 targetPosition;
        private Vector3 currentPosition;
        public Vector3 initialPosition;
        public Quaternion initialRotation;

        public float returnAmount;
        public float snappiness;

        public float recoilX;
        public float recoilY;
        public float recoilZ;
        public float kickbackZ;

        private WeaponManager weaponManager;

        void Start()
        {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
            weaponManager = GameObject.Find("Player").GetComponent<WeaponManager>();
        }

        // Update is called once per frame
        void Update()
        {
            if (weaponManager.wantsToAim > 0)
            {
                return;
            }
            if (weaponManager.isAiming)
            {
                initialPosition = weaponManager.currentWeapon.aimingDownSightsPosition;
            }
            else
            {
                initialPosition = weaponManager.currentWeapon.hipFirePosition;
            }
            if (weaponManager.lastFired < 0f)
            {
                return;
            }
            targetRotation = Quaternion.Lerp(targetRotation, initialRotation, Time.deltaTime * returnAmount);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * snappiness);
            transform.localRotation = currentRotation;
            Kickback();
        }

        public void Recoil()
        {
            targetPosition -= new Vector3(0, 0, kickbackZ);

            targetRotation *= Quaternion.AngleAxis(recoilX, transform.right);
            targetRotation *= Quaternion.AngleAxis(Random.Range(-recoilY, recoilY), transform.up);
            targetRotation *= Quaternion.AngleAxis(Random.Range(-recoilZ, recoilZ), transform.forward);
        }

        private void Kickback()
        {
            targetPosition = Vector3.Lerp(targetPosition, initialPosition, Time.deltaTime * returnAmount);
            currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime * snappiness);
            transform.localPosition = currentPosition;
        }
    }
}

