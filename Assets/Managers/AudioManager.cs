
using UnityEngine;
using UnityEngine.Audio;

enum AudioType
{
    Music,
    SFX
}

struct AudioData
{
    public AudioType Type;
    public AudioClip Clip;
}

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioData[] _AudioData;
    [SerializeField] private AudioMixer _Mixer;
    [SerializeField] private AudioSource _MusicSource;
    [SerializeField] private AudioSource _UISource;
    [Space(10)]
    [SerializeField] private AudioClip _DefaultMusic;
    [SerializeField] private AudioClip _FinalBossMusic;
    [Space(10)]
    [SerializeField] private AudioClip _ButtonHoverSound;
    [SerializeField] private AudioClip _ButtonClickSound;

    //singleton pattern
    public static AudioManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (_MusicSource != null && _DefaultMusic != null)
        {
            _MusicSource.clip = _DefaultMusic;
            _MusicSource.Play();
        }
    }

    private void OnEnable()
    {
        GameManager.OnBossFightTriggered += OnBossFightStart;
        GameManager.OnPlayerRespawn += OnPlayerRespawn;
        GameManager.OnBossDeathTriggered += OnBossDead;
    }

    private void OnDisable()
    {
        GameManager.OnBossFightTriggered -= OnBossFightStart;
        GameManager.OnPlayerRespawn -= OnPlayerRespawn;
        GameManager.OnBossDeathTriggered -= OnBossDead;
    }

    private void OnBossFightStart()
    {
        if (_FinalBossMusic != null)
        {
            if (_MusicSource.clip != _FinalBossMusic)
            {
                _MusicSource.clip = _FinalBossMusic;
                _MusicSource.Play();
            }
        }
    }

    private void OnPlayerRespawn()
    {
        if (_DefaultMusic != null)
        {
            if (_MusicSource.clip != _DefaultMusic)
            {
                _MusicSource.clip = _DefaultMusic;
                _MusicSource.Play();
            }
        }
    }

    private void OnBossDead()
    {
        // fade out to nothing
        _MusicSource.Stop();
    }

    public static float PercentToDB(float percent) => Mathf.Log10(Mathf.Clamp(percent, 0.0001f, 1f)) * 20f;
    public static float DBToPercent(float db) => Mathf.Pow(10f, db / 20f);

    public void SetMusicVolume(float percent) => _Mixer.SetFloat("MusicVolume", PercentToDB(percent));
    public void SetSFXVolume(float percent) => _Mixer.SetFloat("SFXVolume", PercentToDB(percent));

    public float GetMusicVolume()
    {
        _Mixer.GetFloat("MusicVolume", out float db);
        return DBToPercent(db);
    }

    public float GetSFXVolume()
    {
        _Mixer.GetFloat("SFXVolume", out float db);
        return DBToPercent(db);
    }

    public void PlayButtonHover()
    {
        if (_ButtonHoverSound != null && _UISource != null)
        {
            _UISource.PlayOneShot(_ButtonHoverSound);
        }
    }

    internal void PlayButtonClick()
    {
        if (_ButtonHoverSound != null && _UISource != null)
        {
            _UISource.PlayOneShot(_ButtonClickSound);
        }
    }
}
