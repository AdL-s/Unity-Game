using UnityEngine;

public class GunCamera : MonoBehaviour
{
    public Camera playerCamera;
    public Camera gunCamera;
    public SniperRifle SR;
    // Find this
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (SR.isActiveAndEnabled)
        {
            CameraChange();
        }
    }
    private void CameraChange()
    {
        gunCamera.fieldOfView = playerCamera.fieldOfView;
    }
}
