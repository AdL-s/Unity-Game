using UnityEngine;

public class ParticlesOnImpact : MonoBehaviour
{
    [Header("Sound Settings")]

    public float soundMaxDistance = 10f;
    public float soundMinDiastance = 1f;
    public float spatialBlend = 1f;


    public GameObject Particles;
    public AudioClip ImpactSound;


    private void OnCollisionEnter(Collision collision)
    {
        // Spawn particles
        GameObject particles = Instantiate(Particles, collision.contacts[0].point, Quaternion.identity);
        particles.transform.forward = collision.contacts[0].normal;

        // Play audio at collision point
        if (ImpactSound != null)
        {
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = collision.contacts[0].point;
            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = ImpactSound;
            source.spatialBlend = spatialBlend; // 3D sound
            source.minDistance = soundMinDiastance;
            source.maxDistance = soundMaxDistance;
            source.Play();
            Destroy(tempAudio, ImpactSound.length);
        }
    }
}
