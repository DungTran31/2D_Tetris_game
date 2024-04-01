using UnityEngine;

public class ClickedButton : MonoBehaviour
{
    public void OnClickVibrate()
    {
        if (SettingMenuManager.isVibrate)
        {
            Debug.Log("Vibrate");
        } 
        else
        {
            Debug.Log("Vibrate is disabled. Performing alternative action...");
        }    
    }
}
