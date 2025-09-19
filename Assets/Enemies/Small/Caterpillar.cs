
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Caterpillar : MonoBehaviour, IDamageable
{
    [SerializeField] private float _Speed;
    [SerializeField] private Vector2 _Offset;
    [SerializeField] private float _StunnedTime = 0.5f;
    [SerializeField] private float _HealthValue = 100;
    [Space(5)]
    [SerializeField] private float _DownCheckDistance;
    [SerializeField] private float _SideCheckDistance;
    [SerializeField] private LayerMask _GroundLayer;

    public float Health => _HealthValue;

    SpriteRenderer _Renderer;
    Rigidbody2D _Rigidbody;

    bool CanMove => Time.time - _LastHitTime > _StunnedTime;

    int _Direction = 1;
    float _LastHitTime;

    private void Awake()
    {
        _Renderer = GetComponent<SpriteRenderer>();
        _Rigidbody = GetComponent<Rigidbody2D>();

        SetDirection(1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector2 point = (Vector2)transform.position + _Offset;
        Gizmos.DrawLine(point, point + _Direction * _SideCheckDistance * Vector2.right);

        point += _Direction * _SideCheckDistance * Vector2.right;
        Gizmos.DrawLine(point, point + Vector2.down * _DownCheckDistance);
    }

    void UpdateDirection()
    {
        Vector2 point = (Vector2)transform.position + _Offset;

        // check sides
        if (Physics2D.Raycast(point, Vector2.right * _Direction, _SideCheckDistance, _GroundLayer))
        {
            SetDirection(-_Direction);
            return;
        }

        // check ground ending in forward direction
        point += _Direction * _SideCheckDistance * Vector2.right;
        if (!Physics2D.Raycast(point, Vector2.down, _DownCheckDistance, _GroundLayer))
            SetDirection(-_Direction);
    }

    private void FixedUpdate()
    {
        if (CanMove)
        {
            UpdateDirection();
            Vector2 vel = _Direction * _Speed * Vector2.right * Time.fixedDeltaTime;
            _Rigidbody.MovePosition(_Rigidbody.position + vel);
        }
    }

    private void SetDirection(int dir)
    {
        _Direction = dir;
        _Renderer.flipX = _Direction > 0;
    }

    public void Die()
    {
        _Renderer.color = Color.black;
        _Rigidbody.simulated = false;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
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
