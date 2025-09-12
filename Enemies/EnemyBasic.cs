using UnityEngine;

public class EnemyBasic : MonoBehaviour
{   
    public float Hp = 100f;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void DamageRecivied(float damage)
    {
        Hp -= damage;

        if (Hp <= 0)
        {
            Death();
        }

    }

    private void Death()
    {
        Destroy(gameObject);    
    }
}
