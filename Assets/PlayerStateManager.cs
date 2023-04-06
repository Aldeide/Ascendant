using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ascendant
{
    public enum PlayerMovementState
    {
        Idle,
        Running,
        Sprinting
    }

    public enum PlayerStanceState
    {
        Upright,
        Crouched,
        Aiming
    }

    public enum PlayerGroundedState
    {
        Grounded,
        Jumping,
        Falling,
        Climbing
    }

    public enum PlayerFiringState
    {
        NotFiring,
        Firing
    }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerMovementController))]
    public class PlayerStateManager : MonoBehaviour
    {
        private Animator animator;
        private PlayerMovementController movementController;
        private PlayerInputController inputController;

        public PlayerStanceState stanceState;
        public PlayerMovementState movementState;
        private PlayerGroundedState groundedState;
        public PlayerGroundedState GroundedState
        {
            get => groundedState;
            set
            {
                if (groundedState != value)
                {
                    groundedState = value;
                    Debug.Log("PlayerGroundedState changed to: " + value);
                }
            }
        }
        public PlayerFiringState firingState;
        private Ray dectectClimbableRay;
        private RaycastHit hitInfo;

        public float timeSinceLastFired;
        public float firingTimeStanceDelay = 1.2f;

        public LayerMask layerMask;

        void Start()
        {
            // Fetch required components.
            animator = GetComponent<Animator>();
            movementController = GetComponent<PlayerMovementController>();
            inputController = GetComponent<PlayerInputController>();

            // Initialise variables.
            stanceState = PlayerStanceState.Upright;
            movementState = PlayerMovementState.Idle;
        }

        void Update()
        {
            if (!GameManager.Instance.IsLocalPlayer(this.gameObject)) return;
            // Movement state update.
            if (inputController.movementInput.sqrMagnitude > 0)
            {
                movementState = PlayerMovementState.Running;
                if (inputController.sprintInput > 0)
                {
                    movementState = PlayerMovementState.Sprinting;
                }
            }
            else
            {
                movementState = PlayerMovementState.Idle;
            }

            // Stance state update.
            if (inputController.crouchInput > 0)
            {
                stanceState = PlayerStanceState.Crouched;
            }

            // Aiming overrides crouching.
            if (inputController.aimInput > 0)
            {
                stanceState = PlayerStanceState.Aiming;
            }
            else
            {
                stanceState = PlayerStanceState.Upright;
            }

            // Jumping.
            if (inputController.jumpInput > 0 && movementController.IsGrounded())
            {
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsGrounded", false);
            }
            else
            {
                animator.SetBool("IsJumping", false);
            }

            // Firing
            if (inputController.fireInput > 0)
            {
                // TODO: maintaining the fire button but not being able to fire (e.g. reloading) should not reset
                // the timer.
                firingState = PlayerFiringState.Firing;
                timeSinceLastFired = 0;
            }
            timeSinceLastFired += Time.deltaTime;
            if (timeSinceLastFired > firingTimeStanceDelay)
            {
                firingState = PlayerFiringState.NotFiring;
            }

            // Climbing        
            dectectClimbableRay = new Ray(transform.position, transform.forward);
            if (GroundedState != PlayerGroundedState.Climbing
                && Physics.Raycast(dectectClimbableRay, out hitInfo, 0.8f)
                && hitInfo.collider.gameObject.tag == "Climbable"
                && inputController.movementInput.y > 0)
            {
                GroundedState = PlayerGroundedState.Climbing;
            }


            // Animator updates.
            if (inputController.movementInput.sqrMagnitude > 0 && stanceState != PlayerStanceState.Aiming && firingState != PlayerFiringState.Firing)
            {
                animator.SetFloat("MovementY", 1.0f, 10.1f, 3.5f);
                animator.SetFloat("MovementX", 0.0f, 10.1f, 3.5f);
                animator.SetBool("isRunning", true);
            }
            if (inputController.movementInput.sqrMagnitude > 0 && (stanceState == PlayerStanceState.Aiming || firingState == PlayerFiringState.Firing))
            {
                animator.SetFloat("MovementX", inputController.movementInput.x, 10.1f, 3.5f);
                animator.SetFloat("MovementY", inputController.movementInput.y, 10.1f, 3.5f);
                animator.SetBool("isRunning", true);
            }
            if (inputController.movementInput.sqrMagnitude < 0.01)
            {
                animator.SetBool("isRunning", false);
            }
            // Jumping is handled inside the callback function.

            // Falling
            if (!movementController.IsGrounded() && FallHeight() > 0.4f)
            {
                animator.SetBool("IsFalling", true);
                //animator.SetBool("IsJumping", false);
                animator.SetBool("IsGrounded", false);
            }
            if (movementController.IsGrounded())
            {
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsGrounded", true);
            }
        }

        // Plants.
        public void Plant180()
        {
            // TODO: implement animation plants.
        }

        // Raycasts
        public float FallHeight()
        {
            Ray ray = new Ray(transform.position, -1.0f * transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f, layerMask, QueryTriggerInteraction.Ignore))
            {
                // Debug.Log(hit.distance);
                return hit.distance;
            }
            return 0;
        }

        public Networking.PlayerStateData ToPlayerStateData()
        {
            return new Networking.PlayerStateData(GameManager.Instance.localPlayerId, -9.81f, this.transform.position, this.transform.rotation);
        }

    }
}

