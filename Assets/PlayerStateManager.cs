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
    private PlayerMovementController movementController;
    void Start()
    {
        animator = GetComponent<Animator>();
        movementController = GetComponent<PlayerMovementController>();
    }

    void Update()
    {
        Vector3 heading = movementController.transform.forward;
        Vector2 direction = new Vector2(0, 0);

        if (movementInput.x > 0) // d
        {
            direction += new Vector2(-1, -1);
        }
        if (movementInput.x < 0) // q
        {
            direction += new Vector2(1, 1);
        }
        if (movementInput.y > 0) // z
        {
            direction += new Vector2(1, -1);
        }
        if (movementInput.y < 0) // s
        {
            direction += new Vector2(-1, 1);
        }

        Debug.DrawLine(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(transform.position.x + direction.x, 0, transform.position.z + direction.y), Color.yellow);

        Debug.DrawLine(
            new Vector3(0, 0, 0),
            new Vector3(direction.x, 0, direction.y), Color.red);

        Debug.DrawLine(
            transform.position,
            transform.position + heading, Color.cyan);

        float alpha = SignedAngle(new Vector3(direction.x, 0, direction.y), heading);
        //Debug.Log(direction);
        if (alpha < 0)
        {
            alpha *= -1;
        } else
        {
            alpha = 360 - alpha;
        }
        float alphaRadians = Mathf.Deg2Rad * alpha;
        Vector2 animationDirection = movementInput;
        if (direction.x != 0 || direction.y != 0)
        {
            animationDirection = new Vector2(Mathf.Sin(alphaRadians), Mathf.Cos(alphaRadians));
        }

        heading.y = 0;
        if (movementInput.sqrMagnitude > 0)
        {
            animator.SetFloat("MovementX", animationDirection.x, 0.2f, 1.6f);
            animator.SetFloat("MovementY", animationDirection.y, 0.2f, 1.6f);
            animator.SetBool("isRunning", true);
        } else
        {
            animator.SetBool("isRunning", false);
        }
    }

    private float SignedAngle(Vector3 a, Vector3 b)
    {
        return Vector3.Angle(a, b) * Mathf.Sign(Vector3.Cross(a, b).y);
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
