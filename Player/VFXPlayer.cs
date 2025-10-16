using UnityEngine;

public class VFXPlayer : MonoBehaviour 
{   

    [SerializeField]
    private ParticleSystem dopadPS;
    [SerializeField]
    private ParticleSystem SlidingPS;
    [SerializeField]
    private ParticleSystem shootingPS;
    [SerializeField]
    private ParticleSystem dashingPS;

    public CameraMovement CM;

    public void PlayParticles()
    {
            dopadPS.Play();
        
    }

    public void SlidingParticles()
    {
        
        SlidingPS.Play();

    }
    public void PlayShootingPS()
    {
        shootingPS.Play();
    }

    public void StopSlidingParticles()
    {
        SlidingPS.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

    public void PlayDashingPS()
    {
        var main = dashingPS.main;
        dashingPS.Clear();
        if(CM.state == CameraMovement.PlayerStates.Dashing)
        {
            main.duration = 0.05f;
        }
        else
        {
            main.duration = 0.2f;
        }
           
        dashingPS.Play();
        
    }

    public void StopDashingPS()
    {
        dashingPS.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

}
