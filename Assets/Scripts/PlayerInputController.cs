using UnityEngine;
using UnityEngine.InputSystem;

namespace Ascendant
{
    public class PlayerInputController : MonoBehaviour
    {
        public Vector2 movementInput = new Vector2();
        public Vector2 lookInput = new Vector2();
        public float sprintInput = 0f;
        public float crouchInput = 0f;
        public float aimInput = 0f;
        public float fireInput = 0f;
        public float jumpInput = 0f;

        // Callbacks.
        public void OnMoveCallback(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }
        public void OnLookCallback(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        public void OnSprintCallback(InputAction.CallbackContext context)
        {
            sprintInput = context.ReadValue<float>();
        }
        public void OnCrouchCallback(InputAction.CallbackContext context)
        {
            crouchInput = context.ReadValue<float>();
        }
        public void OnAimCallback(InputAction.CallbackContext context)
        {
            aimInput = context.ReadValue<float>();
        }
        public void OnJumpCallback(InputAction.CallbackContext context)
        {
            jumpInput = context.ReadValue<float>();
        }
        public void OnFireCallBack(InputAction.CallbackContext context)
        {
            fireInput = context.ReadValue<float>();
        }

        public Networking.PlayerInputData ToPlayerInputData()
        {
            return new Networking.PlayerInputData(movementInput, this.transform.rotation, sprintInput, crouchInput, aimInput, fireInput, jumpInput, 1);
        }

    }
}

