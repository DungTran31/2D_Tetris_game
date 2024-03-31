using UnityEngine;

public class ClickedButton : MonoBehaviour
{
    public void OnClickVibrate()
    {
        if (SettingMenuManager.isVibrate)
        {
            Handheld.Vibrate();
            Debug.Log("Vibrate");
        }
    }
}
