using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShootingAI : MonoBehaviour
{
    public float ADistance;// Distance of melee attack 
    private float m_Distance; //Distance from player

    [Range(1, 11)] public int LM = 11; //Level of layerMask the collider should react to
    public float shotDistance;// Distance of shooting(when the ai starts shooting)
    public Transform Target; // Player
    public Collider AttackCol; //Trigger of the attack / collider
    public float spawndelay = 10f;
    public float rotationSpeed = 5f; // Speed of rotation when looking at player
    
    private NavMeshAgent mAgent; //The enemy model
    private bool isAttacking = false;
    private float timer;

    public GameObject player;
    public LayerMask LayerMask = (1 << 0) | (1 << 9);
    [SerializeField] public GameObject bullet;
    [SerializeField] public Transform muzzlePoint;//where the bullets come from
    
    private Renderer triggerRend;
    private Color originalColor; //Color of trigger
    private Color triggerColor = new Color(0, 0, 1, 0.5f); // Color of triggered trigger

    void Start()
    {
        mAgent = GetComponent<NavMeshAgent>();
        triggerRend = AttackCol.GetComponent<Renderer>();
        originalColor = triggerRend.material.color;
        AttackCol.enabled = false;
        if (AttackCol != null)
        {
            AttackCol.isTrigger = true;
        }
    }

    void Update()
    {
        //logic for when ray does hit wall it moves where he can see player without problem
        RaycastHit hit;
        if (Physics.Raycast(transform.position, player.transform.position, out hit,LayerMask))
        {   
            Debug.DrawRay(transform.position, hit.point);
            mAgent.updateRotation = true;
            mAgent.isStopped = false;
            mAgent.destination = Target.position;
            return;
        } 

        if (Target == null) return;

        timer += Time.deltaTime;
        m_Distance = Vector3.Distance(mAgent.transform.position, Target.position);

        if (m_Distance > shotDistance)
        {
            // Add custom rotation to look at player
            LookAtTarget();

            Shooting();

        }
        else
        {
           if(mAgent.pathStatus == NavMeshPathStatus.PathPartial)
           {
                LookAtTarget();
                Shooting();
           }
            mAgent.updateRotation = true; // Re-enable NavMeshAgent rotation for movement
            mAgent.isStopped = false;
            mAgent.destination = Target.position;

            if (m_Distance < ADistance && !isAttacking)
            {
                StartCoroutine(Delay());
            }
            else if (!isAttacking)
            {
                mAgent.isStopped = false;
                mAgent.destination = Target.position;
            }
        }
    }

    private void LookAtTarget()
    {
        // Calculate direction to target
        Vector3 direction = (Target.position - transform.position).normalized;

        // Remove Y component to only rotate on Y axis (optional - remove this line if you want full 3D rotation)
        direction.y = 0;

        // Only rotate if there's a valid direction
        if (direction != Vector3.zero)
        {
            // Create rotation towards target
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                                                rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LM)
        {
            triggerRend.material.color = triggerColor;
            PlayerBasic player = other.GetComponent<PlayerBasic>();
            if (player != null) // Add null check for safety
            {
                player.DamageRecivied(50);
            }
        }
    }

    private void Damage()
    {

    }

    private void Shooting()
    {
        mAgent.isStopped = true;      // stops agent movement
        mAgent.updateRotation = false; // disable NavMeshAgent rotation

        if (timer >= spawndelay)
        {
            // Calculate direction from muzzle to target
            Vector3 directionToTarget = (Target.position - muzzlePoint.position).normalized;

            // Create rotation that points toward target
            Quaternion shootRotation = Quaternion.LookRotation(directionToTarget);

            // Calculate speed BEFORE instantiating
            float bulletSpeed = m_Distance < 23 ? 50f : m_Distance * 1.25f;

            // Spawn bullet with rotation toward target
            GameObject newBullet = Instantiate(bullet, muzzlePoint.position, shootRotation);

            // Disable the bullet temporarily to set speed before Start() runs
            newBullet.SetActive(false);

            Projectile projectile = newBullet.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetSpeed(bulletSpeed);
            }

            // Re-enable the bullet so Start() runs with correct speed
            newBullet.SetActive(true);

            timer = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LM)
        {
            triggerRend.material.color = originalColor;
        }
    }

    private void OnDisable()
    {
        triggerRend.material.color = originalColor;
    }

    private IEnumerator Delay()
    {
        isAttacking = true;
        mAgent.isStopped = true;
        yield return new WaitForSeconds(1);
        AttackCol.enabled = true;
        yield return new WaitForSeconds(3);
        AttackCol.enabled = false;
        mAgent.isStopped = false;
        mAgent.destination = Target.position;
        isAttacking = false;
    }
}