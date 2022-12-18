using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowTarget : MonoBehaviour
{
    public float rotationPower;
    public GameObject player;

    private Vector2 input;

    void Update()
    {
        // Horizontal camera rotation.
        transform.rotation *= Quaternion.AngleAxis(input.x * rotationPower, Vector3.up);

        // Vertical camera rotation.
        transform.rotation *= Quaternion.AngleAxis(-1.0f * input.y * rotationPower, Vector3.right);

        // Clamp vertical rotation.
        var angles = transform.localEulerAngles;
        angles.z = 0;
        var angle = transform.localEulerAngles.x;
        if (angle > 180 && angle < 290)
        {
            angles.x = 290;
        }
        else if (angle < 180 && angle > 50)
        {
            angles.x = 50;
        }
        transform.localEulerAngles = angles;

        transform.position = player.transform.position + new Vector3(0, 1.56f, 0);
        // Set player rotation.
        // transform.parent.transform.rotation = Quaternion.Euler(0, -1.0f * transform.rotation.eulerAngles.y, 0);
        // transform.localEulerAngles = new Vector3(angles.x, 0, 0);
    }

    // Processes input callback to rotate the camera.
    public void OnLookInputCallback(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }
}
