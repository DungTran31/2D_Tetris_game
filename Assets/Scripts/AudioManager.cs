using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("------- Audio Source --------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [Header("------- Audio Clip --------")]
    public AudioClip background;
    public AudioClip move;
    public AudioClip rotate;
    public AudioClip land;
    public AudioClip lineCleared;
    public AudioClip gameStart;
    public AudioClip gameOver;
    public AudioClip countdown;
    public AudioClip clicked;

    private void Start()
    {
        musicSource.clip = background;
        //musicSource.Play();
    }

    // Thay đổi mức độ truy cập của musicSource từ private sang public
    public AudioSource MusicSource => musicSource;
    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
