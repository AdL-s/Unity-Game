using System.Collections;
using UnityEngine;

public class SMGScript : MonoBehaviour
{
    [Header("Gun Settings")]
    public float rateOfFire = 300f;
    private float timer = 0f;
    public float damage = 10f;
    private bool ready = false;
    public float waitTime = 0.3f; // after switching to this weapon how many s you have to wait to fire

    [Header("Sound Settings")]
    public float pitchRandomMin = 0.7f;
    public float pitchRandomMax = 0.85f;
    public AudioClip fireSound;
    public AudioClip impactSound;
    public AudioSource audioSource;

    [Header("SMG Model and Muzzle")]
    public Transform muzzlePoint;

    [Header("Ray Settings")]
    public GameObject hitEffectPrefab;
    public LayerMask lm = (1 << 0) | (1 << 9);
    private LayerMask enemyLm = 9;

    [Header("VFX")]
    public ParticleSystem shootingPS;
    
    [Header("Animation")]
    public Animation animationComponent;
    public AnimationClip shootClip;

    void Start()
    {
        if (animationComponent != null && shootClip != null)
        {
            animationComponent.AddClip(shootClip, "SniperRifleAnimation");
            shootClip.legacy = true;
            shootClip.wrapMode = WrapMode.Once;
            shootClip.SampleAnimation(gameObject, 0f);
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        AutomaticFire();
    }

    private void AutomaticFire()
    {
        if (!ready) return;

        if (Input.GetMouseButton(0) && timer <= 0)
        {
            Shoot();
            timer = RayCastsScript.setTimer(rateOfFire);
        }
    }

    private void Shoot()
    {
        if (animationComponent.IsPlaying("SniperRifleAnimation"))
            animationComponent.Stop("SniperRifleAnimation");

        animationComponent.Play("SniperRifleAnimation");
        // Call the static utility function instead of duplicating raycast logic
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
       /* if (vfxPlayer != null)
            vfxPlayer.PlayShootingPS();*/
    }

    private void OnDisable()
    {
        timer = 0f;
    }

    private void OnEnable()
    {
        shootClip.SampleAnimation(gameObject, 0f);
        StartCoroutine(WaitCoroutine());
        StartCoroutine(WaitCoroutine());

    }

    private IEnumerator WaitCoroutine()
    {
        ready = false;
        yield return new WaitForSeconds(waitTime);
        ready = true;
    }
}
