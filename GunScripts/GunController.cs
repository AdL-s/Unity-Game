using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Revolver")]
    public GameObject revolverModel;
    
    [Header("SMG")]
    public GameObject smgModel;
    
    [Header("RailGun")]
    public GameObject railModel;

    [Header("Sniper Rifle")]
    public GameObject sniperRifleModel;



    public enum gunMode { Revolver = 0, RailGun = 1, Smg = 2,SniperRifle = 3 };//add Sword with deflecting, maybe rocketLauncher
    public gunMode mode;


    void Start()
    {
        mode = gunMode.Revolver;
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
                break;

            case gunMode.RailGun:
                railModel.SetActive(false);
                break;
            case gunMode.Smg:
                smgModel.SetActive(false);
                break;
            case gunMode.SniperRifle:
                sniperRifleModel.SetActive(false);
                break;  
        }

        mode = newMode;

        switch (mode)
        {
            case gunMode.Revolver:
 
                
                revolverModel.SetActive(true);

                break;
            case gunMode.RailGun:

                railModel.SetActive(true);

                break;
            case gunMode.Smg:
                smgModel.SetActive(true);              

                break;
            case gunMode.SniperRifle:
                sniperRifleModel.SetActive(true);
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
