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
    [SerializeField] AudioClip[] dashSounds;
    [SerializeField] AudioClip[] crashSounds;

    Vector3 targetPosition;
    bool isMoving = false;
    float startTime;
    public Transform playerPos;
    GameManager gameManager;

    [Header("Particles")]
    [SerializeField] GameObject crashParticles;
    [SerializeField] GameObject dashParticles;
    [SerializeField] Transform dashParticleSpawnPoint;

    [Header("Movement")]
    Vector2 touchStartPosition;
    Vector2 touchEndPosition;
    [SerializeField] float swipeThreshold = 50f;
    private bool canDash = false;
    [SerializeField]  private float dashForce = 250f;
    [SerializeField] private float rotationSpeed = 50f;


    bool isVeryCompletelyDead = false;
    bool isCompletelyDead = false;
    bool isDead = false;
    public bool Dead { get {return isDead;} }

    bool hasBufferedInput = false;
    bool hasBufferedBackwardInput = false;
    UIManager uiManager;
    private int crashes = 0;
    private int crashIndex = 0;
    bool comboMode = false;

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
        gameManager.CalculateScore();


        if(transform.position.y < -10f)
        {
            isDead = true;

            if(!isCompletelyDead)
            {
                StartCoroutine(uiManager.GameOver());
            }

            isCompletelyDead = true;


        }

        Dashing();

        if(isDead)
        {
            return;
        }

        // Handle touch input
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                // Store starting position of touch
                touchStartPosition = touch.position;
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                touchEndPosition = touch.position;

                float swipeDistanceY = touchEndPosition.y - touchStartPosition.y;

                if(Mathf.Abs(swipeDistanceY) > swipeThreshold)
                {
                    if(swipeDistanceY < 0)
                    {
                        // Swipe detected
                        if(transform.position.z < 0)
                        {
                            return;
                        }

                        if(!isMoving)
                        {
                            MovePlayerBackwards();
                        }
                        else
                        {
                            hasBufferedBackwardInput = true;
                        }
                    }
                }
                else
                {
                    if(!isMoving)
                    {
                        MovePlayerForward();
                    }
                    else
                    {
                        hasBufferedInput = true;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!isMoving)
            {
                // If not moving, execute the movement immediately
                MovePlayerForward();
            }
            else
            {
                // If already moving, buffer the input
                hasBufferedInput = true;
            }
        }

        

        if(Input.GetKeyDown(KeyCode.S))
        {
            if(transform.position.z < 0)
            {
                return;
            }

            if(!isMoving)
            {
                MovePlayerBackwards();
            }
            else
            {
                hasBufferedBackwardInput = true;
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
                    MovePlayerForward();
                }
                else if(hasBufferedBackwardInput)
                {
                    hasBufferedBackwardInput = false;
                    MovePlayerBackwards();
                }
            }
        }
    }

    void MovePlayerForward()
    {
        targetPosition += new Vector3(0, 0, 10);
        startTime = Time.time;
        isMoving = true;
        gameManager.CalculateScore();

        // Play a random jump sound
        SoundManager.Instance.PlaySoundEffect(jumpSounds[Random.Range(0, jumpSounds.Length)]);
    }

    void MovePlayerBackwards()
    {
        targetPosition -= new Vector3(0, 0, 10);
        startTime = Time.time;
        isMoving = true;

        SoundManager.Instance.PlaySoundEffect(jumpSounds[Random.Range(0, jumpSounds.Length)]);
    }

    void Dashing()
    {
        // Keyboard

        if(canDash)
        {
            Vector2 dashDirection = Vector2.zero;

            if(Input.GetKeyDown(KeyCode.W))
            {
                dashDirection = Vector2.up;
            }
            else if(Input.GetKeyDown(KeyCode.A))
            {
                dashDirection = Vector2.left;
            }
            else if(Input.GetKeyDown(KeyCode.S))
            {
                dashDirection = Vector2.down;
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                dashDirection = Vector2.right;
            }

            if(dashDirection != Vector2.zero)
            {
                PerformDash(dashDirection);
            }
        }

        //-------------------------------

        // Mobile

        if(canDash && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                touchStartPosition = touch.position;
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                touchEndPosition = touch.position;

                Vector2 swipeDirection = touchEndPosition - touchStartPosition;

                if(swipeDirection.magnitude > swipeThreshold)
                {
                    PerformDash(swipeDirection.normalized);
                    canDash = false;
                }
            }
        }

        //-------------------------------

        return;
    }

    void PerformDash(Vector2 swipeDirection)
    {
        // Converting swipeDirection to a 3d vector in world space
        Vector3 dashDirection = new Vector3(swipeDirection.x, 0, swipeDirection.y).normalized;

        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        Vector3 torqueDirection = new Vector3(-swipeDirection.y, 0, swipeDirection.x).normalized;

        rb.AddTorque(torqueDirection * rotationSpeed, ForceMode.Impulse);

        SoundManager.Instance.PlaySoundEffect(dashSounds[Random.Range(0, dashSounds.Length)]);

        // Play dash particles
        GameObject dashParticlesInstance = Instantiate(dashParticles, transform.position, Quaternion.identity);
        dashParticlesInstance.transform.forward = -dashDirection;




    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            if(other.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.GetComponent<Collider>().isTrigger = false;

                canDash = true;

                // Particles
                crashParticles.SetActive(true);

                //Make sure player stops moving after 5s to avoid infinite score

                

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
                SoundManager.Instance.PlaySoundEffect(crashSounds[crashIndex]);
                crashIndex++;
                crashes++;

                if(crashIndex >= crashSounds.Length)
                {
                    crashIndex = 0;
                }

                // Make enemy fly as well
                float cachedEnemySpeed = enemy.Speed;
                enemy.Speed = 0;

                enemy.AddComponent<Rigidbody>();
                Rigidbody enemyRB = enemy.GetComponent<Rigidbody>();
                Vector3 enemyDirection = enemy.transform.position - transform.position;

                enemyRB.interpolation = RigidbodyInterpolation.Interpolate;

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

                    if(enemy.IsTrain)
                    {
                        rb.AddForce(direction.normalized * cachedEnemySpeed * .5f,ForceMode.Impulse);
                        rb.AddForce(Vector3.up * cachedEnemySpeed * .5f, ForceMode.Impulse);
                        rb.AddTorque(enemyTorque, ForceMode.Impulse);
                    }
                    else
                    {
                        rb.AddForce(direction.normalized * cachedEnemySpeed * 2f, ForceMode.Impulse);
                        rb.AddForce(Vector3.up * cachedEnemySpeed * 1f, ForceMode.Impulse);
                        rb.AddTorque(enemyTorque, ForceMode.Impulse);
                    }

                    // Add enemy torque to enemy as well, too lazy to rearrange stuff here xd
                    enemyRB.AddTorque(enemyTorque, ForceMode.Impulse);

                    uiManager.AnimateCombo(crashes);
                    StartCoroutine(SlowMotion(.15f, .20f));
                    float slowmoTime = .15f;
                    slowmoTime = slowmoTime - .01f * crashes;
                    Mathf.Clamp(slowmoTime, 0, .15f);
                    

                if(!isDead)
                {   
                
                    isDead = true;
                }

            }
        }
        else if(other.CompareTag("Bounds"))
        {
            Debug.Log("Hit bounds!");
            Vector3 playerDirection =  other.transform.position - transform.position;

            rb.AddForce(playerDirection.normalized * 50f, ForceMode.Impulse);

            isDead = true;

            if(!isCompletelyDead)
            {
                StartCoroutine(uiManager.GameOver());
            }
            
            isCompletelyDead = true;
        }
        else if(other.CompareTag("Ground"))
        {
            if(isDead && !isCompletelyDead)
            {
                isCompletelyDead = true;
                other.GetComponentInChildren<Collider>().isTrigger = false;
                StartCoroutine(CheckIfStationary());

            }
        }
    }

    IEnumerator CheckIfStationary()
    {
        float stationaryTime = 0f;
        float requiredStationaryTime = .15f;

        while(stationaryTime < requiredStationaryTime)
        {
            if(rb.linearVelocity.magnitude < .1f)
            {
                stationaryTime += Time.deltaTime;
            }
            else
            {
                stationaryTime = 0f;
            }

            yield return null;
        }

        rb.isKinematic = true;

        StartCoroutine(uiManager.GameOver());
    }

    IEnumerator SlowMotion(float duration, float slowFactor)
    {
        if(duration <= 0)
        {
            yield break;
        }
        Time.timeScale = slowFactor;
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1;
    }

    void ResetPlayerMomentum()
    {
        // reset velocity on z axis
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
        rb.angularVelocity = Vector3.zero;

    }
}
