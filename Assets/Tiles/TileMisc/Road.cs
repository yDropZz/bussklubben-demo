using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
[SerializeField] private string[] vechileTags;
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

    [Header("Bird stuff")]
    [SerializeField] string[] birdTags;
    [SerializeField] GameObject[] birdPrefabs;
    [SerializeField] Transform[] birdSpawnPoints;
    [SerializeField] float birdSpawnRadius = 50f;
    [SerializeField] float birdSpawnTimeMin = 5f;
    [SerializeField] float birdSpawnTimeMax = 10f;
    [SerializeField] float birdSpeedMin = 20f;
    [SerializeField] float birdSpeedMax = 70f;
    [SerializeField] float birdCheckRadius = 15f;

    [SerializeField] Transform birdEndPoint;


    private bool firstSpawn = true;
    private bool firstBirdSpawn = true;
    float birdSpawnTime = 1f;
    ObjectPool objectPool;

    void Awake()
    {
        objectPool = FindAnyObjectByType<ObjectPool>();
        if(objectPool == null)
        {
            Debug.LogError("No object pool found in the scene. Please add one.");
        }
    }




    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if(warningLight != null)
        {
            warningLight.enabled = false;
        }

        StartCoroutine(SpawnVechiles());
        StartCoroutine(SpawnBirds());
        

    }

    void Update()
    {

    }

    private IEnumerator SpawnBirds()
    {
        while(true)
        {
            float distanceToPlayer = Vector3.Distance(player.position, spawnPoints[0].position);
            if(distanceToPlayer < spawnRadius && transform.position.z > player.transform.position.z - 30f)
            {
                float birdSpawnTime = Random.Range(birdSpawnTimeMin, birdSpawnTimeMax);
                if(firstBirdSpawn)
                {
                    firstBirdSpawn = false;
                }

                yield return new WaitForSeconds(birdSpawnTime);

                // Making sure there actually are spawnPoints assigned
                // If not, we just return and don't spawn anything.
                if(birdSpawnPoints.Length == 0)
                {
                    Debug.LogWarning("No spawn points assigned. This message can just be ignored tbh");
                    yield break;
                }

                // Roll a dice to see where we spawn the car.
                int birdSpawnPointIndex = Random.Range(0, birdSpawnPoints.Length);
                Transform selectedSpawnPoint = birdSpawnPoints[birdSpawnPointIndex];

                // Check if the spawn point is clear of other cars
                if(IsSpawnPointClear(selectedSpawnPoint.position, birdCheckRadius))
                {
                    int birdIndex = Random.Range(0, birdPrefabs.Length);
                    GameObject bird = objectPool.GetObject(birdTags[birdIndex], selectedSpawnPoint.position, Quaternion.identity);
                    bird.transform.LookAt(birdEndPoint.position);
                    float speed = Random.Range(birdSpeedMin, birdSpeedMax);
                    float scaledSpeed = ScaleVechileSpeed(speed);
                    bird.GetComponent<Enemy>().Initialize(birdEndPoint, scaledSpeed, selectedSpawnPoint.position);

                    
                }
            }
            else
            {
                yield return null;
            }
        }
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

                // Making sure there actually are spawnPoints assigned
                // If not, we just return and don't spawn anything.
                if(spawnPoints.Length == 0)
                {
                    Debug.LogWarning("No spawn points assigned. This message can just be ignored tbh");
                    yield break;
                }

                // Roll a dice to see where we spawn the car.
                int spawnPointIndex = Random.Range(0, spawnPoints.Length);
                Transform selectedSpawnPoint = spawnPoints[spawnPointIndex];

                // Check if the spawn point is clear of other cars
                if(IsSpawnPointClear(selectedSpawnPoint.position, checkRadius))
                {
                    int vechileIndex = Random.Range(0, vechileTags.Length);
                    GameObject vechile = objectPool.GetObject(vechileTags[vechileIndex], spawnPoints[spawnPointIndex].position, Quaternion.identity);
                    vechile.transform.LookAt(endPoint.position);
                    float speed = Random.Range(minSpeed, maxSpeed);
                    float scaledSpeed = ScaleVechileSpeed(speed);
                    vechile.GetComponent<Enemy>().Initialize(endPoint, scaledSpeed, selectedSpawnPoint.position);

                    
                }
            }
            else
            {
                yield return null;
            }

        }
    }

    float ScaleVechileSpeed(float speed)
    {
        // Temporary scaling simply based on player position.
        carSpeedScaling = (player.position.z / 1000f) + 1f;
        float scaledSpeed = speed * carSpeedScaling;
        if(scaledSpeed > maxSpeed)
        {
            scaledSpeed = maxSpeed;
        }
        return scaledSpeed;
    }

    bool IsSpawnPointClear(Vector3 spawnPosition, float checkRadius)
    {
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, checkRadius, enemyLayer);

        // Return true if no colliders are within the sphere.
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
