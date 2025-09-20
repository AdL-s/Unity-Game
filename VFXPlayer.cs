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
        dashingPS.Clear();
        dashingPS.Play();
        
    }

    public void StopDashingPS()
    {
        Debug.Log("Stopped Playing Dash");
        dashingPS.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

}
