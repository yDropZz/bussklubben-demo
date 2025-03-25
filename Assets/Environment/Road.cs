using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
[SerializeField] private GameObject[] vechilePrefabs;
    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float maxSpawnTime = 5f;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float spawnRadius = 50f;
    private Transform player;

    public Transform spawnPoint;
    public Transform endPoint;
    public float spawnTimer;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(SpawnVechiles());
        

    }

    private IEnumerator SpawnVechiles()
    {
        while(true)
        {
            float distanceToPlayer = Vector3.Distance(player.position, spawnPoint.position);
            if(distanceToPlayer < spawnRadius)
            {
                
                float spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
                spawnTimer = spawnTime;
                yield return new WaitForSeconds(spawnTime);
                yield return new WaitForSeconds(spawnTimer);

                int vechileIndex = Random.Range(0, vechilePrefabs.Length);
                GameObject vechile = Instantiate(vechilePrefabs[vechileIndex], spawnPoint.position, Quaternion.identity);
                vechile.transform.LookAt(endPoint.position);
                float speed = Random.Range(minSpeed, maxSpeed);
                vechile.GetComponent<Enemy>().Initialize(endPoint, speed);
            }
            else
            {
                yield return null;
            }

        }
    }
}
