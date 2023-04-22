using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

namespace Ascendant
{
    public struct InputData
    {
        public Vector2 movementInput;
        public Vector2 lookInput;
        public float sprintInput;
        public float crouchInput;
        public float aimInput;
        public float fireInput;
        public float jumpInput;
    }


    public class PlayerInputController : NetworkBehaviour
    {
        public InputData inputData;

        // Callbacks.
        public void OnMoveCallback(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            inputData.movementInput = context.ReadValue<Vector2>();
        }
        public void OnLookCallback(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            inputData.lookInput = context.ReadValue<Vector2>();
        }
        public void OnSprintCallback(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            inputData.sprintInput = context.ReadValue<float>();
        }
        public void OnCrouchCallback(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            inputData.crouchInput = context.ReadValue<float>();
        }
        public void OnAimCallback(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            inputData.aimInput = context.ReadValue<float>();
        }
        public void OnJumpCallback(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            inputData.jumpInput = context.ReadValue<float>();
        }
        public void OnFireCallBack(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            inputData.fireInput = context.ReadValue<float>();
        }

        public void OnRespawnCallback(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            Respawn();
        }

        [ServerRpc]
        private void Respawn()
        {
            this.GetComponent<CharacterController>().enabled = false;
            this.transform.position = new Vector3(0, 0, 0);
            this.GetComponent<CharacterController>().enabled = true;
        }
    }
}

