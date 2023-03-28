using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    public LayerMask layerMask;


    // Inputs variables.
    private Vector2 movementInput = new Vector2();
    private Vector2 lookInput = new Vector2();
    public float crouchInput;
    private float jumpInput;
    private float sprintInput;

    // Movement Toggle.
    public bool isSprinting;
    public bool isSliding;
    public bool isGrounded;

    // Character controller.
    CharacterController characterController;

    private Vector3 forward = new Vector3();
    private Vector3 right = new Vector3();

    public GameObject head;
    public GameObject weapon;

    public Vector3 direction =  new Vector3();
    public Vector3 localDirection = new Vector3();
    // Velocity.
    public float playerSpeed = 5.0f;

    public float maxPlayerSpeed = 7.0f;
    public float maxRunningSpeed = 3.9f;
    public float maxSprintingSpeed = 5.7f;
    public float maxCrouchingSpeed = 5.0f;

    public float currentSpeed;
    public float verticalVelocity;
    public float slideAddedVelocity;

    public float sprintAcceleration = 3.2f;

    float xRotation = 0f;
    float yRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        currentSpeed = 0f;
        //Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = IsGrounded();
        GetSlideDirection();
        ComputeSpeed();

        forward = Camera.main.transform.forward;
        right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;

        right = new Vector3(-1, 0, -1);
        forward = new Vector3(1, 0, -1);
        float lol = GetSlopeValue();

        if (crouchInput <= 0)
        {
            isSliding = false;
        }

        if (crouchInput > 0)
        {
            characterController.height = 1.2f;
        }
        else
        {
            characterController.height = 2.0f;
        }
        if (currentSpeed > 3.0f && crouchInput > 0)
        {
            isSliding = true;
            // Slide.
            //direction = (Vector3.Dot(GetSlideDirection(), right) / 10.0f * right + forward).normalized * currentSpeed * Time.deltaTime;
            //characterController.Move(forward.normalized * currentSpeed * Time.deltaTime);
        }

        // Translate.


        // If the character looks directly down or up, movement will be nil, taking the player's transform as the forward direction instead.
        if (Mathf.Abs(forward.magnitude) <= 0.01f)
        {
            //forward = transform.forward;
            //right = transform.right;
        }

        if (!isSliding)
        {
            localDirection = (right.normalized * movementInput.x + forward.normalized * movementInput.y).normalized;
        }

            direction.x = localDirection.x * currentSpeed * Time.deltaTime;
            direction.z = localDirection.z * currentSpeed * Time.deltaTime;


        // Applying gravity.
        Gravity();

        //this.transform.position += forward.normalized * Time.deltaTime * movementInput.y * playerSpeed;
        //this.transform.position += right.normalized * Time.deltaTime * movementInput.x * playerSpeed;

        // Rotate.
        Rotate();

        characterController.Move(direction);
    }

    private void ComputeSpeed()
    {
        if (currentSpeed < 0)
        {
            currentSpeed = 0;
        }
        if (isSliding)
        {
            float slope = GetSlopeValue();
            if (slope > 25)
            {
                currentSpeed += 7.0f * Time.deltaTime;
            } else
            {
                currentSpeed -= 2.8f * Time.deltaTime;
                if (currentSpeed < maxCrouchingSpeed)
                {
                    currentSpeed = maxCrouchingSpeed;
                    isSliding = false;
                }
            }
            return;
        }
        if (movementInput.magnitude == 0)
        {
            currentSpeed = 0;
            isSprinting = false;
        }
        if (crouchInput > 0 && !isSliding && IsGrounded() && currentSpeed < 3.0f)
        {
            currentSpeed = maxCrouchingSpeed;
        }
        if (isSprinting)
        {
            currentSpeed += Time.deltaTime * sprintAcceleration;
            if (currentSpeed > maxSprintingSpeed)
            {
                currentSpeed = maxSprintingSpeed;
            }
            return;
        }
        if (movementInput.magnitude > 0.1f)
        {
            currentSpeed += Time.deltaTime * sprintAcceleration;
            if (currentSpeed > maxRunningSpeed)
            {
                currentSpeed = maxRunningSpeed;
            }
            return;
        }

    }

    private void Rotate()
    {
        //xRotation -= lookInput.y;
        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //yRotation += lookInput.x;
        //head.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Match body to head.
        //Vector3 bodyRotation = new Vector3(0, head.transform.rotation.eulerAngles.y, 0);
        //transform.localRotation = Quaternion.Euler(0, yRotation, 0);

        // We create a plane perpendicular to the player's axis and at its feet.
        Plane plane = new Plane(new Vector3(0, 1, 0), transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter = 0;
        plane.Raycast(ray, out enter);
        Vector3 hit = ray.GetPoint(enter);

        transform.LookAt(hit);

    }

    private void Jump()
    {
        verticalVelocity += 0.4f;
    }

    private void Gravity()
    {
        if (IsGrounded() && verticalVelocity < 0f)
        {
            verticalVelocity = -0.2f;
        } else
        {
            verticalVelocity += -0.81f * Time.deltaTime;
        }
        direction.y = verticalVelocity;
    }

    private bool IsGrounded() => characterController.isGrounded;

    private Vector3 GetSlideDirection()
    {
        int rayCount = 16;
        float angle = 0;
        Vector3 steepest = new Vector3(0, 0, 0);
        float maxDistance = 0;
        for (int i = 0; i < rayCount; i++)
        {
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            angle += 2 * Mathf.PI / rayCount;
            Vector3 direction = new Vector3(transform.position.x + x, transform.position.y + 1.0f, transform.position.z + y); ;
            RaycastHit hit;
            Ray ray = new Ray(direction, -Vector3.up);
            //Debug.DrawLine(transform.position, direction, Color.red);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == null) continue;
                float distance = (hit.point - direction).magnitude;
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    steepest = hit.point;
                }
            }
            
        }
        //Debug.DrawLine(transform.position, steepest, Color.yellow);
        return steepest;
    }

    private float GetSlopeValue()
    {
        int rayCount = 16;
        float angle = (transform.eulerAngles.y - 22.5f) * (Mathf.PI) / 180;
        Vector3 steepest = new Vector3(0, 0, 0);
        float distanceSum = 0;
        int numHits = 0;
        for (int i = 0; i < rayCount; i++)
        {
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            angle += Mathf.PI / 4 / rayCount;
            Vector3 direction = new Vector3(transform.position.x + x, transform.position.y + 1.0f, transform.position.z + y);
            RaycastHit hit;
            Ray ray = new Ray(direction, -Vector3.up);
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == null) continue;
                float ang = Vector3.Angle(hit.normal, Vector3.up);
                //Debug.Log(ang);
                if (Vector3.Dot(hit.normal, forward) < 0)
                {
                    ang *= -1;
                }
                distanceSum += ang;
                numHits++;
            }

        }
        //Debug.DrawLine(transform.position, steepest, Color.yellow);
        //Debug.Log(distanceSum / numHits);
        return distanceSum / numHits;
    }

    #region Input Callbacks.
    public void OnMove(InputAction.CallbackContext context)
    {
        this.movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        this.lookInput = context.ReadValue<Vector2>();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        this.crouchInput = context.ReadValue<float>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!IsGrounded()) return;
        Jump();
        //this.jumpInput = context.ReadValue<float>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        this.sprintInput = context.ReadValue<float>();
        if (isSprinting) {
            isSprinting = false;
            return;
        }
        if (!isSprinting) isSprinting = true;
    }
    #endregion
}
