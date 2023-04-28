using Ascendant.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Controllers
{

    // Controls the animator component based on the entity's state.
    [RequireComponent(typeof(EntityStateModel))]
    [RequireComponent(typeof(Animator))]
    public class EntityAnimatorController : MonoBehaviour
    {
        public Animator animator;
        public EntityStateModel stateModel;

        void Start()
        {
            animator = GetComponent<Animator>();
            stateModel = GetComponent<EntityStateModel>();
        }

        // Update is called once per frame
        void Update()
        {
            // If the entity just died trigger the death animation.
            if (animator.GetBool("isDead") == false && stateModel.aliveState == EntityAliveState.Dead)
            {
                animator.SetTrigger("died");
                animator.SetBool("isDead", true);
                return;
            }
            // If the entity is dead, return early.
            if (stateModel.aliveState == EntityAliveState.Dead)
            {
                return;
            } else
            {
                animator.SetBool("isDead", false);
            }

            // Movement animator updates.
            if (stateModel.IsMoving() && stateModel.stanceState != EntityStanceState.Aiming && stateModel.firingState != EntityFiringState.Firing)
            {
                animator.SetFloat("MovementY", 1.0f, 10.1f, 3.5f);
                animator.SetFloat("MovementX", 0.0f, 10.1f, 3.5f);
                animator.SetBool("isRunning", true);
            }
            if (stateModel.IsMoving() && (stateModel.stanceState == EntityStanceState.Aiming || stateModel.firingState == EntityFiringState.Firing))
            {
                animator.SetFloat("MovementX", stateModel.direction.x, 10.1f, 3.5f);
                animator.SetFloat("MovementY", stateModel.direction.y, 10.1f, 3.5f);
                animator.SetBool("isRunning", true);
            }
            if (!stateModel.IsMoving())
            {
                animator.SetBool("isRunning", false);
            }
            // Jump
            if (stateModel.groundedState == EntityGroundedState.Jumping)
            {
                animator.SetBool("IsJumping", true);
            } else
            {
                animator.SetBool("IsJumping", false);
            }

            // Falling
            if (!stateModel.IsGrounded())
            {
                animator.SetBool("IsFalling", true);
                animator.SetBool("IsGrounded", false);
            }
            if (stateModel.IsGrounded())
            {
                animator.SetBool("IsFalling", false);
                animator.SetBool("IsGrounded", true);
            }

        }
    }
}

