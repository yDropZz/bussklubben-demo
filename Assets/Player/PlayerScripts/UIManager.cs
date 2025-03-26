using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    int currentScore = 0;
    GameManager gameManager;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI timeText;


    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();   
    }

    // Update is called once per frame
    void Update()
    {
        currentScore = gameManager.Score;
        scoreText.text = "Score: " + currentScore;
        timeText.text = "Time: " + Mathf.FloorToInt(Time.timeSinceLevelLoad);
        
    }
}
