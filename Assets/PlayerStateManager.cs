using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    Falling
}

public enum PlayerFiringState
{
    NotFiring,
    Firing
}


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerStateManager : MonoBehaviour
{
    public Vector2 movementInput = new Vector2();
    public Vector2 lookInput = new Vector2();
    public float sprintInput = 0f;
    public float crouchInput = 0f;
    public float aimInput = 0f;
    public float fireInput = 0f;

    private Animator animator;
    private PlayerMovementController movementController;

    public PlayerStanceState stanceState;
    public PlayerMovementState movementState;
    public PlayerFiringState firingState;

    public float timeSinceLastFired;
    public float firingTimeStanceDelay = 1.2f;

    void Start()
    {
        // Fetch required components.
        animator = GetComponent<Animator>();
        movementController = GetComponent<PlayerMovementController>();

        // Initialise variables.
        stanceState = PlayerStanceState.Upright;
        movementState = PlayerMovementState.Idle;
    }

    void Update()
    {
        // Movement state update.
        if (movementInput.sqrMagnitude > 0)
        {
            movementState = PlayerMovementState.Running;
            if (sprintInput > 0)
            {
                movementState = PlayerMovementState.Sprinting;
            }
        } else
        {
            movementState = PlayerMovementState.Idle;
        }

        // Stance state update.
        if (crouchInput > 0)
        {
            stanceState = PlayerStanceState.Crouched;
        } 

        // Aiming overrides crouching.
        if (aimInput > 0)
        {
            stanceState = PlayerStanceState.Aiming;
        } else
        {
            stanceState = PlayerStanceState.Upright;
        }

        // Jumping.
        if (movementController.isGrounded)
        {

        }

        // Firing
        if (fireInput > 0)
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

        // Animator updates.
        if (movementInput.sqrMagnitude > 0 && stanceState != PlayerStanceState.Aiming && firingState != PlayerFiringState.Firing)
        {
            animator.SetFloat("MovementY", 1.0f, 0.1f, 0.5f);
            animator.SetFloat("MovementX", 0.0f, 0.1f, 0.5f);
            animator.SetBool("isRunning", true);
            return;
        }
        if (movementInput.sqrMagnitude > 0 && (stanceState == PlayerStanceState.Aiming || firingState == PlayerFiringState.Firing))
        {
            animator.SetFloat("MovementX", movementInput.x, 0.1f, 0.5f);
            animator.SetFloat("MovementY", movementInput.y, 0.1f, 0.5f);
            animator.SetBool("isRunning", true);
            return;
        }
        animator.SetBool("isRunning", false);
    }


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
        if (!context.started) return;
        if (!movementController.isGrounded) return;
        // TODO.
    }
    public void OnFireCallBack(InputAction.CallbackContext context)
    {
        fireInput = context.ReadValue<float>();
    }

}
