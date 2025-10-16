using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class RayCastsScript : MonoBehaviour
{
    // not comenting this shit
    public static void FireRayCastLineRay(
    Transform muzzlePoint, LayerMask lm, LayerMask enemyLm,
    GameObject hitEffectPrefab,
    LineRenderer lineRendererPrefab, float rayDuration,
    AudioSource audioSource, AudioClip fireSound,
    float pitchRandomMin, float pitchRandomMax,
    ParticleSystem shootingPS, float damage,
    AudioClip impactSound, float spatialBlend = 1f, float soundMinDistance = 1f, float soundMaxDistance = 50f)
    {
        // Start of the laser (muzzle)
        Vector3 origin = muzzlePoint.position;
        // Ray from the center of the screen (crosshair)
        Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        // The point we want to shoot at
        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out hit, 5000, lm))
        {
            targetPoint = hit.point;
            // Spawn hit particles
            GameObject particles = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
            particles.transform.forward = hit.normal;

            // Play impact audio at hit point
            if (impactSound != null)
            {
                GameObject tempAudio = new GameObject("TempAudio");
                tempAudio.transform.position = hit.point;
                AudioSource source = tempAudio.AddComponent<AudioSource>();
                source.clip = impactSound;
                source.spatialBlend = spatialBlend; // 3D sound
                source.minDistance = soundMinDistance;
                source.maxDistance = soundMaxDistance;
                source.Play();
                Destroy(tempAudio, impactSound.length);
            }

            if (hit.collider.gameObject.layer == enemyLm)
            {
                EnemyBasic enemyBasic = hit.collider.GetComponent<EnemyBasic>();
                enemyBasic.DamageRecivied(damage);
            }
        }
        else
        {
            // If nothing hit, shoot straight far away
            targetPoint = camRay.origin + camRay.direction * 1000f;
        }

        // Instantiate laser beam
        LineRenderer lr = Instantiate(lineRendererPrefab);
        lr.SetPosition(0, origin);       // start from muzzle
        lr.SetPosition(1, targetPoint);  // end at crosshair target
        lr.GetComponent<LineFade>().Init(rayDuration);

        // Play sound
        audioSource.pitch = Random.Range(pitchRandomMin, pitchRandomMax);
        audioSource.PlayOneShot(fireSound);

        // Play muzzle flash
        shootingPS.Play();

        // Debug line
        Debug.DrawLine(origin, targetPoint, Color.red, 1f);
    }

    // pretend that this doesnt exist
    public static void FireRayCast(
        AudioSource audioSource, AudioClip fireSound, float pitchRandomMin, float pitchRandomMax,
        GameObject hitEffectPrefab, LayerMask lm, LayerMask enemyLm,
        ParticleSystem shootingsPS,
        float damage,
        AudioClip impactSound, float spatialBlend = 1f, float soundMinDistance = 1f, float soundMaxDistance = 50f)
    {
        // Get a ray from the center of the screen (crosshair)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Play sound
        audioSource.clip = fireSound;
        audioSource.pitch = Random.Range(pitchRandomMin, pitchRandomMax);
        audioSource.PlayOneShot(fireSound);

        if (shootingsPS != null)
        {
            shootingsPS.Play();
        }

        // Check for hit
        if (Physics.Raycast(ray, out RaycastHit hit, 5000, lm))
        {
            // Spawn hit effect
            GameObject particles = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
            particles.transform.forward = hit.normal;

            // Play impact audio at hit point
            if (impactSound != null)
            {
                GameObject tempAudio = new GameObject("TempAudio");
                tempAudio.transform.position = hit.point;
                AudioSource source = tempAudio.AddComponent<AudioSource>();
                source.clip = impactSound;
                source.spatialBlend = spatialBlend; // 3D sound
                source.minDistance = soundMinDistance;
                source.maxDistance = soundMaxDistance;
                source.Play();
                Destroy(tempAudio, impactSound.length);
            }

            if (hit.collider.gameObject.layer == enemyLm)
            {
                EnemyBasic enemyBasic = hit.collider.GetComponent<EnemyBasic>();
                enemyBasic.DamageRecivied(damage);
            }
        }
    }
    public static float setTimer(float rateoffFire)
    {
        return 60f / rateoffFire;
    }
   

}
