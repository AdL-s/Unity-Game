using Unity.VisualScripting;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Revolver")]
    public GameObject revolverModel;
    public GameObject revolverMuzzlePoint;
    
    [Header("SMG")]
    public GameObject smgModel;
    public GameObject smgMuzzlePoint;
    
    [Header("RailGun")]
    public GameObject railModel;
    public GameObject railMuzzlePoint;

    [Header("Sniper Rifle")]
    public GameObject sniperRifleModel;
    public GameObject sniperRiflePoint;



    public enum gunMode { Revolver = 0, RailGun = 1, Smg = 2,SniperRifle = 3 };//add Sword with deflecting, maybe rocketLauncher
    public gunMode mode;

    private GunModelShooting GunModelShooting;

    void Start()
    {
        GunModelShooting = GetComponent<GunModelShooting>();

    }

    
    void Update()
    {
        Switcher();
    }
    public void ModeSwitch(gunMode newMode)
    {
        if (mode == newMode)
        { 
            return; 
        }
        gunMode oldMode = mode;

        switch(oldMode)
        {
            case gunMode.Revolver:
                revolverModel.SetActive(false);
                revolverMuzzlePoint.SetActive(false);
                break;

            case gunMode.RailGun:
                railModel.SetActive(false);
                railMuzzlePoint.SetActive(false);
                break;
            case gunMode.Smg:
                smgModel.SetActive(false);
                smgMuzzlePoint.SetActive(false);
                break;
            case gunMode.SniperRifle:
                sniperRifleModel.SetActive(false);
                sniperRiflePoint.SetActive(false);
                break;  
        }

        mode = newMode;

        switch (mode)
        {
            case gunMode.Revolver:
 
                
                revolverModel.SetActive(true);
                revolverMuzzlePoint.SetActive(true);

                break;
            case gunMode.RailGun:

                railModel.SetActive(true);
                railMuzzlePoint.SetActive(true);

                break;
            case gunMode.Smg:
                smgModel.SetActive(true);
                smgMuzzlePoint.SetActive(true);

                break;
            case gunMode.SniperRifle:
                sniperRifleModel.SetActive(true);
                sniperRiflePoint.SetActive(true);
                break;
            
        }
    }
    public void UpdateMode()
    {
        switch (mode)
        {
            case gunMode.Revolver:
                
                break;
            case gunMode.Smg:
                
                break;
            case gunMode.RailGun:
                break;
            case gunMode.SniperRifle:

                break;
        }
    }


    private void Switcher()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ModeSwitch(gunMode.Revolver);

        }else if (Input.GetKeyDown(KeyCode.E))
        {
            ModeSwitch(gunMode.Smg);

        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            ModeSwitch(gunMode.RailGun);
        }else if (Input.GetKeyDown(KeyCode.C))
        {
            ModeSwitch(gunMode.SniperRifle);
        }
    }
}
