using UnityEngine;

public class FadeAnimation : MonoBehaviour
{
    Animator _Animator;

    private void Awake() => _Animator = GetComponent<Animator>();

    private void Start() => FadeOut();

    private void OnEnable()
    {

        GameManager.OnPlayerDead += FadeIn;
        GameManager.OnPlayerRespawn += FadeOut;
    }

    private void OnDisable()
    {

        GameManager.OnPlayerDead -= FadeIn;
        GameManager.OnPlayerRespawn -= FadeOut;
    }

    private void FadeIn() => _Animator.Play("FadeIn");
    private void FadeOut() => _Animator.Play("FadeOut");
}
