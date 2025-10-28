using System.Collections;
using UnityEngine;

public class SwayNBobScript : MonoBehaviour
{
    public CameraMovement mover;

    [Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    Vector3 swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot;
    public float smooth = 10f;
    float smoothRot = 12f;

    [Header("Bobbing")]
    public float speedCurve;
    float curveSin { get => Mathf.Sin(speedCurve); }
    float curveCos { get => Mathf.Cos(speedCurve); }
    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;
    Vector3 bobPosition;
    public float bobExaggeration;

    [Header("Idle Bob Settings")]
    public float idleBobAmount = 0.1f; // How much bob when standing still (0-1)
    public float idleBobSpeed = 1f; // Speed of idle breathing bob

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    Vector3 bobEulerRotation;

    [Header("Model Rotation Offset")]
    public Vector3 modelRotationOffset = Vector3.zero;
    private Quaternion baseRotation;

    [Header("Base Position & Rotation")]
    public Vector3 basePosition = Vector3.zero; // Set the gun's base position here
    public Vector3 baseRotationEuler = Vector3.zero; // This doesnt work use offset

    void Start()
    {
        baseRotation = Quaternion.Euler(modelRotationOffset);
    }

    void OnEnable()
    {
        // Reset all movement values to prevent drift
        swayPos = Vector3.zero;
        bobPosition = Vector3.zero;
        swayEulerRot = Vector3.zero;
        bobEulerRotation = Vector3.zero;
        speedCurve = 0f;
    }

    void Update()
    {
        // Add null check for mover
        if (mover == null) return;

        GetInput();
        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();
        CompositePositionRotation();
    }

    Vector2 walkInput;
    Vector2 lookInput;

    void GetInput()
    {
        walkInput.x = Input.GetAxis("Horizontal");
        walkInput.y = Input.GetAxis("Vertical");
        walkInput = walkInput.normalized;
        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
    }

    void Sway()
    {
        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);
        swayPos = invertLook;
    }

    void SwayRotation()
    {
        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
        swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    void CompositePositionRotation()
    {
        // Use base position from Inspector + sway and bob effects
        Vector3 targetPosition = basePosition + swayPos + bobPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smooth);

        // Use base rotation from Inspector + model offset + sway and bob
        Quaternion baseRot = Quaternion.Euler(baseRotationEuler);
        Quaternion targetRotation = baseRot * baseRotation * Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothRot);
    }

    void BobOffset()
    {
        // Check if player is moving
        bool isMoving = walkInput.magnitude > 0.01f;

        if (isMoving)
        {
            // Normal walking bob
            speedCurve += Time.deltaTime * (mover.grounded ? (Input.GetAxis("Horizontal") + Input.GetAxis("Vertical")) * bobExaggeration : 1f) + 0.01f;
            bobPosition.x = (curveCos * bobLimit.x * (mover.grounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
            bobPosition.y = (curveSin * bobLimit.y) - (Input.GetAxis("Vertical") * travelLimit.y);
            bobPosition.z = -(walkInput.y * travelLimit.z);
        }
        else
        {
            // Idle breathing bob (subtle)
            speedCurve += Time.deltaTime * idleBobSpeed;
            bobPosition.x = curveCos * bobLimit.x * idleBobAmount;
            bobPosition.y = curveSin * bobLimit.y * idleBobAmount;
            bobPosition.z = 0f;
        }
    }

    void BobRotation()
    {
        bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
        bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
    }
}