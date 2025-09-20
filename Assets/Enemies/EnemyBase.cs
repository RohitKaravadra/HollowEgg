using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    protected HealthSystem _HealthSystem;
    protected SpriteRenderer _Renderer;
    protected Rigidbody2D _Rigidbody;
    protected Animator _Animator;

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
    }

    public virtual void OnEnable()
    {
        if (_HealthSystem != null)
        {
            _HealthSystem.OnDeath.AddListener(OnDeath);
            _HealthSystem.OnHit.AddListener(OnHit);
        }
    }

    public virtual void OnDisable()
    {
        if (_HealthSystem != null)
        {
            _HealthSystem.OnDeath.RemoveListener(OnDeath);
            _HealthSystem.OnHit.RemoveListener(OnHit);
        }
    }

    protected void ResetColor() => _Renderer.color = Color.white;

    protected virtual void OnDeath()
    {
        _Renderer.color = Color.black;
        _Rigidbody.simulated = false;

        if (TryGetComponent(out Collider2D collider)) collider.enabled = false;
        if (_Animator != null) _Animator.enabled = false;
    }

    protected virtual void OnHit()
    {
        _Renderer.color = Color.red;
        _LastHitTime = Time.time;
        Invoke(nameof(ResetColor), 0.1f);
    }
}
