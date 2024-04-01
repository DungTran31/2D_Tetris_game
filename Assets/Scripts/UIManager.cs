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
    public Text hud_countdown;
    AudioManager audioManager;
    Board board;
    private void Awake()
    {
        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
        optionScreen.SetActive(false);
        highScoreText.text = PlayerPrefs.GetInt("highscore").ToString();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
    }

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
        board.ClearTiles();
        audioManager.PlaySFX(audioManager.clicked);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        audioManager.MusicSource.Play();
        CountdownController.countdownCompleted = false;
    }

    public void RestartPause()
    {
        Board.isPaused = false;
        Time.timeScale = 1.0f;
        board.ClearTiles();
        audioManager.PlaySFX(audioManager.clicked);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        audioManager.MusicSource.Play();
        CountdownController.countdownCompleted = false;
    }

    public void MainMenu()
    {
        board.ClearTiles();
        audioManager.PlaySFX(audioManager.clicked);
        SceneManager.LoadScene(0);
        audioManager.MusicSource.Play();
        CountdownController.countdownCompleted = false;
    }

    public void MainMenuPause()
    {
        Board.isPaused = false;
        Time.timeScale = 1.0f;
        board.ClearTiles();
        audioManager.PlaySFX(audioManager.clicked);
        SceneManager.LoadScene(0);
        audioManager.MusicSource.Play();
        CountdownController.countdownCompleted = false;
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
