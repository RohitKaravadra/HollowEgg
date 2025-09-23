using UnityEngine;

[System.Serializable]
public class PlayerAudio
{
    [SerializeField] AudioSource _Source;
    [SerializeField] AudioClip _FootStepSound;
    [SerializeField] AudioClip _DashSound;
    [SerializeField] AudioClip _SlashSound;
    [SerializeField] AudioClip _landSound;
    [SerializeField] AudioClip _HitSound;
    [SerializeField] AudioClip _DeathSound;

    public void PlaySlash()
    {
        if (_Source != null && _SlashSound != null)
            _Source.PlayOneShot(_SlashSound);
    }

    public void PlayHit()
    {
        if (_Source != null && _HitSound != null)
            _Source.PlayOneShot(_HitSound);
    }

    public void PlayDeath()
    {
        if (_Source != null && _DeathSound != null)
            _Source.PlayOneShot(_DeathSound);
    }

    public void PlayDash()
    {
        if (_Source != null && _DashSound != null)
            _Source.PlayOneShot(_DashSound);
    }

    public void PlayLand()
    {
        if (_Source != null && _landSound != null)
            _Source.PlayOneShot(_landSound);
    }

    public void PlayFootStep()
    {
        if (_Source != null && _FootStepSound != null)
            _Source.PlayOneShot(_FootStepSound);
    }
}
