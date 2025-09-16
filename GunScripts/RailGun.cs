using System.Collections;
using UnityEngine;

public class RailGun : MonoBehaviour
{
    public float rateOfFire = 200f;
    private float timer = 0f;
    public float damage = 70;

    [Header("Sound Settings")]
    [SerializeField] public float pitchRandomMin = 0.7f;
    [SerializeField] public float pitchRandomMax = 0.85f;

    [Header("Ray Settings")]
    public float rayDuration = 0.5f;
    public float waitTime = 2f;//after switching to this weapon how many s you have to wait to fire
    public LineRenderer lineRendererPrefab;

    public GameObject gunModel;   // Gun mesh root
    public Transform muzzlePoint; // Assign the muzzle/bullet spawn point in Inspect
    public GameObject hitEffectPrefab;

    private bool ready = false; // tells if waitTime is over 
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
        timer -= Time.deltaTime;
        SingleFire();
    }
    private void SingleFire()
    {
        if (!ready)
        {
            return;
        }else if (Input.GetMouseButtonDown(0) && timer <= 0)
         {

            FireRayCast();
            setTimer(rateOfFire);
         }
    }

    public void FireRayCast()
    {
        // Start of the laser (muzzle)
        Vector3 origin = muzzlePoint.position;

        // Ray from the center of the screen (crosshair)
        Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // The point we want to shoot at
        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out hit, 5000, lm))
        {
            targetPoint = hit.point;

            // Spawn hit particles
            GameObject particles = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
            particles.transform.forward = hit.normal;
            if (hit.collider.gameObject.layer == enemyLm)
            {
                EnemyBasic enemyBasic = hit.collider.GetComponent<EnemyBasic>();
                enemyBasic.DamageRecivied(damage);
            }
        }
        else
        {
            // If nothing hit, shoot straight far away
            targetPoint = camRay.origin + camRay.direction * 1000f;
        }

        // Instantiate laser beam
        LineRenderer lr = Instantiate(lineRendererPrefab);
        lr.SetPosition(0, origin);       // start from muzzle
        lr.SetPosition(1, targetPoint);  // end at crosshair target
        lr.GetComponent<LineFade>().Init(rayDuration);

        // Play sound
        audioSource.pitch = Random.Range(pitchRandomMin, pitchRandomMax);
        audioSource.PlayOneShot(fireSound);

        // Play muzzle flash
        vfxPlayer.PlayShootingPS();

        // Debug line
        Debug.DrawLine(origin, targetPoint, Color.red, 1f);
    }


    private void setTimer(float rateoffFire)
    {
        timer = 60 / rateoffFire;
    }

    private void OnDisable()
    {
        timer = 0;
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
}
