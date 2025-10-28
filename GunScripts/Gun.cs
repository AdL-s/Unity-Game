
using UnityEngine;

public class Gun : MonoBehaviour 
{
    public float rateOfFire = 200;
    public int ammo = 1;
    public GameObject bullet;
    public GameObject hitEffectPrefab;
    public GameObject gunModel;
    // private int currentAmmo = 1;
    private CameraMovement cameraMovement;
    private float timer = 0f;
    
    public LayerMask lm;

    [SerializeField]


    public enum gunMode { SingleFire = 0, Automatic = 1, Burst = 2, Shotgun = 3 };
    public gunMode mode;


    private void Start()
    {
        cameraMovement = GetComponent<CameraMovement>();    
    }

    private void Update()
    {
        UpdateMode();
        timer -= Time.deltaTime;
    }

    private void setTimer()
    {
        timer = 60 / rateOfFire;
    }



    private void Fire()
    {

        GameObject NewBullet = Instantiate(bullet);
        NewBullet.transform.position = transform.position;
        NewBullet.transform.forward = cameraMovement.playerCamera.transform.forward;

       
    }

    private void ModeSwitch(gunMode newMode)
    {
        if (mode == newMode)
        {
            return;
        }

        mode = newMode;

        switch (mode)
        {
            case gunMode.SingleFire:
                rateOfFire = 80;
                
                break;
            case gunMode.Automatic:
                rateOfFire  = 300;
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
            setTimer(); 
            Fire();
        }
    }
    private void SingleFire()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            setTimer();
            FireRayCast();
        }
    }

    private void Pistol()
    {   
        if (Input.GetKeyDown(KeyCode.Q))
        {
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
    private void FireRayCast()
    {
        // Origin is the camera position (so you aim where you look)
        Vector3 origin = cameraMovement.playerCamera.transform.position;
        Vector3 direction = cameraMovement.playerCamera.transform.forward;

        // Cast ray


        //Debug.Log("Hit: " + hit.collider.name);
        Debug.DrawRay(origin, direction * 100f, Color.red, 1f);
        if (Physics.Raycast(origin, direction, out RaycastHit hit1, 5000, lm)) {

            GameObject particles = Instantiate(hitEffectPrefab, hit1.point,Quaternion.identity);
            particles.transform.forward = hit1.normal;

        }
    }

}
