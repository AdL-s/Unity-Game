using System.Collections;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{

    public GameObject enemy;
    public GameObject player;
    public float travelSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, player.transform.position, travelSpeed);
        
    }
}
