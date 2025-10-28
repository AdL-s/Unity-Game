using System.Collections;
using UnityEngine;

public class SniperRifle : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private FovUpdates fov;
    public float rateOfFire = 80f;
    private float timer = 0f;
    public float damage = 90f;
    public float waitTime = 0.8f;
    private bool ready = false;

    [Header("Scope Settings")]
    public float scopedFOV = 20f;
    public float normalFOV = 60f;
    public bool isScoped = false;
    public float ScopedSens = 0.2f;
    float originalSens;

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;
    public AudioClip fireSound;
    public AudioClip impactSound;
    public AudioSource audioSource;
    
    [Header("Ray Settings")]
    public float rayDuration = 0.8f;
    public LineRenderer lineRendererPrefab;

    [Header("Model and Muzzle")]
    public Transform muzzlePoint;

    [Header("Animation")]
    public Animation animationComponent;
    public AnimationClip shootClip;

    [Header("VFX")]
    public GameObject hitEffectPrefab;
    public ParticleSystem shootingPS;

    [Header("Ray Settings")]
    public LayerMask lm = (1 << 0) | (1 << 9);
    private LayerMask enemyLm = 9;
    public CameraMovement CM;



    void Start()
    {
        originalSens = CM.sensitivity;
        if (CM == null)
            CM = GetComponent<CameraMovement>();

        // Set initial FOV if fov component exists
        if (fov != null)
        {
            normalFOV = fov.targetFOV; // Store current FOV as normal
        }
        if (animationComponent != null && shootClip != null)
        {
            animationComponent.AddClip(shootClip, "SniperRifleAnimation");
            shootClip.legacy = true;
            shootClip.wrapMode = WrapMode.Once;
            shootClip.SampleAnimation(gameObject, 0f);
        }

    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Update FOV transitions
        if (fov != null)
            fov.UpdateFOV();

        HandleScoping();
        HandleInput();
    }
    private void HandleInput()
    {
        if (!ready) return;

        if (Input.GetMouseButtonDown(0) && timer <= 0)
        {
            Shoot();
            timer = RayCastsScript.setTimer(rateOfFire);
        }
    }

    private void HandleScoping()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            ToggleScope();
        }
    }

  

    private void ToggleScope()
    {
        if (fov == null) return;

        isScoped = !isScoped;

        if (isScoped)
        {
            CM.sensitivity = CM.sensitivity * ScopedSens;
            fov.targetFOV = scopedFOV; 
        }
        else
        {
            fov.targetFOV = normalFOV;
            CM.sensitivity = originalSens;

        }

    }

    private void OnEnable()
    {
        shootClip.SampleAnimation(gameObject, 0f);
        StartCoroutine(WaitCoroutine());

        // Reset scope when weapon is enabled
            isScoped = false;
            fov.targetFOV = normalFOV;
    }

    private void OnDisable()
    {
        timer = 0;

        // Reset scope when weapon is disabled
        if (fov != null)
        {
            isScoped = false;
        }
        fov.targetFOV = normalFOV;
        fov.UpdateFOV();
        CM.sensitivity = originalSens;
    }

    private IEnumerator WaitCoroutine()
    {
        ready = false;
        yield return new WaitForSeconds(waitTime);
        ready = true;
    }


    private void Shoot()
    {
        // Play legacy animation
        if (animationComponent.IsPlaying("SniperRifleAnimation"))
            animationComponent.Stop("SniperRifleAnimation");

        animationComponent.Play("SniperRifleAnimation");

        RayCastsScript.FireRayCastLineRay(
             muzzlePoint, lm, enemyLm,
             hitEffectPrefab,
             lineRendererPrefab, rayDuration,
             audioSource, fireSound,
             pitchRandomMin, pitchRandomMax,
             shootingPS,
             damage,
             impactSound
         );
    }
}
