using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text livesLabel;
    [SerializeField, Min(1)] private int startingLives = 3;

    private bool isGameOver;
    private int currentLives;

    private void Start()
    {
        Time.timeScale = 1f;
        currentLives = Mathf.Max(1, startingLives);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "GameOverController: asigna el panel de Game Over en el Inspector.",
                this
            );
        }

        UpdateLivesLabel();
    }

    public void LoseLife()
    {
        if (isGameOver || currentLives <= 0)
            return;

        currentLives--;
        UpdateLivesLabel();

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void UpdateLivesLabel()
    {
        if (livesLabel != null)
        {
            livesLabel.text = $"LIVES: {currentLives}/{Mathf.Max(1, startingLives)}";
        }
    }
}
