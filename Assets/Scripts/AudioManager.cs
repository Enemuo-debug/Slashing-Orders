using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip backgroundMusicClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource.clip = backgroundMusicClip;
            musicSource.loop = true;
            PlayBackgroundMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySlash()
    {
        sfxSource.PlayOneShot(slashClip);
    }

    public void PlayGameOver()
    {
        sfxSource.PlayOneShot(gameOverClip);
    }

    public void PlayBackgroundMusic()
    {
        musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        musicSource.Stop();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.buildIndex == 1)
        {
            StopBackgroundMusic();
        }
        else
        {
            PlayBackgroundMusic();
        }
    }
}
