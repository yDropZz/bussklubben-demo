using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [Header("Individual Car Settings")]
    [SerializeField] private string carTag;
    [SerializeField] private int distanceToDespawn = 200;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private Vector3 raycastOffset = new Vector3(0, 2.5f, 0);

    [Header("Sounds")]
    [SerializeField] private AudioClip[] honkSounds;

    [Header("Train")]
    [SerializeField] private bool isTrain = false;
    public bool IsTrain { get {return isTrain;}}

    [Header("Bird")]
    [SerializeField] private bool isBird = false;
    public bool IsBird { get {return isBird;}}

    Player player;
    private Transform targetPoint;
    private float speed;
    public float Speed 
    { 
        get { return speed; }
        set { speed = value; }
    
    }

    Collider coll;
    ObjectPool objectPool;

    //Bool to check if the car is driving freely or with limited speed (determined by car in front)
    private bool limitedSpeed = false;

    public void Initialize(Transform target, float moveSpeed, Vector3 spawnPosition)
    {
        targetPoint = target;
        speed = moveSpeed;

        transform.position = spawnPosition;

        limitedSpeed = false;
    }

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
        objectPool = FindAnyObjectByType<ObjectPool>();
    }

    void OnDisable()
    {
        // Full vechile reset
        speed = 0;

        limitedSpeed = false;

        targetPoint = null;

        if(coll == null)
        {
            coll = GetComponent<Collider>();
            coll.enabled = true;
            coll.isTrigger = true;
        }
        else
        {
            coll.enabled = true;
            coll.isTrigger = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null)
        {
            Destroy(rb);
        }

        transform.rotation = Quaternion.identity;
        
    }


    void Update()
    {   

        if(limitedSpeed == false)
        Raycast();

        if(targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

            if(Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                objectPool.ReturnObject(carTag, gameObject);
            }
        }

        if(Vector3.Distance(transform.position, player.transform.position) > distanceToDespawn)
        {
            objectPool.ReturnObject(carTag, gameObject);
        }
        else if(transform.position.z < player.transform.position.z - 50f)
        {
            objectPool.ReturnObject(carTag, gameObject);
        }
    }

    private void Raycast()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + raycastOffset, transform.forward, out hit, raycastDistance))
        {
            // Get component from car in front of you
            if(hit.collider.TryGetComponent<Enemy>(out Enemy carInFront))
            {
                speed = carInFront.Speed;
                limitedSpeed = true;

            } 
            else if(hit.collider.CompareTag("Player"))
            {
                if(SoundManager.Instance.HonkSoundIsPlaying == false)
                {
                    SoundManager.Instance.PlaySoundEffect(honkSounds[Random.Range(0, honkSounds.Length)]);
                    SoundManager.Instance.HonkSoundIsPlaying = true;
                    Invoke("DisableHonkSoundBool", 1f);
                    if(hit.collider.TryGetComponent<Player>(out Player player))
                    {
                        if(player.Dead == true)
                        {
                            speed = 0;
                        }
                    }
                }
                //Debug.Log("Honk!");

            }
        }
    }

    void DisableHonkSoundBool()
    {
        SoundManager.Instance.HonkSoundIsPlaying = false;
    }

    public void TurnOffCollider()
    {
        coll = GetComponent<Collider>();
        coll.enabled = false;
    }

}
