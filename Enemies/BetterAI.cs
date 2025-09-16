using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MelleeAI : MonoBehaviour
{
    public Transform Target;
    public float ADistance;
    public Collider AttackCol;

    private NavMeshAgent mAgent;
    private float m_Distance;
    [Range(1, 11)] public int LM = 11;

    private bool isAttacking = false;

    private Renderer triggerRend;
    private Color originalColor;
    private Color triggerColor = new Color(0, 0, 1, 0.5f);



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