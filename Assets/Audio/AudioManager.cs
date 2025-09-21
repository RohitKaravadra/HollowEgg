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
    [SerializeField] private AudioSource _SFXSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static float PercentToDB(float percent) => Mathf.Log10(Mathf.Clamp(percent, 0.0001f, 1f)) * 20f;
    public static float DBToPercent(float db) => Mathf.Pow(10f, db / 20f);

    public void PlaySound()
    {

    }

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
}
