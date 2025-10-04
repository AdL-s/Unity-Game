using System.Collections;
using UnityEngine;

public class RailGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float rateOfFire = 200f;
    private float timer = 0f;
    public float damage = 70f;
    public float waitTime = 0.5f; // after switching to this weapon how many s you have to wait to fire
    private bool ready = false; // tells if waitTime is over 

    [Header("Sound Settings")]
    [SerializeField] public float pitchRandomMin = 0.7f;
    [SerializeField] public float pitchRandomMax = 0.85f;
    public AudioClip fireSound;
    public AudioSource audioSource;

    [Header("Ray Settings")]
    public float rayDuration = 0.5f;
    public LineRenderer lineRendererPrefab;

    [Header("VFX")]
    public ParticleSystem shootingPS;

    [Header("Other Settings")]
    public Transform muzzlePoint;
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
        RayCastsScript.FireRayCastLineRay(
            muzzlePoint, lm, enemyLm,
            hitEffectPrefab,
            lineRendererPrefab, rayDuration,
            audioSource, fireSound,
            pitchRandomMin, pitchRandomMax,
            shootingPS, 
            damage
        );
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
