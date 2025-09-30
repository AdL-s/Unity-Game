using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
       transform.localPosition = new Vector3(0, 0.6f, 0);
    }
    private void FixedUpdate()
      {

        if (target == null) return;
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}
