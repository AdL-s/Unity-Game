using System.Collections;
using UnityEngine;

public class SniperRifle : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private FovUpdates fov;
    public float rateOfFire = 200f;
    private float timer = 0f;
    public float damage = 35;
    public float waitTime = 2f;
    private bool ready = false;

    [Header("Scope Settings")]
    public float scopedFOV = 30f;
    public float normalFOV = 60f;
    public bool isScoped = false;
    public bool isScoped2 = false;

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;
    public GameObject gunModel;
    public Transform muzzlePoint;
    public GameObject hitEffectPrefab;
    public AudioClip fireSound;
    public AudioSource audioSource;

    

    public LayerMask lm;
    private LayerMask enemyLm = 9;

    void Start()
    {

        // Set initial FOV if fov component exists
        if (fov != null)
        {
            normalFOV = fov.targetFOV; // Store the current FOV as normal
        }
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        // Continuously update FOV for smooth transitions
        if (fov != null)
        {
            fov.UpdateFOV();
        }

        // Handle scoping (right-click to aim down sights)
        HandleScoping();

        // Handle firing (left-click to shoot)
        HandleShooting();
    }

    private void HandleScoping()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click pressed
        {
            ToggleScope();
        }
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click pressed
        {
            SingleFire();
        }
    }

    private void ToggleScope()
    {
        if (fov == null) return;

        isScoped = !isScoped;

        if (isScoped)
        {
            // Zoom in
            fov.targetFOV = scopedFOV;
        }
        else
        {
            // Zoom out
            fov.targetFOV = normalFOV;
        }

        // No need to call UpdateFOV() here since it's called every frame in Update()
    }

    private void OnEnable()
    {
        StartCoroutine(WaitCoroutine());

        // Reset scope when weapon is enabled
        if (fov != null)
        {
            isScoped = false;
            fov.targetFOV = normalFOV;
            // UpdateFOV() will be called in Update()
        }
    }

    private void OnDisable()
    {
        timer = 0;

        // Reset scope when weapon is disabled
        if (fov != null)
        {
            isScoped = false;
            fov.targetFOV = normalFOV;
            // Force immediate FOV change when disabling
            fov.UpdateFOV();
        }
    }

    private IEnumerator WaitCoroutine()
    {
        ready = false;
        yield return new WaitForSeconds(waitTime);
        ready = true;
    }

    private void SingleFire()
    {
        if (!ready || timer > 0)
        {
            return;
        }

        setTimer(rateOfFire);
        FireRayCast();
    }

    public void FireRayCast()
    {
        // Muzzle world position
        Vector3 origin = muzzlePoint.position;

        // Cast a ray from the center of the screen (crosshair)
        Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out RaycastHit camHit, 5000, lm))
        {
            // Crosshair hit something
            targetPoint = camHit.point;

            // Spawn particles at hit point
            if (hitEffectPrefab != null)
            {
                GameObject particles = Instantiate(hitEffectPrefab, camHit.point, Quaternion.identity);
                particles.transform.forward = camHit.normal;
            }

            // Check if we hit an enemy
            if (camHit.collider.gameObject.layer == enemyLm)
            {
                EnemyBasic enemyBasic = camHit.collider.GetComponent<EnemyBasic>();
                if (enemyBasic != null)
                {
                    enemyBasic.DamageRecivied(damage);
                }
            }
        }
        else
        {
            // Nothing hit, ray goes far into distance
            targetPoint = camRay.origin + camRay.direction * 1000f;
        }

        // Direction from muzzle to target point
        Vector3 direction = (targetPoint - origin).normalized;

        // Debug visualization
        Debug.DrawRay(origin, direction * 100f, Color.red, 1f);

        // Play fire sound with random pitch
        if (audioSource != null && fireSound != null)
        {
            audioSource.pitch = Random.Range(pitchRandomMin, pitchRandomMax);
            audioSource.PlayOneShot(fireSound);
        }

    }

    private void setTimer(float rateOfFire)
    {
        timer = 60f / rateOfFire;
    }
}