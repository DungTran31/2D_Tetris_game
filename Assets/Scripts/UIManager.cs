using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject gameOverScreen;
    [SerializeField] public GameObject pauseScreen;
    [SerializeField] public GameObject optionScreen;
    public Text highScoreText;
    AudioManager audioManager;
    public static UIManager instance;

    private void Awake()
    {
        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
        optionScreen.SetActive(false);
        highScoreText.text = PlayerPrefs.GetInt("highscore").ToString();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        if (instance == null)
        {
            // Nếu chưa, gán instance này cho GameManager
            instance = this;
        }
        else
        {
            // Nếu đã có instance khác tồn tại, huỷ bản sao này
            Destroy(gameObject);
        }

    }


        // Kiểm tra xem đã có instance của GameManager chưa
    #region Game Over Functions
    //Game over function
    public void GameOver()
    {
        gameOverScreen.SetActive(true);
    }
    public void Pause()
    {
        audioManager.PlaySFX(audioManager.clicked);
        pauseScreen.SetActive(true);
    }

    //Restart level
    public void Restart()
    {
        audioManager.PlaySFX(audioManager.clicked);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartPause()
    {
        Board.isPaused = false;
        Time.timeScale = 1.0f;
        audioManager.PlaySFX(audioManager.clicked);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        audioManager.PlaySFX(audioManager.clicked);
        SceneManager.LoadScene(0);
    }

    //Quit game/exit play mode if in Editor
    public void Quit()
    {
        audioManager.PlaySFX(audioManager.clicked);
        Application.Quit(); //Quits the game (only works in build)

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; //Exits play mode
        #endif
    }
    #endregion
}
