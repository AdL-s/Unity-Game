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
    public float movementSpeed = 1;
    public float maxVelocity = 2;

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
    // private float sprintLerpSpeed = 5f; 
    private float sprintVelocityMultiplier = 1f;
    public float cooldown = 2f;
    //private float targetSprintMultiplier = 1f; 

    public bool grounded;

    [Header("Abillity Cooldown")]
    private float lastUsedTime = -Mathf.Infinity;

    [Header("Colliders")]
    public Collider crouchingCollider;
    public Collider standingCollider;

    [Header("Smooth Movement")]
    public float accelerationSpeed = 3f;      // How fast you accelerate
    public float decelerationSpeed = 5f;     // How fast you decelerate
    private Vector3 currentForce = Vector3.zero;  // Current force being applied

    private bool invokeUncrouch = false;
    private float timer = 0;

    private bool doubleJump = true;
    private bool forceJump = false;
    //private bool dopadVFX = false;

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
    public float dashForce = 20f;        // how hard the burst is
    public float dashDuration = 0.15f;   // how long you’re “in dash state”
    public float dashCooldown = 0.8f;    // time before you can dash again
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;

    private VFXPlayer vfxPlayer;

    private Vector3 moveDirection = Vector3.zero;

    [Header("layerMasks")]
    public LayerMask lm;



    public enum PlayerStates { OnGround = 0, InAir = 1, Crouching = 2, Sliding = 3, Sprinting = 4, Dashing = 5 };
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
        UpdateByState();
        UpdateFOV();
        AirTimer();

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
        if (state == PlayerStates.InAir && !doubleJump)
        {
            StartCoroutine(ForcedJump());
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) == true || forceJump)
        {
            if (GetDistanceFromRoof() > 1.12f && state != PlayerStates.InAir && state != PlayerStates.Dashing)
            {
                rb.AddForce((Vector3.up + moveDirection / 2) * jumpForce, ForceMode.VelocityChange);
                vfxPlayer.PlayParticles();
                forceJump = false;
            }
            else if (state == PlayerStates.InAir && GetDistanceFromRoof() > 1.12f)
            {
                if (doubleJump)
                {
                    airCounter = 0;
                    airTimer = 0;
                    rb.AddForce(((Vector3.up + moveDirection / 2) * jumpForce) * secondJumpMultForce, ForceMode.VelocityChange);
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

            rb.AddForce(rb.linearVelocity.normalized * slideForce, ForceMode.VelocityChange);
            SwitchState(PlayerStates.Sliding);

        }
        else
        {
            Crouch();
        }

    }

    private bool CheckForSlide()
    {

        if (rb.linearVelocity.magnitude > maxVelocity * slideStarter && Input.GetKey(KeyCode.LeftControl))
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

        if (Input.GetKeyUp(KeyCode.LeftControl))
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
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    private void LookAround()
    {
        //Mouse look
        verticalX += Input.GetAxis("Mouse X") * sensitivity;
        verticalY += Input.GetAxis("Mouse Y") * sensitivity;
        verticalY = Mathf.Clamp(verticalY, -90, 90);

        transform.localEulerAngles = new Vector3(0, verticalX, 0);

        // Apply to camera
        Quaternion baseRotation = Quaternion.Euler(-verticalY, 0, 0);

        // Calculate target lean 
        float targetLean = 0f;

        if (Input.GetKey(KeyCode.A) && rb.linearVelocity.magnitude > 0.1f)   //left
            targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.D) && rb.linearVelocity.magnitude > 0.1f) //right
            targetLean = -leanAngle;

        // Smoothly interpolate
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        // Apply final rotation (pitch + lean)
        playerCamera.transform.localRotation = baseRotation * Quaternion.Euler(0, 0, currentLean);
    }
    public void Movement(float mult)
    {
        Vector3 direction = Vector3.zero;
        Vector2 lookMag = FindVelRelativeToLook();
        if (Input.GetKey(KeyCode.W) == true)
        {
            if (lookMag.y < maxVelocity)
                direction += transform.forward;
        }

        if (Input.GetKey(KeyCode.S) == true)
        {
            if (lookMag.y > -maxVelocity)
                direction += -transform.forward;

        }

        if (Input.GetKey(KeyCode.A) == true)
        {
            if (lookMag.x > -maxVelocity)
                direction += -transform.right;
        }

        if (Input.GetKey(KeyCode.D) == true)
        {
            if (lookMag.x < maxVelocity)
                direction += transform.right;
        }

        rb.AddForce(direction.normalized * Time.deltaTime * movementSpeed * mult * speedMultipl * sprintVelocityMultiplier);
        moveDirection = direction.normalized;

    }

    private void Counter(float modifier)
    {
        rb.AddForce(new Vector3(-rb.linearVelocity.x, 0, -rb.linearVelocity.z) * Time.deltaTime * modifier * (movementSpeed * 0.2f));

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
            case PlayerStates.Sprinting:
                SetColliderToGround();
                maxVelocity = defaultMaxMovement * 3;
                invokeUncrouch = false;
                doubleJump = true;

                targetFOV = sprintFOV;

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
        if (Input.GetKey(KeyCode.LeftControl) != true)
        {
            standingCollider.enabled = true;
            crouchingCollider.enabled = false;
            playerCamera.transform.localPosition = new Vector3(0, 0.6f, 0);

        }

    }

    private void CheckAirCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
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
    public void UpdateByState()
    {
        switch (state)
        {
            case PlayerStates.OnGround:
                Movement(defaultMovement);
                LookAround();
                Counter(defaultCounterMaxMovement);
                if (!CheckForSlide())
                {
                    CheckForCrouch();
                }
                Jump();
                CheckForAir();
                StopSlidingVFX();
                //GetSprint();
                CheckDash();
                break;
            case PlayerStates.InAir:
                AirTimer();
                LookAround();
                Movement(defaultMovement * 0.1f);
                CheckForGround();
                Jump();
                CheckAirCrouch();
                Counter(airCounter * 1.25f);
                CheckForAirUnCrouch();
                StopSlidingVFX();
                Gravity();
                CheckDash();

                break;
            case PlayerStates.Crouching:
                LookAround();
                Movement(defaultMovement);
                Counter(defaultCounterMaxMovement);
                CheckForUnCrouch();
                Jump();
                CheckForAir();
                StopSlidingVFX();


                break;
            case PlayerStates.Sliding:

                timer += Time.deltaTime;
                SlidingVFX();
                LookAround();
                Jump();
                CheckForAir();
                Counter(timer * slideCounter);
                CheckForUnCrouch();
                SlideCancel();

                break;
            case PlayerStates.Sprinting:

                LookAround();
                Movement(defaultMovement);
                Counter(defaultCounterMaxMovement);
                if (!CheckForSlide())
                {
                    CheckForCrouch();
                }
                Jump();
                CheckForAir();
                Counter(0.12f);
                StopSlidingVFX();
                //Sprint();

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
    //
    /* public void GetSprint()
     {
         if (Input.GetKeyDown(KeyCode.LeftShift))
         {
             SwitchState(PlayerStates.Sprinting);
         }
     }

     public void Sprint()
     {
         if (grounded)
         {
             sprintVelocityMultiplier = Mathf.Lerp(sprintVelocityMultiplier, targetSprintMultiplier, Time.deltaTime * sprintLerpSpeed);
         }

     }
    */
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
        if (Input.GetKeyDown(dashKey) && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float originalDrag = rb.drag;//could change
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
        /*else
        {
            inputDir  = Vector3.ProjectOnPlane(playerCamera.transform.forward, playerCamera.transform.up).normalized;
            Debug.Log(inputDir);
            Debug.Log("2");
        }*/

        rb.AddForce(inputDir.normalized * dashForce, ForceMode.VelocityChange);

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

