using System.Collections;
using UnityEngine;

public class SMGScript : MonoBehaviour
{
    public float rateOfFire = 200f;
    private float timer = 0f;

    public float damage = 10;


    public float waitTime = 2f;//after switching to this weapon how many s you have to wait to fire
    private bool ready = false; // tells if waitTime is over 

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;

    [Header("SMG model and Muzzle")]
    public GameObject gunModel;   // Gun mesh root
    public Transform muzzlePoint; // Assign the muzzle/bullet spawn point in Inspect
    
    [Header("Sounds")]
    public AudioClip fireSound;
    public AudioSource audioSource;

    private VFXPlayer vfxPlayer;
    private GunModelShooting GunModelShooting;
    public GameObject hitEffectPrefab;

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
        AutomaticFire();
    }
    private void AutomaticFire()
    {
        if (!ready)
        {
            return;
        }
        else if (Input.GetMouseButton(0) && timer <= 0)
        {
            setTimer(rateOfFire);
            FireRayCast();
        }
    }
    /*private void SingleFire()
    {
        if (!ready)
        {
            return;
        }
        else if (Input.GetMouseButtonDown(0) && timer <= 0)
        {
            setTimer(rateOfFire);
    FireRayCast();
}
    }*/

    

    public void FireRayCast()
    {
        // Get a ray from the center of the screen (crosshair)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));


        // Muzzle flash

        // Check for hit
        if (Physics.Raycast(ray, out RaycastHit hit, 5000, lm))
        {
            // Spawn hit effect
            GameObject particles = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
            particles.transform.forward = hit.normal;
           
            if(hit.collider.gameObject.layer == enemyLm)
            {
                EnemyBasic enemyBasic = hit.collider.GetComponent<EnemyBasic>();
                enemyBasic.DamageRecivied(damage);
            }
        }
        // Play sound
        audioSource.clip = fireSound;
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
