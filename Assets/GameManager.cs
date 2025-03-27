using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] AudioClip backgroundMusic;
    [SerializeField] int score = 0;
    [SerializeField] Transform playerPos;
    public int Score { get { return score;}}


    MapGeneration mapGeneration;


    void Awake()
    {
        Application.targetFrameRate = 60;
        mapGeneration = FindAnyObjectByType<MapGeneration>();
        playerPos = FindAnyObjectByType<Player>().transform;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        SoundManager.Instance.PlayMusic(backgroundMusic);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            mapGeneration.ClearTiles();
        }

        
    }

    public void CalculateScore()
    {
        score = Mathf.FloorToInt(playerPos.position.z / 10);
        Debug.Log("Score: " + score);
    }
}
