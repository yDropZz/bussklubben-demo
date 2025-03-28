using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    int currentScore = 0;
    GameManager gameManager;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI timeText;

    [Header("Game Over Screen")]
    [SerializeField] Image gameOverOverlay;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] float fadeInDuration = 1f;
    [SerializeField] Button playAgainButton;

    [Header("Game Over Animation")]
    [SerializeField] RectTransform gameText;
    [SerializeField] RectTransform overText;
    [SerializeField] RectTransform playAgainButtonMovement;
    [SerializeField] Vector2 gameTextFinalPos;
    [SerializeField] Vector2 overTextFinalPos;
    [SerializeField] Vector2 playAgainButtonFinalPos;
    [SerializeField] float textMoveDuration = 1f;
    [SerializeField] float textMoveDelay = 0.5f;


    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>(); 
    }

    // Update is called once per frame
    void Update()
    {
        currentScore = gameManager.Score;
        scoreText.text = "Score: " + currentScore;
        timeText.text = "Time: " + Mathf.FloorToInt(Time.timeSinceLevelLoad);
        
    }

    public IEnumerator GameOver()
    {
        
        yield return new WaitForSeconds(1f);

        gameOverOverlay.gameObject.SetActive(true);
        gameOverScreen.SetActive(true);
        Color overlayColor = gameOverOverlay.color;

        overlayColor.a = 0;
        gameOverOverlay.color = overlayColor;

        float fadeInDuration = 1f;
        float elapsedTime = 0f;

        while(elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            overlayColor.a = Mathf.Lerp(0, .8f, elapsedTime / fadeInDuration);
            gameOverOverlay.color = overlayColor;
            yield return null; // wait for next frame
        }

        StartCoroutine(PlayOutro());

    }

    public void PlayAgain()
    {
        gameManager.RestartGame();
    }

    public IEnumerator PlayOutro()
    {
        yield return new WaitForSeconds(textMoveDelay);

        gameOverOverlay.gameObject.SetActive(true);

        // Animate Game Text
        gameText.anchoredPosition = new Vector2(-Screen.width, gameText.anchoredPosition.y);
        LeanTween.move(gameText, gameTextFinalPos, textMoveDuration).setEaseOutBack();

        overText.anchoredPosition = new Vector2(Screen.width, overText.anchoredPosition.y);
        LeanTween.move(overText, overTextFinalPos, textMoveDuration).setEaseOutBack().setDelay(textMoveDelay);

        yield return new WaitForSeconds(textMoveDuration);

        playAgainButtonMovement.transform.localScale = Vector3.zero;
        LeanTween.scale(playAgainButtonMovement, Vector3.one, textMoveDuration).setEaseOutBack();

        gameOverScreen.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
    }
}
