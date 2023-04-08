using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ascendant.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerMovementController : MonoBehaviour
    {
        public LayerMask layerMask;
        public GameObject followTarget;
        public float rotationPower = 0.01f;

        // Required components.
        CharacterController characterController;
        PlayerInputController inputController;

        private Vector3 forward = new Vector3();
        private Vector3 right = new Vector3();
        private Vector3 up = new Vector3(0, 1, 0);

        public GameObject head;
        public GameObject weapon;

        public Vector3 direction = new Vector3();
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

        private PlayerStateController stateController;


        void Start()
        {
            currentSpeed = 0f;
            stateController = GetComponent<PlayerStateController>();
            Cursor.lockState = CursorLockMode.Locked;
            characterController = GetComponent<CharacterController>();
            followTarget = GameObject.Find("FollowTarget");
            inputController = GetComponent<PlayerInputController>();
        }

        void Update()
        {
            if (!GameManager.Instance.IsLocalPlayer(this.gameObject)) return;
            //if (IsGrounded()) { stateManager.GroundedState = PlayerGroundedState.Grounded; }

            ComputeSpeed();
            forward = Camera.main.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            right = Camera.main.transform.right;
            right.y = 0f;
            right.Normalize();

            direction = forward * inputController.movementInput.y + right * inputController.movementInput.x;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            var currentAngle = transform.rotation.eulerAngles.y;
            var currentAngleVelocity = 0f;

            if (Vector3.Angle(transform.forward, direction) > 160)
            {
                // Debug.Log(Vector3.Angle(transform.forward, direction));
                // Debug.Log("Direction Switch!");
                //TODO: animate 180 degrees turns.
            }
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, 0.04f);

            // Performing the player rotation.
            if (inputController.movementInput.sqrMagnitude > 0)
            {
                if (!stateController.IsAiming() && !stateController.IsFiring())
                {
                    transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                }
                else
                {
                    Vector3 lookAtTest = this.transform.position + forward;
                    lookAtTest.y = this.transform.position.y;
                    this.transform.LookAt(lookAtTest);
                }
            }
            else if (stateController.IsAiming() || stateController.IsFiring())
            {
                Vector3 lookAtvector = this.transform.position + forward;
                lookAtvector.y = this.transform.position.y;
                this.transform.LookAt(lookAtvector);
            }

            if (stateController.IsClimbing())
            {
                direction = direction = up * inputController.movementInput.y + right * inputController.movementInput.x;
            }
            else
            {
                Jump();
                Gravity();
            }

            // Applying speed and perfoming the movement.
            direction.x *= currentSpeed;
            direction.z *= currentSpeed;
            direction.y *= 6.0f;

            if (stateController.CanMove())
            {
                characterController.Move(direction * Time.deltaTime);
            }

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
            if (inputController.movementInput.magnitude == 0)
            {
                currentSpeed = 2.0f;
            }
            if (inputController.crouchInput > 0 && IsGrounded() && currentSpeed < 3.0f)
            {
                currentSpeed = maxCrouchingSpeed;
            }
            if (inputController.sprintInput > 0)
            {
                currentSpeed += Time.deltaTime * sprintAcceleration;
                if (currentSpeed > maxSprintingSpeed)
                {
                    currentSpeed = maxSprintingSpeed;
                }
                return;
            }
            if (inputController.movementInput.magnitude > 0.1f)
            {
                currentSpeed += Time.deltaTime * sprintAcceleration;
                if (currentSpeed > maxRunningSpeed)
                {
                    currentSpeed = maxRunningSpeed;
                }
                return;
            }

        }

        private void Jump()
        {
            if (!IsGrounded()) return;
            if (inputController.jumpInput == 0) return;
            verticalVelocity += 2.2f;
            //stateManager.GroundedState = PlayerGroundedState.Jumping;
        }

        private void JumpOutOfClimbable()
        {
            verticalVelocity = 0;
            //stateManager.GroundedState = PlayerGroundedState.Falling;
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

        private void CameraRotation()
        {
            // Horizontal Camera Rotation.
            followTarget.transform.rotation *= Quaternion.AngleAxis(inputController.lookInput.x * rotationPower, Vector3.up);
            followTarget.transform.rotation *= Quaternion.AngleAxis(inputController.lookInput.y * rotationPower, -1.0f * Vector3.right);

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

    }
}


