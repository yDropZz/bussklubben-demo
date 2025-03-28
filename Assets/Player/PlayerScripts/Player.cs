using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementTime = .15f;
    [SerializeField] private float bounceMultiplier = .1f;
    [SerializeField] private Rigidbody rb;


    [Header("Sound")]
    [SerializeField] AudioClip[] jumpSounds;
    [SerializeField] AudioClip crashSound;
    Vector3 targetPosition;
    bool isMoving = false;
    float startTime;
    public Transform playerPos;
    GameManager gameManager;
    [SerializeField] GameObject crashParticles;


    bool isDead = false;
    public bool Dead { get {return isDead;} }

    bool hasBufferedInput = false;
    UIManager uiManager;

    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        uiManager = FindAnyObjectByType<UIManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (!isMoving)
            {
                // If not moving, execute the movement immediately
                MovePlayer();
            }
            else
            {
                // If already moving, buffer the input
                hasBufferedInput = true;
            }
        }

        if(isMoving)
        {
            float t = (Time.time - startTime) / movementTime;
            float bounce = Mathf.Sin(t * Mathf.PI) * bounceMultiplier;
            transform.position = Vector3.Lerp(transform.position, targetPosition, t) + new Vector3(0, bounce, 0);

            if(t >= 1)
            {
                isMoving = false;
                transform.position = targetPosition;

                if(hasBufferedInput)
                {
                    hasBufferedInput = false;
                    MovePlayer();
                }
            }
        }
    }

    void MovePlayer()
    {
        targetPosition += new Vector3(0, 0, 10);
        startTime = Time.time;
        isMoving = true;
        gameManager.CalculateScore();

        // Play a random jump sound
        SoundManager.Instance.PlaySoundEffect(jumpSounds[Random.Range(0, jumpSounds.Length)]);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            if(other.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.GetComponent<Collider>().isTrigger = false;

                // Particles
                crashParticles.SetActive(true);

                Vector3 collisionPoint = (transform.position + other.transform.position) / 2;
                GameObject sparks = Instantiate(crashParticles, collisionPoint, Quaternion.identity);
                Destroy(sparks, 2f);

                // clear all movement and buffer b4 applying force
                isMoving = false;
                hasBufferedInput = false;
                targetPosition = transform.position;

                // Camera change
                Camera.main.GetComponent<CameraFollower>().SmoothTime = .05f;

                // Crash sound
                SoundManager.Instance.PlaySoundEffect(crashSound);

                // Make enemy fly as well
                float cachedEnemySpeed = enemy.Speed;
                enemy.Speed = 0;
                enemy.AddComponent<Rigidbody>();
                Rigidbody enemyRB = enemy.GetComponent<Rigidbody>();
                Vector3 enemyDirection = enemy.transform.position - transform.position;

                //Add force to enemy rigidbody
                enemyRB.AddForce(enemyDirection.normalized * 50, ForceMode.Impulse);
                enemyRB.AddForce(Vector3.up * 25, ForceMode.Impulse);

                //Calculate enemy direction
                Vector3 direction = transform.position - enemy.transform.position;

                //Add force to the player in the opposite direction of enemy    
                rb.isKinematic = false;
                // Torque to add rotation on crashing.
                Vector3 enemyTorque = new Vector3
                    (
                    Random.Range(-1, 1),
                    Random.Range(-1, 1),
                    Random.Range(-1, 1)
                    ).normalized * cachedEnemySpeed * 10f;
                rb.AddForce(direction.normalized * cachedEnemySpeed * 2f, ForceMode.Impulse);
                rb.AddForce(Vector3.up * cachedEnemySpeed * 1.5f, ForceMode.Impulse);
                rb.AddTorque(enemyTorque, ForceMode.Impulse);
                enemyRB.AddTorque(enemyTorque, ForceMode.Impulse);

                if(!isDead)
                {
                    StartCoroutine(uiManager.GameOver());
                    isDead = true;
                }

            }
            //UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    IEnumerator SlowMotion(float duration, float slowFactor)
    {
        Time.timeScale = slowFactor;
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1;
    }
}
