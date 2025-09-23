using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] AudioSource _Audio;
    Animator _Animator;
    public static System.Action<Transform> OnCheckpointHit;
    bool _IsActive = false;

    private void Awake()
    {
        _IsActive = false;
        _Animator = GetComponent<Animator>();
    }

    private void OnEnable() => OnCheckpointHit += CheckpointHit;
    private void OnDisable() => OnCheckpointHit -= CheckpointHit;

    void CheckpointHit(Transform transform)
    {
        if (_Animator == null) return;

        if (Vector2.Distance(transform.position, this.transform.position) < 0.05f )
        {
            if (_IsActive) return;
            _IsActive = true;
            if (_Audio != null) _Audio.Play();
            _Animator.SetBool("Active", true);
        }
        else
        {
            _IsActive = false;
            _Animator.SetBool("Active", false);
        }
    }
}
