using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ascendant.Networking
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementController : MonoBehaviour
    {
        public LayerMask layerMask;
        public bool thirdPersonController = true;
        public GameObject followTarget;
        public float rotationPower = 0.01f;

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
        private Vector3 up = new Vector3(0, 1, 0);

        public GameObject head;
        public GameObject weapon;

        public Vector3 direction = new Vector3();
        public Vector3 localDirection = new Vector3();
        // Velocity.
        public float playerSpeed = 5.0f;

        [SerializeField]
        public float maxPlayerSpeed = 7.0f;
        [SerializeField]
        public float maxRunningSpeed = 3.9f;
        [SerializeField]
        public float maxSprintingSpeed = 5.7f;
        [SerializeField]
        public float maxCrouchingSpeed = 5.0f;

        public float currentSpeed;
        public float verticalVelocity;
        public float slideAddedVelocity;

        public float sprintAcceleration = 3.2f;

        float xRotation = 0f;
        float yRotation = 0f;

        private NetworkedPlayerStateManager stateManager;


        void Start()
        {
            currentSpeed = 0f;
            stateManager = GetComponent<NetworkedPlayerStateManager>();
            Cursor.lockState = CursorLockMode.Locked;
            characterController = GetComponent<CharacterController>();
            followTarget = GameObject.Find("FollowTarget");

        }

        void FixedUpdate()
        {
            if (IsGrounded()) { stateManager.GroundedState = PlayerGroundedState.Grounded; }

            ComputeSpeed();
            forward = Camera.main.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            right = Camera.main.transform.right;
            right.y = 0f;
            right.Normalize();

            direction = forward * movementInput.y + right * movementInput.x;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            var currentAngle = transform.rotation.eulerAngles.y;
            var currentAngleVelocity = 0f;

            if (Vector3.Angle(transform.forward, direction) > 160)
            {
                //TODO: animate 180 degrees turns.
            }
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, 0.04f);

            // Performing the player rotation.
            if (movementInput.sqrMagnitude > 0)
            {
                if (stateManager.stanceState != PlayerStanceState.Aiming && stateManager.firingState != PlayerFiringState.Firing)
                {
                    transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                }
                else
                {
                    Vector3 lookAtTest = this.transform.position + forward;
                    lookAtTest.y = this.transform.position.y;

                    this.transform.LookAt(lookAtTest);
                    /*
                    targetAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
                    currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, 0.04f);
                    transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                    */
                }
            }
            else if (stateManager.stanceState == PlayerStanceState.Aiming || stateManager.firingState == PlayerFiringState.Firing)
            {
                Vector3 lookAtTest = this.transform.position + forward;
                lookAtTest.y = this.transform.position.y;

                this.transform.LookAt(lookAtTest);
                /*
                var targetAngle2 = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

                if (targetAngle2 < 0)
                {
                    targetAngle2 = 360 + targetAngle2;
                }
                Debug.Log(targetAngle2);
                currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle2, ref currentAngleVelocity, 0.02f);
                transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                */
            }

            if (stateManager.GroundedState == PlayerGroundedState.Climbing)
            {
                direction = direction = up * movementInput.y + right * movementInput.x;
            }
            else
            {
                Gravity();
            }

            // Applying speed and perfoming the movement.
            direction.x *= currentSpeed;
            direction.z *= currentSpeed;
            direction.y *= 6.0f;
            characterController.Move(direction * Time.deltaTime);

            // Debug lines.
            Debug.DrawLine(transform.position, transform.position + forward, Color.red);
            Debug.DrawLine(transform.position, transform.position + right, Color.yellow);
            Debug.DrawLine(transform.position, transform.position + direction, Color.blue);

            CameraRotation();
        }

        private void ComputeSpeed()
        {
            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }
            if (movementInput.magnitude == 0)
            {
                currentSpeed = 2.0f;
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
            verticalVelocity += 2.2f;
            //stateManager.GroundedState = PlayerGroundedState.Jumping;
        }

        private void JumpOutOfClimbable()
        {
            verticalVelocity = 0;
            stateManager.GroundedState = PlayerGroundedState.Falling;
        }

        private void Gravity()
        {
            if (IsGrounded() && verticalVelocity < 0f)
            {
                verticalVelocity = -0.2f;
            }
            else
            {
                verticalVelocity += -10 * 0.81f * Time.deltaTime;
            }
            direction.y = verticalVelocity;
        }

        public bool IsGrounded() => characterController.isGrounded;

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

        private void CameraRotation()
        {
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
            /*
            if (!context.started) return;
            switch (stateManager.GroundedState)
            {
                case PlayerGroundedState.Grounded:
                    Jump();
                    break;
                case PlayerGroundedState.Climbing:
                    JumpOutOfClimbable();
                    break;
            }
            */
            //this.jumpInput = context.ReadValue<float>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            this.sprintInput = context.ReadValue<float>();
            if (isSprinting)
            {
                isSprinting = false;
                return;
            }
            if (!isSprinting) isSprinting = true;
            if (!isSprinting) isSprinting = true;
        }

        #endregion
    }
}


