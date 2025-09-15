using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BetterAI : MonoBehaviour
{
    public float ADistance;//Distance from player
    private float m_Distance;// Distance of melee attack
    [Range(1, 10)] public int LM = 6; //Level of layerMask the collider should react to
    public float shotDistance;// Distance of shooting(when the ai starts shooting)

    public Transform Target; // Player
    public Collider AttackCol; //Trigger of the attack / collider

    private NavMeshAgent mAgent; //The enemy model

    private bool isAttacking = false;

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
        if (Target == null) return;

        m_Distance = Vector3.Distance(mAgent.transform.position, Target.position);

        if (m_Distance > shotDistance)
        {
            mAgent.velocity = Vector3.zero;
            mAgent.updateRotation = true;
            GameObject newBullet = Instantiate(bullet, muzzlePoint.position, muzzlePoint.rotation); // need to fix this (this in an update is not a great idea)
            newBullet.transform.forward = muzzlePoint.forward;

        }
        else
        {
            mAgent.updatePosition = true;

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

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer == LM)
        {
            triggerRend.material.color = triggerColor;
            PlayerBasic player = other.GetComponent<PlayerBasic>();
            player.DamageRecivied(50);

        }
    }

    private void Damage() 
    {
    
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