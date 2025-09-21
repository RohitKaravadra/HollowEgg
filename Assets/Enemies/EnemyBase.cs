using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    protected HealthSystem _HealthSystem;
    protected SpriteRenderer _Renderer;
    protected Rigidbody2D _Rigidbody;
    protected Animator _Animator;

    protected Vector2 _InitPosition;
    protected float _LastHitTime;

    public virtual void Awake()
    {
        _Renderer = GetComponent<SpriteRenderer>();
        _Rigidbody = GetComponent<Rigidbody2D>();
        _Animator = GetComponent<Animator>();
        _HealthSystem = GetComponent<HealthSystem>();
    }

    public virtual void Start()
    {
        _HealthSystem.Reset();
        _InitPosition = _Rigidbody.position;
    }

    public virtual void OnEnable()
    {
        if (_HealthSystem != null)
        {
            _HealthSystem.OnDeath.AddListener(OnDeath);
            _HealthSystem.OnHit.AddListener(OnHit);
        }

        GameManager.OnResetEnemies += Reset;
    }

    public virtual void OnDisable()
    {
        if (_HealthSystem != null)
        {
            _HealthSystem.OnDeath.RemoveListener(OnDeath);
            _HealthSystem.OnHit.RemoveListener(OnHit);
        }

        GameManager.OnResetEnemies -= Reset;
    }

    private void OnDestroy() => CancelInvoke();

    protected void ResetColor() => _Renderer.color = Color.white;

    protected virtual void OnDeath()
    {
        _Rigidbody.simulated = false;

        if (TryGetComponent(out Collider2D collider)) collider.enabled = false;
        if (_Animator != null) _Animator.SetBool("Dead", true);
    }

    protected virtual void OnHit()
    {
        _Renderer.color = Color.red;
        _LastHitTime = Time.time;
        Invoke(nameof(ResetColor), 0.1f);
    }

    protected virtual void Reset()
    {
        _Rigidbody.simulated = false;
        transform.position = _InitPosition;
        _Rigidbody.simulated = true;

        if (TryGetComponent(out Collider2D collider)) collider.enabled = true;
        if (_Animator != null) _Animator.SetBool("Dead", false);
        _HealthSystem.Reset();
    }
}
