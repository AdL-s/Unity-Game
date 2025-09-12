using JetBrains.Annotations;
using System.Net.NetworkInformation;
using UnityEngine;

public class GunModelShooting : MonoBehaviour
{
    public float rateOfFire = 200f;
    public int ammo = 1;
    private float timer = 0f;
    
    [Header("FireRate")]
    public float pistolFireRate = 120f;
    public float smgFireRate = 400f;

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;

    [Header("Ray Settings")]
    public float rayDuration = 0.5f;
    public LineRenderer lineRendererPrefab;


    public GameObject bullet;
    public GameObject hitEffectPrefab;

    [Header("Gun Models")]
    public GameObject pistolModel;
    public GameObject smgModel;
    
    public GameObject gunModel;   // Gun mesh root
    public Transform muzzlePoint; // Assign the muzzle/bullet spawn point in Inspector
    
    public AudioClip fireSound;
    public AudioSource audioSource;
    
    private CameraMovement cameraMovement;
    private VFXPlayer vfxPlayer;

    public LayerMask lm;

    public enum gunMode { SingleFire = 0, Automatic = 1, Burst = 2, Shotgun = 3 };
    public gunMode mode;

    private void Start()
    {
        cameraMovement = GetComponent<CameraMovement>();
        vfxPlayer = GetComponent<VFXPlayer>();
    }

    private void Update()
    {
        UpdateMode();
        timer -= Time.deltaTime;
    }

    private void setTimer(float rateoffFire)
    {
        timer = 60 / rateoffFire;
    }

    private void Fire()
    {
       
        GameObject newBullet = Instantiate(bullet, muzzlePoint.position, muzzlePoint.rotation);

        newBullet.transform.forward = muzzlePoint.forward;
    }

    private void ModeSwitch(gunMode newMode)
    {
        if (mode == newMode) return;
        
        mode = newMode;
    
        switch (mode)
        {
            case gunMode.SingleFire:
                rateOfFire = pistolFireRate;
                break;
            case gunMode.Automatic:
                rateOfFire = smgFireRate;
                break;
            case gunMode.Burst:
                break;
            case gunMode.Shotgun:
                break;
        }
    }

    public void UpdateMode()
    {
        switch (mode)
        {
            case gunMode.SingleFire:
                SingleFire();
                Smg();
                break;
            case gunMode.Automatic:
                AutomaticFire();
                Pistol();
                break;
            case gunMode.Burst:
                break;
        }
    }

    private void AutomaticFire()
    {
        if (Input.GetMouseButton(0) && timer <= 0)
        {
            setTimer(rateOfFire);
            Fire();
        }
    }

    private void SingleFire()
    {
        if (Input.GetMouseButtonDown(0) && timer <= 0)
        {
            setTimer(rateOfFire);
            FireRayCast();
        }
    }

    private void Pistol()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            smgModel.SetActive(false);
            ModeSwitch(gunMode.SingleFire);
        }
    }

    private void Smg()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ModeSwitch(gunMode.Automatic);
            
        }
    }

    public void FireRayCast()
    {   
        Vector3 origin = muzzlePoint.position;
        Vector3 direction = muzzlePoint.forward;

        //Debug ray
        Debug.DrawRay(origin, direction * 100f, Color.red, 1f);
        //Sound of firing
        audioSource.clip = fireSound;
        audioSource.pitch = Random.Range(pitchRandomMin, pitchRandomMax);   
        audioSource.PlayOneShot(fireSound);
        // play muzzle flash
        vfxPlayer.PlayShootingPS();

        if (Physics.Raycast(origin, direction, out RaycastHit hit1, 5000, lm))
        {
            GameObject particles = Instantiate(hitEffectPrefab, hit1.point, Quaternion.identity);
            particles.transform.forward = hit1.normal;

            //Instantiate ray/laser
            LineRenderer lr = Instantiate(lineRendererPrefab);
            //set possition from where to where to stretch laser
            lr.SetPosition(0, origin);
            lr.SetPosition(1,hit1.point);
            //Destroys it after timer
            Destroy(lr.gameObject,rayDuration);
        }
        else
        {
            //Instantiate ray/laser
            LineRenderer lr = Instantiate(lineRendererPrefab);
            //set possition from where to where to stretch laser
            lr.SetPosition(0, origin);
            lr.SetPosition(1,origin  + direction * 100f);
            //Destroys it after timer
            Destroy(lr.gameObject, rayDuration);
        }
    }
}
