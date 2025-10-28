using System.Collections;
using UnityEngine;

public class RevolverScript : MonoBehaviour
{
    [Header("Gun Settings")]
    public float rateOfFire = 90f;
    private float timer = 0f;
    public float damage = 50f;
    private bool ready = false;
    public float waitTime = 0.5f;

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;
    public AudioClip fireSound;
    public AudioClip impactSound;
    public AudioSource audioSource;

    [Header("Animations")]
    public Animation animationComponent; 
    public AnimationClip shootClip;      

    [Header("Revolver Model and Muzzle")]
    public Transform muzzlePoint;

    [Header("VFX")]
    public ParticleSystem shootingPS;

    [Header("Base Position & Rotation")]
    public Vector3 basePosition = Vector3.zero; // Set the gun's base position here

    [Header("Other Settings")]
    public GameObject hitEffectPrefab;
    public LayerMask lm = (1 << 0) | (1 << 9);
    private LayerMask enemyLm = 9;

    void Start()
    {
        if (animationComponent != null && shootClip != null)
        {
            shootClip.SampleAnimation(gameObject, 0f);
            animationComponent.AddClip(shootClip, "Revolver Animation");
            shootClip.legacy = true;
            shootClip.wrapMode = WrapMode.Once;
        }
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
        // Play legacy animation
          
         if (animationComponent.IsPlaying("RevolverAnimation"))
                   animationComponent.Stop("RevolverAnimation");

                animationComponent.Play("RevolverAnimation");
            

        RayCastsScript.FireRayCast(
            audioSource, fireSound,
            pitchRandomMin, pitchRandomMax,
            hitEffectPrefab,
            lm, enemyLm,
            shootingPS,
            damage,
            impactSound
        );
    }

    private void OnDisable() 
    { 
        timer = 0f;
    }
   /* void Awake() 
    {
        if (animationComponent != null)
        {
            animationComponent.Stop();
        }
    }*/

    private void OnEnable() 
    {
        shootClip.SampleAnimation(gameObject, 0f);
        StartCoroutine(WaitCoroutine()); 
    }

    private IEnumerator WaitCoroutine()
    {
        ready = false;
        yield return new WaitForSeconds(waitTime);
        ready = true;
    }
}