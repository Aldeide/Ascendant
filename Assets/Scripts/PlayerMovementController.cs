using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    private Vector2 movementInput = new Vector2();
    private Vector2 lookInput = new Vector2();

    private Vector3 forward = new Vector3();
    private Vector3 right = new Vector3();

    public GameObject head;
    public GameObject weapon;

    public float playerSpeed = 3.0f;

    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Translate.
        forward = Camera.main.transform.forward;
        right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;


        this.transform.position += forward * Time.deltaTime * movementInput.y * playerSpeed;
        this.transform.position += right * Time.deltaTime * movementInput.x * playerSpeed;

        // Rotate.
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        head.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        //weapon.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        this.transform.Rotate(Vector3.up * lookInput.x);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        this.movementInput = context.ReadValue<Vector2>();
        Debug.Log(movementInput);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        this.lookInput = context.ReadValue<Vector2>();
        Debug.Log(lookInput);
    }
}
