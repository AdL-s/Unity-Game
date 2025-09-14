using UnityEngine;
using UnityEngine.UI;

public class PlayerBasic : MonoBehaviour
{   
    
    public float MaxHp = 100f;

    public float currentHealth;
    public Image healthBarFill;
    
    void Start()
    {
        currentHealth = MaxHp;
        UpdateHealthBar();
    }

 
    void Update()
    {
        
    }
    public void DamageRecivied(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Death();
        }

        UpdateHealthBar();
        Debug.Log("Took damage");
    }
    private void Death()
    {
        transform.position = new Vector3(2.556f, 2.998f, 0.966f);
        Debug.Log("Death");
        currentHealth = 100;
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / MaxHp;
        }
    }
}
