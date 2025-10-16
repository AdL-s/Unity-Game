using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;
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
    public float movementSpeed = 8f;
    public float maxVelocity = 2;

    [Header("Movement Smoothing")]
    public float acceleration = 12f;
    public float deceleration = 15f;
    public float airControl = 0.7f;

    [Header("Jump")]
    public float secondJumpMultForce = 1.25f;
    public float jumpForce = 50;
    public float doubleJumpDelay = 0.3f;

    private float lastJumpTime = 0f;

    [Header("Jump Momentum")]
    public float momentumMultiplier = 0.3f;  // How much speed affects jump
    public float maxMomentumBonus = 20f;     // Max bonus jump force
    public float jumpAngle = 0.3f;           // How much forward vs up (0 = straight up, 1 = 45 degrees)

    [Header("Gravity")]
    public float gravityAcce = 15;
    private float gravityModifier = 1;
    private float airTimer = 0f;
    public float baseGravity = 15f;        // Initial gravity force
    public float maxGravity = 35f;         // Maximum gravity force
    public float gravityBuildupTime = 1f;  // Time to reach max gravity
    public float gravityMultiplier = 1f;   // Overall gravity modifier

    [Header("AirCounterMov")]
    private float airCounter = 0.015f;

    [Header("Slide")]
    public float slideStarter = 0.8f; //what percantage of max velocity you can start slide 
    public float slideCounter = 0.03f;
    public float slideForce = 500;
    public float cooldown = 2f;


    [Header("Abillity Cooldown")]
    private float lastUsedTime = -Mathf.Infinity;

    [Header("Colliders")]
    public Collider crouchingCollider;
    public Collider standingCollider;


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

    private bool invokeUncrouch = false;
    private float timer = 0;

    private bool canMoveThatDir = false;//bool to check if we can move that direction

    private bool doubleJump = true;
    private bool forceJump = false;

    private VFXPlayer vfxPlayer;
    private FovUpdates FoP;

    private Vector3 moveDirection = Vector3.zero;

    [Header("layerMasks")]
    public LayerMask lm;
    public bool crouched = false;

    public bool grounded;
    public enum PlayerStates { OnGround = 0, InAir = 1, Crouching = 2, Sliding = 3, Dashing = 4 };
    public PlayerStates state;

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
            jump = Input.GetKey(KeyCode.Space);
            crouch = Input.GetKeyDown(KeyCode.LeftControl);
            crouchUp = Input.GetKeyUp(KeyCode.LeftControl);
            dash = Input.GetKeyDown(KeyCode.LeftShift);
            mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
    }

    private InputCache cachedInput;

    private void Start()
    {

      
        ToggleCursor();
        InitializacePlayer();
        vfxPlayer = GetComponent<VFXPlayer>();
        FoP = GetComponent<FovUpdates>();
        FoP.targetFOV = FoP.normalFOV;
        playerCamera.fieldOfView = FoP.normalFOV; 
    }

    private void Update()
    {
        // Cache input for FixedUpdate
        cachedInput.Cache();

        LookAround();
        UpdateByStatePhysics();
        UpdateByStateNonPhysics();
        FoP.UpdateFOV();
        AirTimer();
    }

    private void FixedUpdate()
    {
        // Handle all physics-based movement
        UpdateByStatePhysics();
    }

    private void Jump()
    {

        if (state == PlayerStates.InAir && !doubleJump)
        {
            StartCoroutine(ForcedJump());
            return;
        }

        if (cachedInput.jump || forceJump)
        {
            if (GetDistanceFromRoof() > 1.12f && state != PlayerStates.InAir && state != PlayerStates.Dashing)
            {

                // Get current horizontal velocity
                Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                float horizontalSpeed = horizontalVelocity.magnitude;

                // Calculate jump direction (up + forward momentum)
                Vector3 jumpDirection = Vector3.up;
                if (horizontalSpeed > 0.1f)
                {
                    // Add forward component based on movement direction
                    jumpDirection += horizontalVelocity.normalized * 0.3f;
                    jumpDirection = jumpDirection.normalized;
                }

                // Apply jump with momentum 
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(jumpDirection * jumpForce, ForceMode.VelocityChange);

                vfxPlayer.PlayParticles();
                lastJumpTime = Time.time; // Record jump time
                forceJump = false;
            }
            else if (state == PlayerStates.InAir && GetDistanceFromRoof() > 1.12f && doubleJump && cachedInput.jump)
            {

                if (Time.time - lastJumpTime >= doubleJumpDelay)
                {
                    airCounter = 0;
                    airTimer = 0;

                    Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                    float horizontalSpeed = horizontalVelocity.magnitude;

                    Vector3 jumpDirection = Vector3.up;
                    if (horizontalSpeed > 0.1f)
                    {
                        jumpDirection += horizontalVelocity.normalized * 0.25f;
                        jumpDirection = jumpDirection.normalized;
                    }

                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                    rb.AddForce(jumpDirection * (jumpForce * secondJumpMultForce), ForceMode.VelocityChange);

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
            rb.AddForce(rb.linearVelocity.normalized * slideForce * 0.7f, ForceMode.VelocityChange);
            vfxPlayer.PlayDashingPS();
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

    private void Movement(float mult)
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
        Vector3 targetVelocity = direction * movementSpeed * mult;

        // Get current horizontal velocity
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // Raycast setup - 5 rays to cover more area
        float rayDistance = 1.2f;

        if (crouched) // Collider height: 1.1
        {
            Vector3 ray1 = transform.position + transform.up * -0.65f;
            Vector3 ray2 = transform.position + transform.up * -0.325f;
            Vector3 ray3 = transform.position;
            Vector3 ray4 = transform.position + transform.up * 0.325f;
            Vector3 ray5 = transform.position + transform.up * 0.65f;

            /*Debug.DrawRay(ray1, direction * rayDistance, Color.red, 0.1f);
            Debug.DrawRay(ray2, direction * rayDistance, Color.yellow, 0.1f);
            Debug.DrawRay(ray3, direction * rayDistance, Color.green, 0.1f);
            Debug.DrawRay(ray4, direction * rayDistance, Color.cyan, 0.1f);
            Debug.DrawRay(ray5, direction * rayDistance, Color.blue, 0.1f);*/

            bool hit1 = Physics.Raycast(ray1, direction, rayDistance, lm);
            bool hit2 = Physics.Raycast(ray2, direction, rayDistance, lm);
            bool hit3 = Physics.Raycast(ray3, direction, rayDistance, lm);
            bool hit4 = Physics.Raycast(ray4, direction, rayDistance, lm);
            bool hit5 = Physics.Raycast(ray5, direction, rayDistance, lm);

            canMoveThatDir = !(hit1 || hit2 || hit3 || hit4 || hit5);
        }
        else // Collider height: 2.2
        {
            Vector3 ray1 = transform.position + transform.up * -0.8f;
            Vector3 ray2 = transform.position + transform.up * -0.4f;
            Vector3 ray3 = transform.position;
            Vector3 ray4 = transform.position + transform.up * 0.4f;
            Vector3 ray5 = transform.position + transform.up * 0.8f;

            /*Debug.DrawRay(ray1, direction * rayDistance, Color.red, 0.1f);
            Debug.DrawRay(ray2, direction * rayDistance, Color.yellow, 0.1f);
            Debug.DrawRay(ray3, direction * rayDistance, Color.green, 0.1f);
            Debug.DrawRay(ray4, direction * rayDistance, Color.cyan, 0.1f);
            Debug.DrawRay(ray5, direction * rayDistance, Color.blue, 0.1f);*/

            bool hit1 = Physics.Raycast(ray1, direction, rayDistance, lm);
            bool hit2 = Physics.Raycast(ray2, direction, rayDistance, lm);
            bool hit3 = Physics.Raycast(ray3, direction, rayDistance, lm);
            bool hit4 = Physics.Raycast(ray4, direction, rayDistance, lm);
            bool hit5 = Physics.Raycast(ray5, direction, rayDistance, lm);

            canMoveThatDir = !(hit1 || hit2 || hit3 || hit4 || hit5);
        }

        // If there's an object in that direction, don't apply movement
        if (!canMoveThatDir) return;

        // Apply acceleration/deceleration
        Vector3 newVelocity;
        if (direction.magnitude > 0.1f)
        {
            // Accelerate toward target velocity
            newVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // Decelerate toward zero
            newVelocity = Vector3.Lerp(currentHorizontalVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Calculate velocity change needed
        Vector3 velocityChange = newVelocity - currentHorizontalVelocity;

        // Apply force
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    private void Counter(float modifier)
    {

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
            if (state != PlayerStates.Dashing)
            {
                SwitchState(PlayerStates.InAir);
            }
            grounded = false;
        }
    }

    private void CheckForGround()
    {
        if (GetDistanceFromGround() < 1.12f)
        {
            if (!crouchingCollider.enabled)
            {
                SwitchState(PlayerStates.OnGround);
            }
            else
            {
                Slide();
            }


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

        switch (oldState)
        {
            case PlayerStates.Dashing:

                break;
            case PlayerStates.Sliding:
                vfxPlayer.StopDashingPS();
                vfxPlayer.StopSlidingParticles();
                break;
            case PlayerStates.InAir:

                break;
            case PlayerStates.OnGround:

                break;
        }


        state = newState;

        switch (state)
        {
            case PlayerStates.OnGround:
                invokeUncrouch = false;
                maxVelocity = defaultMaxMovement;
                doubleJump = true;
                SetColliderToGround();
                FoP.targetFOV = FoP.normalFOV;
                break;
            case PlayerStates.InAir:
                SetColliderToGround();
                FoP.targetFOV = FoP.normalFOV;
                break;
            case PlayerStates.Crouching:
                SetColliderToCrouch();
                maxVelocity = defaultCrouchMaxMovement;
                FoP.targetFOV = FoP.crouchFOV;
                break;
            case PlayerStates.Sliding:
                SetColliderToCrouch();
                timer = 0;
                FoP.targetFOV = FoP.slideFOV;
                break;
            case PlayerStates.Dashing:
                SetColliderToGround();
                FoP.targetFOV = FoP.normalFOV;
                break;
        }
    }

    public void SetColliderToCrouch()
    {
        standingCollider.enabled = false;
        crouchingCollider.enabled = true;

        crouched = true;

        Vector3 StandingCol = new Vector3(0, 0.6f, 0);
        Vector3 CrouchCol = new Vector3(0, -0.2f, 0);

        playerCamera.transform.localPosition = Vector3.Lerp(CrouchCol, StandingCol, Time.deltaTime * FoP.colChangeCamSpeed);
    }

    public void SetColliderToGround()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            standingCollider.enabled = true;
            crouchingCollider.enabled = false;
            crouched = false;

            
            Vector3 StandingCol = new Vector3(0, 0.6f, 0);
            Vector3 CrouchCol = new Vector3(0, -0.2f, 0);

            playerCamera.transform.localPosition = Vector3.Lerp(StandingCol, CrouchCol, Time.deltaTime * FoP.colChangeCamSpeed); ;
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
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            SetColliderToGround();
        }
    }

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
                Movement(defaultMovement);
                CheckForGround();
                Jump();
                Counter(airCounter);
                Gravity();
                break;

            case PlayerStates.Crouching:
                Movement(defaultMovement * 0.5f);
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

                if (!CheckForSlide())
                {
                    CheckForCrouch();
                }
                CheckDash();
                break;
            case PlayerStates.InAir:

                CheckAirCrouch();
                CheckForAirUnCrouch();
                CheckDash();
                break;
            case PlayerStates.Crouching:

                CheckForUnCrouch();
                break;
            case PlayerStates.Sliding:
                timer += Time.deltaTime;
                vfxPlayer.SlidingParticles();
               
                CheckForUnCrouch();
                SlideCancel();
                break;
            case PlayerStates.Dashing:


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
        if (state != PlayerStates.InAir) return;

        // Calculate gravity progression (0 to 1)
        float gravityProgress = Mathf.Clamp01(airTimer / gravityBuildupTime);

        // Smooth curve
        float smoothProgress = Mathf.SmoothStep(0f, 1f, gravityProgress);

        // Calculate current gravity force
        float currentGravity = Mathf.Lerp(baseGravity, maxGravity, smoothProgress);

        // Apply
        Vector3 gravityForce = Vector3.down * currentGravity * rb.mass * gravityMultiplier;
        rb.AddForce(gravityForce);
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
        SwitchState(PlayerStates.Dashing);
        vfxPlayer.PlayDashingPS();

        float originalDrag = rb.drag;
        float originalGravityModifer = gravityModifier;
        float originalMaxVelocity = maxVelocity;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        FoP.targetFOV = FoP.dashFOV;

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

        rb.AddForce(inputDir.normalized * dashForce * 0.5f, ForceMode.VelocityChange);

        float t = 0f;
        while (t < dashDuration)
        {

            Vector3 vel = rb.velocity;
            vel.y = 0f;
            rb.velocity = vel;

            t += Time.deltaTime;
            vfxPlayer.StopDashingPS();
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