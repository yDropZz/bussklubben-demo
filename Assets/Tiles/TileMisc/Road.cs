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
    [SerializeField] private float carSpeedScaling = .5f;
    [SerializeField] private float checkRadius = 15f;


    private Transform player;


    public Transform[] spawnPoints;
    public Transform endPoint;
    public float spawnTimer;
    public float spawnTime = 5f;

    [Header("Railroad stuff")]
    [SerializeField] bool isRail = false;
    [SerializeField] bool trainComing = false;
    [SerializeField] float trainWarningTime = 3f;
    [SerializeField] private Light warningLight;
    [SerializeField] AudioClip trainSound;

    private bool firstSpawn = true;
    



    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if(warningLight != null)
        {
            warningLight.enabled = false;
        }

        StartCoroutine(SpawnVechiles());
        

    }

    void Update()
    {

    }

    private IEnumerator SpawnVechiles()
    {
        while(true)
        {
            float distanceToPlayer = Vector3.Distance(player.position, spawnPoints[0].position);
            if(distanceToPlayer < spawnRadius && transform.position.z > player.transform.position.z - 30f)
            {
                
                float spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
                if(firstSpawn)
                {
                    spawnTime = 0f;
                    firstSpawn = false;
                }

                yield return new WaitForSeconds(spawnTime);

                if(isRail)
                {
                    trainComing = true;
                    StartCoroutine(BlinkLight());
                    yield return new WaitForSeconds(trainWarningTime);

                    trainComing = false;

                    //Time to calculate train volume
                    float maxDistance = spawnRadius; // max sound distance
                    float distance = Vector3.Distance(player.position, transform.position);
                    float volume = Mathf.Clamp01(1 - (distance / maxDistance));
                    Debug.Log("Volume of train: " + volume);

                    SoundManager.Instance.PlaySoundEffect(trainSound, volume);
                }

                // Roll a dice to see where we spawn the car.
                int spawnPointIndex = Random.Range(0, spawnPoints.Length);
                Transform selectedSpawnPoint = spawnPoints[spawnPointIndex];

                // Check if the spawn point is clear of other cars
                if(IsSpawnPointClear(selectedSpawnPoint.position, checkRadius))
                {
                    int vechileIndex = Random.Range(0, vechilePrefabs.Length);
                    GameObject vechile = Instantiate(vechilePrefabs[vechileIndex], spawnPoints[spawnPointIndex].position, Quaternion.identity);
                    vechile.transform.LookAt(endPoint.position);
                    float speed = Random.Range(minSpeed, maxSpeed);
                    vechile.GetComponent<Enemy>().Initialize(endPoint, speed);
                }
            }
            else
            {
                yield return null;
            }

        }
    }

    bool IsSpawnPointClear(Vector3 spawnPosition, float checkRadius)
    {
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, checkRadius, enemyLayer);

        return colliders.Length == 0;
    }

    IEnumerator BlinkLight()
    {
        if(warningLight == null) yield break;

        while(trainComing)
        {
            warningLight.enabled = !warningLight.enabled;
            yield return new WaitForSeconds(.5f);
        }
        warningLight.enabled = false;
    }
}
