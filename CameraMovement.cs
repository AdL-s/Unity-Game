using UnityEngine;
using System;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    [Header("Constants")]
    const float defaultCrouchMaxMovement = 2f;
    const float defaultMaxMovement = 5;
    const float defaultCounterMaxMovement = 0.275f;
    const float defaultMovement = 1;

    [Header("Sensitivity")]
    public float sensitivity = 2.0f;
    float verticalX = 0;
    float verticalY = 0;

    public Camera playerCamera;
    public Rigidbody rb;

    [Header("Speed")]
    public float speedMultipl = 1;
    public float movementSpeed = 8f; // Reduced from 1 to prevent excessive speed
    public float maxVelocity = 2;

    [Header("Movement Smoothing")]
    public float acceleration = 12f;
    public float deceleration = 15f;
    public float airControl = 0.4f;
    private Vector3 moveInput = Vector3.zero;
    private Vector3 currentVelocity = Vector3.zero;

    [Header("Jump")]
    public float secondJumpMultForce = 1.25f;
    public float jumpForce = 50;

    [Header("Gravity")]
    public float gravityAcce = 0;
    private float gravityModifier = 1;
    private float airTimer = 0f;
    private float airCounter = 0.015f;

    [Header("Wall Detection")]
    public float wallCheckDistance = 0.6f;

    [Header("Slide")]
    public float slideStarter = 0.8f; //what percantage of max velocity you can start slide 
    public float slideCounter = 0.03f;
    public float slideForce = 500;
    private float sprintVelocityMultiplier = 1f;
    public float cooldown = 2f;

    public bool grounded;

    [Header("Abillity Cooldown")]
    private float lastUsedTime = -Mathf.Infinity;

    [Header("Colliders")]
    public Collider crouchingCollider;
    public Collider standingCollider;

    private bool invokeUncrouch = false;
    private float timer = 0;

    private bool doubleJump = true;
    private bool forceJump = false;

    [Header("Camera FOVS")]
    public float normalFOV = 60f;       // Default FOV
    public float sprintFOV = 80f;       // Sprinting FOV
    public float crouchFOV = 55f;       // Crouching FOV
    public float slideFOV = 90f;        // Sliding FOV
    public float dashFOV = 80f;          // temporary FOV while dashing
    [Range(1, 20)] public float fovChangeSpeed = 10f; // Smoothing speed
    private float targetFOV;            // The FOV we're moving toward

    [Header("Camera Leaning")]
    public float leanAngle = 4f;      // how much tilt (degrees)
    public float leanSpeed = 12f;       // how fast it tilts
    private float currentLean = 0f;    // current tilt

    [Header("Dash")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashForce = 15f;        // Reduced from 20f
    public float dashDuration = 0.15f;   // how long you're "in dash state"
    public float dashCooldown = 0.8f;    // time before you can dash again
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;

    private VFXPlayer vfxPlayer;

    private Vector3 moveDirection = Vector3.zero;

    [Header("layerMasks")]
    public LayerMask lm;

    // Input caching system
    private struct InputCache
    {
        public bool w, a, s, d;
        public bool jump;
        public bool crouch;
        public bool crouchUp;
        public bool dash;
        public Vector2 mouseInput;

        public void Cache()
        {
            w = Input.GetKey(KeyCode.W);
            a = Input.GetKey(KeyCode.A);
            s = Input.GetKey(KeyCode.S);
            d = Input.GetKey(KeyCode.D);
            jump = Input.GetKeyDown(KeyCode.Space);
            crouch = Input.GetKeyDown(KeyCode.LeftControl);
            crouchUp = Input.GetKeyUp(KeyCode.LeftControl);
            dash = Input.GetKeyDown(KeyCode.LeftShift);
            mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
    }

    private InputCache cachedInput;

    public enum PlayerStates { OnGround = 0, InAir = 1, Crouching = 2, Sliding = 3,  Dashing = 4 };
    public PlayerStates state;

    private void Start()
    {
        ToggleCursor();
        InitializacePlayer();
        vfxPlayer = GetComponent<VFXPlayer>();
        targetFOV = normalFOV;
        playerCamera.fieldOfView = normalFOV; // Apply immediately
    }

    private void Update()
    {
        // Cache input for FixedUpdate
        cachedInput.Cache();

        // Keep non-physics updates here
        UpdateByStateNonPhysics();
        UpdateFOV();
        AirTimer();
    }

    private void FixedUpdate()
    {
        // Handle all physics-based movement
        UpdateByStatePhysics();
    }

    private void UpdateFOV()
    {
        if (playerCamera.fieldOfView != targetFOV)
        {
            // Smoothly transition to target FOV
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                fovChangeSpeed * Time.deltaTime
            );
        }
    }

    private void Jump()
    {
        Debug.Log("Called");
        if (state == PlayerStates.InAir && !doubleJump)
        {
            StartCoroutine(ForcedJump());
            return;
        }

        if (cachedInput.jump || forceJump)
        {
            if (GetDistanceFromRoof() > 1.12f && state != PlayerStates.InAir && state != PlayerStates.Dashing)
            {
                Debug.Log("Jump");
                // Store current horizontal velocity
                Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                // Apply jump force (vertical only)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

                // Maintain horizontal momentum
                rb.AddForce(horizontalVelocity.normalized * (jumpForce * 0.3f), ForceMode.VelocityChange);

                vfxPlayer.PlayParticles();
                forceJump = false;
            }
            else if (state == PlayerStates.InAir && GetDistanceFromRoof() > 1.12f)
            {
                if (doubleJump)
                {
                    airCounter = 0;
                    airTimer = 0;

                    // Store current horizontal velocity
                    Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                    // Apply double jump force (vertical only)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                    rb.AddForce(Vector3.up * jumpForce * secondJumpMultForce, ForceMode.VelocityChange);

                    // Maintain horizontal momentum
                    rb.AddForce(horizontalVelocity.normalized * (jumpForce * 0.3f * secondJumpMultForce), ForceMode.VelocityChange);

                    doubleJump = false;
                    airCounter = 0.015f;
                }
            }
        }
    }

    private void Slide()
    {
        if (TryUseAbility())
        {
            rb.AddForce(rb.linearVelocity.normalized * slideForce * 0.7f, ForceMode.VelocityChange); // Reduced slide force
            SwitchState(PlayerStates.Sliding);
        }
        else
        {
            Crouch();
        }
    }

    private bool CheckForSlide()
    {
        if (rb.linearVelocity.magnitude > maxVelocity * slideStarter && cachedInput.crouch)
        {
            Slide();
            return true;
        }

        return false;
    }

    private void SlideCancel()
    {
        if (rb.linearVelocity.magnitude < maxVelocity * 0.15f)
        {
            SwitchState(PlayerStates.Crouching);
        }
    }

    private void CheckForUnCrouch()
    {
        if (invokeUncrouch && GetDistanceFromRoof() > 1.12f)
        {
            UnCrouch();
            return;
        }

        if (cachedInput.crouchUp)
        {
            if (GetDistanceFromRoof() > 1.12f)
            {
                UnCrouch();
            }
            else
            {
                invokeUncrouch = true;
            }
        }
    }

    private void CheckForCrouch()
    {
        if (cachedInput.crouch)
        {
            Crouch();
        }
    }

    private void LookAround()
    {
        //Mouse look
        verticalX += cachedInput.mouseInput.x * sensitivity;
        verticalY += cachedInput.mouseInput.y * sensitivity;
        verticalY = Mathf.Clamp(verticalY, -90, 90);

        transform.localEulerAngles = new Vector3(0, verticalX, 0);

        // Apply to camera
        Quaternion baseRotation = Quaternion.Euler(-verticalY, 0, 0);

        // Calculate target lean 
        float targetLean = 0f;

        if (cachedInput.a && rb.linearVelocity.magnitude > 0.1f)   //left
            targetLean = leanAngle;
        else if (cachedInput.d && rb.linearVelocity.magnitude > 0.1f) //right
            targetLean = -leanAngle;

        // Smoothly interpolate
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        // Apply final rotation (pitch + lean)
        playerCamera.transform.localRotation = baseRotation * Quaternion.Euler(0, 0, currentLean);
    }

    public void Movement(float mult)
    {
        // Get input direction
        Vector3 direction = Vector3.zero;
        if (cachedInput.w) direction += transform.forward;
        if (cachedInput.s) direction += -transform.forward;
        if (cachedInput.a) direction += -transform.right;
        if (cachedInput.d) direction += transform.right;

        direction.Normalize();
        moveDirection = direction;

        // Calculate target velocity
        float controlFactor = (state == PlayerStates.InAir) ? airControl : 1f;
        Vector3 targetVelocity = direction * movementSpeed * mult * speedMultipl * sprintVelocityMultiplier * controlFactor;

        // Apply acceleration/deceleration
        if (direction.magnitude > 0.1f)
        {
            // Accelerate
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        // Apply movement
        Vector3 velocityChange = currentVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void Counter(float modifier)
    {
        // Only counter movement if not providing input
        if (moveDirection.magnitude < 0.1f)
        {
            Vector3 counterForce = new Vector3(-rb.linearVelocity.x, 0, -rb.linearVelocity.z) * modifier * (movementSpeed * 0.2f);
            rb.AddForce(counterForce, ForceMode.Acceleration);
        }
    }

    private void CheckForAir()
    {
        if (GetDistanceFromGround() > 1.12f)
        {
            SwitchState(PlayerStates.InAir);
            grounded = false;
        }
    }

    private void CheckForGround()
    {
        if (GetDistanceFromGround() < 1.12f)
        {
            SwitchState(PlayerStates.OnGround);
            grounded = true;
            doubleJump = true;
        }
    }

    private float GetDistanceFromGround()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, 5000, lm);
        return hit.distance;
    }

    private float GetDistanceFromRoof()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.up, out hit, 100, lm);
        if (hit.distance == 0)
        {
            return 100;
        }

        return hit.distance;
    }

    private void ToggleCursor()
    {
        Cursor.visible = !Cursor.visible;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void InitializacePlayer()
    {
        rb = GetComponent<Rigidbody>();
        grounded = true;
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.linearVelocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    public void SwitchState(PlayerStates newState)
    {
        if (state == newState)
        {
            return;
        }

        PlayerStates oldState = state;
        state = newState;

        switch (state)
        {
            case PlayerStates.OnGround:
                SetColliderToGround();
                maxVelocity = defaultMaxMovement;
                invokeUncrouch = false;
                doubleJump = true;
                targetFOV = normalFOV;
                break;
            case PlayerStates.InAir:
                SetColliderToGround();
                maxVelocity = defaultMaxMovement * 1.5f;
                targetFOV = normalFOV;
                break;
            case PlayerStates.Crouching:
                SetColliderToCrouch();
                maxVelocity = defaultCrouchMaxMovement;
                targetFOV = crouchFOV;
                break;
            case PlayerStates.Sliding:
                SetColliderToCrouch();
                timer = 0;
                targetFOV = slideFOV;
                break;   
            case PlayerStates.Dashing:
                SetColliderToGround();
                targetFOV = normalFOV;
                break;
        }
    }

    public void SetColliderToCrouch()
    {
        standingCollider.enabled = false;
        crouchingCollider.enabled = true;
        playerCamera.transform.localPosition = new Vector3(0, -0.2f, 0);
    }

    public void SetColliderToGround()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            standingCollider.enabled = true;
            crouchingCollider.enabled = false;
            playerCamera.transform.localPosition = new Vector3(0, 0.6f, 0);
        }
    }

    private void CheckAirCrouch()
    {
        if (cachedInput.crouch)
        {
            SetColliderToCrouch();
        }
    }

    private void CheckForAirUnCrouch()
    {
        if (Input.GetKeyUp(KeyCode.RightControl))
        {
            SetColliderToGround();
        }
    }

    // Split UpdateByState into physics and non-physics parts
    public void UpdateByStatePhysics()
    {
        switch (state)
        {
            case PlayerStates.OnGround:
                Movement(defaultMovement);
                Counter(defaultCounterMaxMovement);
                Jump();
                CheckForAir();
                break;

            case PlayerStates.InAir:
                Movement(defaultMovement * 0.5f);
                CheckForGround();
                Jump();
                Counter(airCounter * 1.25f);
                Gravity();
                break;

            case PlayerStates.Crouching:
                Movement(defaultMovement);
                Counter(defaultCounterMaxMovement);
                Jump();
                CheckForAir();
                break;

            case PlayerStates.Sliding:
                Jump();
                CheckForAir();
                Counter(timer * slideCounter);
                break;
        }
    }

    public void UpdateByStateNonPhysics()
    {
        switch (state)
        {
            case PlayerStates.OnGround:
                LookAround();
                if (!CheckForSlide())
                {
                    CheckForCrouch();
                }
                StopSlidingVFX();
                CheckDash();
                break;
            case PlayerStates.InAir:
                LookAround();
                CheckAirCrouch();
                CheckForAirUnCrouch();
                StopSlidingVFX();
                CheckDash();
                break;
            case PlayerStates.Crouching:
                LookAround();
                CheckForUnCrouch();
                StopSlidingVFX();
                break;
            case PlayerStates.Sliding:
                timer += Time.deltaTime;
                SlidingVFX();
                LookAround();
                CheckForUnCrouch();
                SlideCancel();
                break;
        }
    }

    public void Crouch()
    {
        SwitchState(PlayerStates.Crouching);
    }

    public void UnCrouch()
    {
        SwitchState(PlayerStates.OnGround);
    }

    public IEnumerator ForcedJump()
    {
        forceJump = true;
        yield return new WaitForSeconds(0.5f);
        forceJump = false;
    }

    private void SlidingVFX()
    {
        vfxPlayer.SlidingParticles();
    }

    private void StopSlidingVFX()
    {
        vfxPlayer.StopSlidingParticles();
    }

    public bool TryUseAbility()
    {
        if (Time.time >= lastUsedTime + cooldown)
        {
            lastUsedTime = Time.time;
            return true;
        }
        return false;
    }

    private float AirTimer()
    {
        if (state == PlayerStates.InAir)
        {
            airTimer += Time.deltaTime;
            return airTimer;
        }
        else
        {
            airTimer = 0;
            return airTimer;
        }
    }

    private void Gravity()
    {
        Vector3 force = new Vector3(0, -gravityAcce * AirTimer() * rb.mass * gravityModifier, 0);
        rb.AddForce(force);
    }

    private void CheckDash()
    {
        if (cachedInput.dash && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float originalDrag = rb.drag;
        float originalGravityModifer = gravityModifier;
        float originalMaxVelocity = maxVelocity;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        SwitchState(PlayerStates.Dashing);
        targetFOV = dashFOV;

        Vector3 inputDir = (playerCamera.transform.forward * v + playerCamera.transform.right * h);
        inputDir.y = 0f;
        inputDir = inputDir.normalized;

        rb.drag = 0f;
        gravityModifier = 0f;

        maxVelocity = defaultMaxMovement * 6f;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            inputDir = moveDirection.normalized;
        }

        // Apply dash force with more control
        rb.AddForce(inputDir.normalized * dashForce * 0.5f, ForceMode.VelocityChange);

        float t = 0f;
        while (t < dashDuration)
        {
            LookAround();
            t += Time.deltaTime;
            yield return null;
        }

        rb.drag = originalDrag;
        gravityModifier = originalGravityModifer;
        maxVelocity = originalMaxVelocity;
        isDashing = false;

        if (GetDistanceFromGround() < 1.12f)
        {
            SwitchState(PlayerStates.OnGround);
        }
        else
        {
            SwitchState(PlayerStates.InAir);
        }
    }
}