using System.Collections;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{

    public GameObject enemy;
    public Transform Target; // Player's transform
    public float travelSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the player by tag at runtime
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            Target = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player object is tagged with 'Player'.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Target == null) return; // Don't move if no target

        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, Target.position, travelSpeed);
        
    }
}
