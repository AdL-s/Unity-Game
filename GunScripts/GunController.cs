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

    public enum gunMode { Revolver = 0, RailGun = 1, Smg = 2, };//add Sword with deflecting, maybe rocketLauncher
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

        mode = newMode;

        switch (mode)
        {
            case gunMode.Revolver:
               
                SMGDeActiveate();

                RailGunDeActivate();

                RevolverActivate();


                break;
            case gunMode.RailGun:
                
                RevolverDeActivate();

                SMGDeActiveate();

                RailGunActivate();

                break;
            case gunMode.Smg:

                RevolverDeActivate();

                RailGunDeActivate();

                SMGActivate();
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
        }
    }

    private void RevolverActivate()
    {
        revolverModel.SetActive(true);
        revolverMuzzlePoint.SetActive(true);
    }

    private void RevolverDeActivate()
    {
        revolverModel.SetActive(false);
        revolverMuzzlePoint.SetActive(false);
    }

    private void SMGActivate()
    {
        smgModel.SetActive(true);
        smgMuzzlePoint.SetActive(true);
    }

    private void SMGDeActiveate()
    {
        smgModel.SetActive(false);
        smgMuzzlePoint.SetActive(false);
    }

    private void RailGunActivate()
    {
        railModel.SetActive(true);
        railMuzzlePoint.SetActive(true);
    }
    private void RailGunDeActivate()
    {
        railModel.SetActive(false);
        railMuzzlePoint.SetActive(false);
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
        }
    }
}
