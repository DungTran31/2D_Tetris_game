using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingMenuManager : MonoBehaviour
{
    public static bool isVibrate;
    public Dropdown graphicsDropdown;
    public Slider masterVol, musicVol, sfxVol;
    public AudioMixer mainAudioMixer;
    public Toggle vibrateToggle;
    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ChangeMasterVolume() 
    {
        mainAudioMixer.SetFloat("MasterVol", masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat("MusicVol", musicVol.value);
    }
    public void ChangeSFXVolume()
    {
        mainAudioMixer.SetFloat("SFXVol", sfxVol.value);
    }

    public void ChangeVibrate()
    {
        isVibrate = vibrateToggle;
    }

    public void OpenLink(string link)
    {
        Application.OpenURL(link);
    }
    private void Start()
    {
        isVibrate = true;
        vibrateToggle.isOn = true;
    }
}
