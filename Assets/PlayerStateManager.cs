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
    Falling,
    Climbing
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
    public float jumpInput = 0f;

    private Animator animator;
    private PlayerMovementController movementController;

    public PlayerStanceState stanceState;
    public PlayerMovementState movementState;
    private PlayerGroundedState groundedState;
    public PlayerGroundedState GroundedState 
        { 
            get => groundedState;
            set { 
                if(groundedState != value) {
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
            //Not working
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

        // Climbing        
        dectectClimbableRay = new Ray(transform.position,transform.forward);
        if(GroundedState != PlayerGroundedState.Climbing 
            && Physics.Raycast(dectectClimbableRay, out hitInfo,0.8f) 
            && hitInfo.collider.gameObject.tag == "Climbable"
            && movementInput.y > 0){
                GroundedState = PlayerGroundedState.Climbing;
        }
        

        // Animator updates.
        if (movementInput.sqrMagnitude > 0 && stanceState != PlayerStanceState.Aiming && firingState != PlayerFiringState.Firing)
        {
            animator.SetFloat("MovementY", 1.0f, 10.1f, 3.5f);
            animator.SetFloat("MovementX", 0.0f, 10.1f, 3.5f);
            animator.SetBool("isRunning", true);
        }
        if (movementInput.sqrMagnitude > 0 && (stanceState == PlayerStanceState.Aiming || firingState == PlayerFiringState.Firing))
        {
            animator.SetFloat("MovementX", movementInput.x, 10.1f, 3.5f);
            animator.SetFloat("MovementY", movementInput.y, 10.1f, 3.5f);
            animator.SetBool("isRunning", true);
        }
        if (movementInput.sqrMagnitude < 0.01)
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
        //if (!context.started) return;
        //if (!movementController.IsGrounded()) return;
        jumpInput = context.ReadValue<float>();
        if (context.ReadValue<float>() > 0 && movementController.IsGrounded())
        {
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsGrounded", false);
        } else
        {
            animator.SetBool("IsJumping", false);
        }
    }
    public void OnFireCallBack(InputAction.CallbackContext context)
    {
        fireInput = context.ReadValue<float>();
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
        if (Physics.Raycast(ray, out hit, 100.0f, layerMask, QueryTriggerInteraction.Ignore)) {
            // Debug.Log(hit.distance);
            return hit.distance;
        }
        return 0;
    }

}
