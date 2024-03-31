using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownController : MonoBehaviour
{
    public int countdownTime;
    public Text hud_countdownText;
    public static event Action CountdownFinished;
    public static bool countdownCompleted = false;
    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
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

            yield return new WaitForSeconds(1f);

            countdownTime--;
        }

        hud_countdownText.text = "GO!";

        yield return new WaitForSeconds(1f);

        hud_countdownText.gameObject.SetActive(false);
        CountdownFinished?.Invoke();
        countdownCompleted = true;
    }
}
