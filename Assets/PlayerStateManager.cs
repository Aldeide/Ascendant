using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerStateManager : MonoBehaviour
{
    public Vector2 movementInput = new Vector2();
    public Vector2 lookInput = new Vector2();

    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (movementInput.sqrMagnitude > 0)
        {
            animator.SetFloat("MovementX", movementInput.x, 1.6f, 1.6f);
            animator.SetFloat("MovementY", movementInput.y, 1.6f, 1.6f);
            animator.SetBool("isRunning", true);
        } else
        {
            animator.SetBool("isRunning", false);
        }
    }

    public void OnMoveCallback(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnLookCallback(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}
