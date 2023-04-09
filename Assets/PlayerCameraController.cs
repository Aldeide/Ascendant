using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
namespace Ascendant.Controllers
{
    public class PlayerCameraController : NetworkBehaviour
    {
        // Cinemachine cameras.
        public GameObject defaultCamera;
        public GameObject aimCamera;
        public GameObject sprintCamera;

        // Player components.
        private PlayerStateController stateController;

        void Start()
        {
            stateController = GetComponent<PlayerStateController>();
            defaultCamera = GameObject.Find("Camera - Third-Person");
            sprintCamera = GameObject.Find("Camera - Sprinting");
            aimCamera = GameObject.Find("Game/Cameras/CameraAim");
            defaultCamera.SetActive(true);
            sprintCamera.SetActive(false);
            aimCamera.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;
            // Activate aim camera if needed. The aim camera is much closer to the player.
            if (stateController.IsAiming() && !aimCamera.activeInHierarchy)
            {
                defaultCamera.SetActive(false);
                sprintCamera.SetActive(false);
                aimCamera.SetActive(true);
                return;
            }

            // Activate the sprint camera if needed. The sprint camera follows the player from afar.
            // Sprinting isn't possible while aiming.
            if (stateController.IsSprinting()
                && !sprintCamera.activeInHierarchy
                && !stateController.IsAiming())
            {
                defaultCamera.SetActive(false);
                sprintCamera.SetActive(true);
                aimCamera.SetActive(false);
                return;
            }

            // Activate the default camera if needed.
            if (!defaultCamera.activeInHierarchy
                && !stateController.IsAiming()
                && !stateController.IsSprinting())
            {
                defaultCamera.SetActive(true);
                sprintCamera.SetActive(false);
                aimCamera.SetActive(false);
            }
        }
    }
}


