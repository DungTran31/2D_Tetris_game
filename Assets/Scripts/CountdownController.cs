using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownController : MonoBehaviour
{
    public int countdownTime;
    public Text hud_countdownText;
    public static bool countdownCompleted = false;
    AudioManager audioManager;
    Board board;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
    }
    private void Start()
    {
        StartCoroutine(CountdownToStart());
        audioManager.PlaySFX(audioManager.countdown);
    }
    IEnumerator CountdownToStart()
    {

        while (countdownTime > 0)
        {
            hud_countdownText.text = countdownTime.ToString();

            yield return new WaitForSeconds(0.82f);

            countdownTime--;
        }

        hud_countdownText.text = "GO!";

        yield return new WaitForSeconds(1f);

        hud_countdownText.gameObject.SetActive(false);
        board.StartGame();
        countdownCompleted = true;
    }
}
