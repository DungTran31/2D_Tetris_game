using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text highScoreText;

    private void Start()
    {
        highScoreText.text = PlayerPrefs.GetInt("highscore").ToString(); 
    }

}
