using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SniperRifle : MonoBehaviour
{
    [SerializeField] private CameraMovement fov;

    public float rateOfFire = 200f;
    private float timer = 0f;

    public float damage = 35;

    public float waitTime = 2f;//after switching to this weapon how many s you have to wait to fire
    private bool ready = false; // tells if waitTime is over 

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;

    public GameObject gunModel;   // Gun mesh root
    public Transform muzzlePoint; // Assign the muzzle/bullet spawn point in Inspect
    public GameObject hitEffectPrefab;

    public AudioClip fireSound;
    public AudioSource audioSource;

    private VFXPlayer vfxPlayer;
    private GunModelShooting GunModelShooting;
    public LayerMask lm;
    private LayerMask enemyLm = 9;
    void Start()
    {
        GunModelShooting = GetComponent<GunModelShooting>();
        vfxPlayer = GetComponent<VFXPlayer>();
    }

    
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            fov.targetFOV = 30f;
            fov.UpdateFOV();
            timer -= Time.deltaTime;
            SingleFire();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(WaitCoroutine());
    }
    private IEnumerator WaitCoroutine()
    {
        ready = false;
        yield return new WaitForSeconds(waitTime);
        ready = true;

    }
    private void SingleFire()
    {
        if (!ready)
        {
            return;
        }else if (Input.GetMouseButtonDown(1)){


        }
        else if (Input.GetMouseButtonDown(0) && timer <= 0)
        {
            setTimer(rateOfFire);
            FireRayCast();
        }
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
            // Crosshair hit
            targetPoint = camHit.point;

            // Spawn particles at hit
            GameObject particles = Instantiate(hitEffectPrefab, camHit.point, Quaternion.identity);

            particles.transform.forward = camHit.normal;

            if (camHit.collider.gameObject.layer == enemyLm)
            {
                EnemyBasic enemyBasic = camHit.collider.GetComponent<EnemyBasic>();
                enemyBasic.DamageRecivied(damage);
            }
        }
        else
        {
            // Nothing hit, just go far
            targetPoint = camRay.origin + camRay.direction * 1000f;
        }

        // Direction from muzzle to crosshair hit
        Vector3 direction = (targetPoint - origin).normalized;

        // Debug ray (from muzzle to crosshair hit)
        Debug.DrawRay(origin, direction * 100f, Color.red, 1f);

        // Sound of firing
        audioSource.pitch = Random.Range(pitchRandomMin, pitchRandomMax);
        audioSource.PlayOneShot(fireSound);
    }
    private void setTimer(float rateoffFire)
    {
        timer = 60 / rateoffFire;
    }
    private void OnDisable()
    {
        timer = 0;

    }

}
