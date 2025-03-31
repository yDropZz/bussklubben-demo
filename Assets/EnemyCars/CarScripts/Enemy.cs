using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private int distanceToDespawn = 200;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private Vector3 raycastOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private AudioClip[] honkSounds;
    [SerializeField] private bool isTrain = false;
    public bool IsTrain { get {return isTrain;}}
    Player player;
    private Transform targetPoint;
    private float speed;
    public float Speed 
    { 
        get { return speed; }
        set { speed = value; }
    
    }

    Collider coll;

    //Bool to check if the car is driving freely or with limited speed (determined by car in front)
    private bool limitedSpeed = false;

    public void Initialize(Transform target, float moveSpeed)
    {
        targetPoint = target;
        speed = moveSpeed;
    }

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + raycastOffset, transform.forward * raycastDistance);
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
                Destroy(gameObject);
            }
        }

        if(Vector3.Distance(transform.position, player.transform.position) > distanceToDespawn)
        {
            Destroy(gameObject);
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
