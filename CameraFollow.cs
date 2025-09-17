using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Rigidbody targetRb;   // Player Rigidbody to follow

    [Header("Settings")]
    public float sensitivity = 2.0f; // Mouse sensitivity
    public float followSmooth = 15f; // Position follow speed

    private float verticalX = 0;
    private float verticalY = 0;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetRb.interpolation = RigidbodyInterpolation.Interpolate; // smooth physics
    }

    private void LateUpdate()
    {
        if (targetRb == null) return;

        Vector3 targetPos = targetRb.position;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSmooth * Time.deltaTime);

        // Mouse look
        verticalX += Input.GetAxis("Mouse X") * sensitivity;
        verticalY += Input.GetAxis("Mouse Y") * sensitivity;
        verticalY = Mathf.Clamp(verticalY, -90f, 90f);

        transform.rotation = Quaternion.Euler(-verticalY, verticalX, 0f);
    }
}