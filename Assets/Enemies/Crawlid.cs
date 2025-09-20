
using UnityEngine;
using BehaviourTreeNamespace;

[RequireComponent(typeof(Rigidbody2D))]
public class Crawlid : MonoBehaviour, IDamageable
{
    [SerializeField] private float _HealthValue = 100;
    [SerializeField] private float _StunnedTime = 0.5f;
    [SerializeField] private PetrolOnGroundStrategy _PetrolStrategy;

    public float Health => _HealthValue;

    SpriteRenderer _Renderer;
    Rigidbody2D _Rigidbody;
    Animator _Animator;

    bool CanMove => Time.time - _LastHitTime > _StunnedTime;
    float _LastHitTime;

    BehaviourTree _BehaviourTree;

    private void Awake()
    {
        _Renderer = GetComponent<SpriteRenderer>();
        _Rigidbody = GetComponent<Rigidbody2D>();
        _Animator = GetComponent<Animator>();

        _PetrolStrategy.Init(ref _Renderer, ref _Rigidbody);

        _BehaviourTree = new BehaviourTree("Caterpillar");
        _BehaviourTree.AddChild(new LeafNode("PetrolOnGround", _PetrolStrategy));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector2 point = (Vector2)transform.position + _PetrolStrategy.Offset;
        Gizmos.DrawLine(point, point + _PetrolStrategy.Dir * _PetrolStrategy.SideCheckDistance * Vector2.right);

        point += _PetrolStrategy.Dir * _PetrolStrategy.SideCheckDistance * Vector2.right;
        Gizmos.DrawLine(point, point + Vector2.down * _PetrolStrategy.GroundCheckDistance);
    }

    private void FixedUpdate()
    {
        if (CanMove)
            _BehaviourTree.Process();
    }

    public void Die()
    {
        _Renderer.color = Color.black;
        _Rigidbody.simulated = false;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
        if(_Animator != null)
            _Animator.enabled = false;
    }

    public void Heal(float amount)
    {

    }

    public void TakeDamage(float damage)
    {
        if (_HealthValue == 0)
            return;

        _Renderer.color = Color.red;
        _HealthValue = Mathf.Max(0, _HealthValue - damage);
        _LastHitTime = Time.time;

        if (_HealthValue == 0) Die();
        else Invoke(nameof(ResetColor), 0.1f);
    }

    private void ResetColor()
    {
        _Renderer.color = Color.white;
    }

}
