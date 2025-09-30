using UnityEngine;

public class FovUpdates : MonoBehaviour
{

    [Header("Camera FOVS")]
    public float normalFOV = 60f;       // Default FOV
    public float sprintFOV = 80f;       // Sprinting FOV
    public float crouchFOV = 55f;       // Crouching FOV
    public float slideFOV = 90f;        // Sliding FOV
    public float dashFOV = 80f;          // temporary FOV while dashing
    [Range(1, 20)] public float fovChangeSpeed = 10f; // Smoothing speed
    public float targetFOV;
    public float colChangeCamSpeed = 5f;

    public CameraMovement CM;
    public SniperRifle SniperRifle;
    void Start()
    {
        CM = GetComponent<CameraMovement>();
        
    }

    void Update()
    {
        
    }

    public void UpdateFOV()
    {
        if(SniperRifle.isScoped)
        {
            return;

        } else if (CM.playerCamera.fieldOfView != targetFOV)
            {
                // Smoothly transition to target FOV
                 CM.playerCamera.fieldOfView = Mathf.Lerp(CM.playerCamera.fieldOfView,targetFOV,fovChangeSpeed * Time.deltaTime);
            }
    }


}
