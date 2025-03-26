using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementTime = .15f;
    [SerializeField] private float bounceMultiplier = .1f;
    Vector3 targetPosition;
    bool isMoving = false;
    float startTime;
    public Transform playerPos;
    GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W) && !isMoving)
        {
            targetPosition += new Vector3(0, 0, 10);
            startTime = Time.time;
            isMoving = true;
            gameManager.CalculateScore();

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
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
