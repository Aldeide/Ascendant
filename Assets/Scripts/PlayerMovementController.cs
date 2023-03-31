using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    public LayerMask layerMask;

    public bool thirdPersonController = true;

    public GameObject followTarget;

    public float rotationPower = 0.25f;

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

    // Cameras.
    public GameObject defaultCamera;
    public GameObject aimCamera;
    public GameObject sprintCamera;


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

    private PlayerStateManager stateManager;


    void Start()
    {
        currentSpeed = 0f;
        stateManager = GetComponent<PlayerStateManager>();
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        followTarget = GameObject.Find("FollowTarget");
    }

    // Update is called once per frame
    void Update()
    {
        ComputeSpeed();
        forward = Camera.main.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        right = Camera.main.transform.right;
        right.y = 0f;
        right.Normalize();
        if (movementInput.magnitude > 0)
        {
            

            direction = forward * movementInput.y + right * movementInput.x;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            var currentAngle = this.transform.rotation.eulerAngles.y;
            var currentAngleVelocity = 0f;

            if (Mathf.DeltaAngle(currentAngle, targetAngle) > 160)
            {
                Debug.Log("Direction Switch!");
            }
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, 0.05f);

            // Performing the rotation.
            if (stateManager.stanceState != PlayerStanceState.Aiming)
            {
                transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            } else
            {
                targetAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
                currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, 0.04f);
                transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            }
            

            //Debug.Log(direction);

            // Applying gravity.
            Gravity();

            // Perfoming the movement.
            characterController.Move(direction * Time.deltaTime * currentSpeed);

            // Debug lines.

            Debug.DrawLine(transform.position, transform.position + forward, Color.red);
            Debug.DrawLine(transform.position, transform.position + right, Color.yellow);

        }

        if (stateManager.stanceState == PlayerStanceState.Aiming)
        {
            var currentAngle = this.transform.rotation.eulerAngles.y;
            var targetAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            var currentAngleVelocity = 0f;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, 0.02f);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
        }

        // Horizontal Camera Rotation.
        followTarget.transform.rotation *= Quaternion.AngleAxis(lookInput.x * rotationPower, Vector3.up);
        followTarget.transform.rotation *= Quaternion.AngleAxis(lookInput.y * rotationPower, -1.0f * Vector3.right);



        // Vertical Camera Rotation
        var angles = followTarget.transform.localEulerAngles;
        angles.z = 0;
        var angle = followTarget.transform.localEulerAngles.x;

        if (angle > 180 && angle < 320)
        {
            angles.x = 320;
        }
        else if (angle < 180 && angle > 60)
        {
            angles.x = 60;
        }
        followTarget.transform.localEulerAngles = angles;
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
            verticalVelocity += -5 * 0.81f * Time.deltaTime;
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
                if (Vector3.Dot(hit.normal, forward) < 0)
                {
                    ang *= -1;
                }
                distanceSum += ang;
                numHits++;
            }

        }
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
        if (!isSprinting) isSprinting = true;
    }
    #endregion
}
