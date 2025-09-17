using UnityEngine;

public class ParticlesOnImpact : MonoBehaviour
{
    [Header("Sound Settings")]

    public float soundMaxDistance = 10f;
    public float soundMinDiastance = 1f;
    public float spatialBlend = 1f;


    public GameObject Particles;
    public AudioClip ImpactSound;


   
    private void OnTriggerEnter(Collider other)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).normalized;

        // Spawn particles
        GameObject particles = Instantiate(Particles, hitPoint, Quaternion.identity);
        particles.transform.forward = hitNormal;

        // Play audio at hit point
        if (ImpactSound != null)
        {
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = hitPoint;
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
