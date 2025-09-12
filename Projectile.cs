using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 50f;
    public float timer = 5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, timer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
