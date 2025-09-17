using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 50f;
    public float timer = 5f;
    public float damage = 20;

    private Rigidbody rb;

    private float startTime;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = transform.forward * speed;

        startTime = Time.time;  

        Destroy(gameObject, timer);
    }


    public void OnTriggerEnter(Collider other)
    {
        float travelTime = Time.time - startTime;
        Debug.Log($"Bullet travel time: {travelTime:F2} seconds");
        if (other.gameObject.tag == "Player")
        {

            PlayerBasic player = other.gameObject.GetComponent<PlayerBasic>();
            if (player != null) // Add null check for safety
            {
                player.DamageRecivied(damage);
            }
        }
        Destroy(gameObject);


    }
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

}
