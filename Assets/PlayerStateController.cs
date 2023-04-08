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
    [RequireComponent(typeof(EntityStateModel))]
    public class PlayerStateController : MonoBehaviour
    {
        private Animator animator;
        private PlayerMovementController movementController;
        private PlayerInputController inputController;
        private PlayerStatsController statsController;

        public EntityStateModel entityStateModel { get; set; }

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
            statsController = GetComponent<PlayerStatsController>();

            // Initialise state model.
            entityStateModel = GetComponent<EntityStateModel>();
        }

        void Update()
        {
            if (!GameManager.Instance.IsLocalPlayer(this.gameObject)) return;

            entityStateModel.position = this.transform.position;
            entityStateModel.rotation = this.transform.rotation;
            entityStateModel.aimPoint = GameObject.Find("Target").transform.position;

            // Death State.
            if (statsController.GetHealth() <= 0)
            {
                if (entityStateModel.aliveState == EntityAliveState.Alive)
                {
                    Debug.Log("Died");
                    entityStateModel.timeOfDeath = Time.time;
                    //animator.SetBool("isDead", true);
                    //animator.SetTrigger("died");
                    entityStateModel.aliveState = EntityAliveState.Dead;
                }
            } else
            {
                entityStateModel.aliveState = EntityAliveState.Alive;
                //animator.SetBool("isDead", false);
            }
            if (entityStateModel.aliveState == EntityAliveState.Dead && Time.time > entityStateModel.timeOfDeath + respawnDelay)
            {
                Respawn();
            }

            if (movementController.IsGrounded() || FallHeight() < 0.4)
            {
                entityStateModel.groundedState = EntityGroundedState.Grounded;
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
            return entityStateModel.ToNetworkedState();
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
            statsController.RestoreAll();
            entityStateModel.aliveState = EntityAliveState.Alive;
        }

        public void Jump()
        {
            entityStateModel.groundedState = EntityGroundedState.Jumping;
        }

        public void SetDirection(Vector3 direction)
        {
            entityStateModel.direction = direction;
        }

    }
}

