using System.Collections;
using UnityEngine;

public class RevolverScript : MonoBehaviour
{
    [Header("Gun Settings")]
    public float rateOfFire = 90f;
    private float timer = 0f;
    public float damage = 50f;
    private bool ready = false; // tells if waitTime is over
    public float waitTime = 0.5f; // after switching to this weapon how many s you have to wait to fire

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;
    public AudioClip fireSound;
    public AudioClip impactSound;
    public AudioSource audioSource;


    [Header("Revolver Model and Muzzle")]
    public Transform muzzlePoint;
    
    [Header("VFX")]
    public ParticleSystem shootingPS;

    [Header("Other Settings")]
    public GameObject hitEffectPrefab;
    public LayerMask lm = (1 << 0) | (1 << 9);
    private LayerMask enemyLm = 9;


    void Start()
    {

    }

    void Update()
    {
        timer -= Time.deltaTime;
        HandleInput();
    }

    private void HandleInput()
    {
        if (!ready) return;

        if (Input.GetMouseButtonDown(0) && timer <= 0)
        {
            Shoot();
            timer = RayCastsScript.setTimer(rateOfFire);
        }
    }

    private void Shoot()
    {
        RayCastsScript.FireRayCast(
            audioSource, fireSound,
            pitchRandomMin, pitchRandomMax,
            hitEffectPrefab,
            lm, enemyLm,
            shootingPS,
            damage,
            impactSound
        );

        // Muzzle flash
        //shootingPS.Play();
    }

    private void OnDisable()
    {
        timer = 0f;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitCoroutine());
    }

    private IEnumerator WaitCoroutine()
    {
        ready = false;
        yield return new WaitForSeconds(waitTime);
        ready = true;
    }
}
