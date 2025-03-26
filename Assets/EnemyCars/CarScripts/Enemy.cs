using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Player player;
    private Transform targetPoint;
    private float speed;

    public void Initialize(Transform target, float moveSpeed)
    {
        targetPoint = target;
        speed = moveSpeed;
    }

    void Awake()
    {
        player = FindObjectOfType<Player>();
    } 

    void Update()
    {
        if(targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

            if(Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                Destroy(gameObject);
            }
        }

        if(Vector3.Distance(transform.position, player.transform.position) > 100)
        {
            Destroy(gameObject);
        }
    }
}
