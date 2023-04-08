using Ascendant.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ascendant.Controllers
{


    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerStatsManager))]
    public class PlayerStateController : MonoBehaviour
    {
        private Animator animator;
        private PlayerMovementController movementController;
        private PlayerInputController inputController;
        private PlayerStatsManager statsManager;

        public Models.EntityStateModel entityStateModel { get; set; }
        public Models.EntityStateModel previousEntityStateModel { get; set; }

        private Ray dectectClimbableRay;
        private RaycastHit hitInfo;

        public float timeSinceLastFired;
        public float firingTimeStanceDelay = 1.2f;
        public float respawnDelay = 5.0f;

        public LayerMask layerMask;

        void Start()
        {
            // Fetch required components.
            animator = GetComponent<Animator>();
            movementController = GetComponent<PlayerMovementController>();
            inputController = GetComponent<PlayerInputController>();
            statsManager = GetComponent<PlayerStatsManager>();

            // Initialise state model.
            entityStateModel = new EntityStateModel(transform.position, transform.forward, transform.rotation, new Vector3());
        }

        void Update()
        {
            if (!GameManager.Instance.IsLocalPlayer(this.gameObject)) return;

            // Death State.
            if (statsManager.currentHealth <= 0)
            {
                if (entityStateModel.aliveState == EntityAliveState.Alive)
                {
                    Debug.Log("Died");
                    entityStateModel.timeOfDeath = Time.time;
                    animator.SetBool("isDead", true);
                    animator.SetTrigger("died");
                    entityStateModel.aliveState = EntityAliveState.Dead;
                }
            } else
            {
                entityStateModel.aliveState = EntityAliveState.Alive;
                animator.SetBool("isDead", false);
            }
            if (entityStateModel.aliveState == EntityAliveState.Dead && Time.time > entityStateModel.timeOfDeath + respawnDelay)
            {
                Respawn();
            }

            // Movement state update.
            if (inputController.movementInput.sqrMagnitude > 0)
            {
                entityStateModel.movementState = EntityMovementState.Running;
                if (inputController.sprintInput > 0)
                {
                    entityStateModel.movementState = EntityMovementState.Sprinting;
                }
            }
            else
            {
                entityStateModel.movementState = EntityMovementState.Idle;
            }

            // Stance state update.
            if (inputController.crouchInput > 0)
            {
                entityStateModel.stanceState = EntityStanceState.Crouched;
            }

            // Aiming overrides crouching.
            if (inputController.aimInput > 0)
            {
                entityStateModel.stanceState = EntityStanceState.Aiming;
            }
            else
            {
                entityStateModel.stanceState = EntityStanceState.Upright;
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
                entityStateModel.firingState = EntityFiringState.Firing;
                timeSinceLastFired = 0;
            }
            timeSinceLastFired += Time.deltaTime;
            if (timeSinceLastFired > firingTimeStanceDelay)
            {
                entityStateModel.firingState = EntityFiringState.NotFiring;
            }

            // Climbing        
            dectectClimbableRay = new Ray(transform.position, transform.forward);
            if (entityStateModel.groundedState != EntityGroundedState.Climbing
                && Physics.Raycast(dectectClimbableRay, out hitInfo, 0.8f)
                && hitInfo.collider.gameObject.tag == "Climbable"
                && inputController.movementInput.y > 0)
            {
                entityStateModel.groundedState = EntityGroundedState.Climbing;
            }


            // Animator updates.
            if (inputController.movementInput.sqrMagnitude > 0 && entityStateModel.stanceState != EntityStanceState.Aiming && entityStateModel.firingState != EntityFiringState.Firing)
            {
                animator.SetFloat("MovementY", 1.0f, 10.1f, 3.5f);
                animator.SetFloat("MovementX", 0.0f, 10.1f, 3.5f);
                animator.SetBool("isRunning", true);
            }
            if (inputController.movementInput.sqrMagnitude > 0 && (entityStateModel.stanceState == EntityStanceState.Aiming || entityStateModel.firingState == EntityFiringState.Firing))
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

        public bool IsAiming()
        {
            return entityStateModel.stanceState == EntityStanceState.Aiming;
        }

        public bool IsFiring()
        {
            return entityStateModel.firingState == EntityFiringState.Firing;
        }

        public bool IsSprinting()
        {
            return entityStateModel.movementState == EntityMovementState.Sprinting;
        }

        public bool IsClimbing()
        {
            return entityStateModel.groundedState == EntityGroundedState.Climbing;
        }

        public bool IsFalling()
        {
            return entityStateModel.groundedState != EntityGroundedState.Falling;
        }

        public bool CanMove()
        {
            return entityStateModel.aliveState == EntityAliveState.Alive;
        }

        public void Respawn()
        {
            transform.position = new Vector3(0, 0, 0);
            statsManager.currentHealth = statsManager.maxHealth;
            statsManager.currentShield = statsManager.maxShield;
            entityStateModel.aliveState = EntityAliveState.Alive;
        }


    }
}

