using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject[] vechilePrefabs;
    [SerializeField] private GameObject[] roads;
    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float maxSpawnTime = 5f;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float spawnRadius = 50f;
    private Transform player;


    // Start is called before the first frame update
    void Start()
    {
        foreach(var road in roads)
        {
            StartCoroutine(SpawnVechiles(road.GetComponent<Road>()));
        }

    }

    private IEnumerator SpawnVechiles(Road road)
    {
        while(true)
        {
            float distanceToPlayer = Vector3.Distance(player.position, road.spawnPoint.position);
            if(distanceToPlayer < spawnRadius)
            {
                float spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
                road.spawnTimer = spawnTime;
                yield return new WaitForSeconds(spawnTime);
                yield return new WaitForSeconds(road.spawnTimer);

                int vechileIndex = Random.Range(0, vechilePrefabs.Length);
                GameObject vechile = Instantiate(vechilePrefabs[vechileIndex], road.spawnPoint.position, Quaternion.identity);
                vechile.transform.LookAt(road.endPoint.position);
                float speed = Random.Range(minSpeed, maxSpeed);
                vechile.GetComponent<Enemy>().Initialize(road.endPoint, speed);
            }
            else
            {
                yield return null;
            }

        }
    }

}
