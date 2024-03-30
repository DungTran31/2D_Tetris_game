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
    public AudioClip gameOver;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
