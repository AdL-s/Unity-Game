using UnityEngine;

public class VFXPlayer : MonoBehaviour 
{   

    [SerializeField]
    private ParticleSystem dopadPS;
    [SerializeField]
    private ParticleSystem SlidingPS;
    [SerializeField]
    private ParticleSystem shootingPS;

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

}
