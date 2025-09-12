using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BetterAI : MonoBehaviour
{
    public Transform Target;
    public float ADistance;
    public Collider AttackCol;

    private NavMeshAgent mAgent;
    private float m_Distance;
    [Range(1, 10)] public int LM = 6;

    void Start()
    {
        mAgent = GetComponent<NavMeshAgent>();

        if (AttackCol != null)
        {
            AttackCol.isTrigger = true;
        }
    }

    void Update()
    {
        if (Target == null) return;

        m_Distance = Vector3.Distance(mAgent.transform.position, Target.position);

        if (m_Distance < ADistance)
        {
            mAgent.isStopped = true;
        }
        else
        {
            mAgent.isStopped = false;
            mAgent.destination = Target.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer == LM)
        {

            PlayerBasic player = other.GetComponent<PlayerBasic>();
            player.DamageRecivied(50);

        }
    }

    private void Damage() 
    {
    
    }
}