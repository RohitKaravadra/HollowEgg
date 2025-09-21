
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Crawlid : EnemyBase
{
    [SerializeField] private float _StunnedTime = 0.5f;
    [SerializeField] private float _WalkSpeed = 1.2f;
    [Space(5)]
    [SerializeField] private Vector2 _Offset;
    [SerializeField] private float _GroundCheckDistance;
    [SerializeField] private float _SideCheckDistance;
    [SerializeField] private LayerMask _GroundLayer;

    bool CanMove => Time.time - _LastHitTime > _StunnedTime;
    int _Dir = 1;

    override public void Start()
    {
        base.Start();
        _Dir = Random.value > 0.5f ? 1 : -1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector2 point = (Vector2)transform.position + _Offset;
        Gizmos.DrawLine(point, point + _Dir * _SideCheckDistance * Vector2.right);

        point += _Dir * _SideCheckDistance * Vector2.right;
        Gizmos.DrawLine(point, point + Vector2.down * _GroundCheckDistance);
    }

    private void UpdateDirection()
    {
        Vector2 point = (Vector2)transform.position + _Offset;
        // check sides
        if (Physics2D.Raycast(point, Vector2.right * _Dir, _SideCheckDistance, _GroundLayer))
        {
            _Dir *= -1;
            return;
        }
        // check ground ending in forward direction
        point += _Dir * _SideCheckDistance * Vector2.right;
        if (!Physics2D.Raycast(point, Vector2.down, _GroundCheckDistance, _GroundLayer))
            _Dir *= -1;
    }

    private void Move()
    {
        Vector2 vel = _WalkSpeed * _Dir * Time.fixedDeltaTime * Vector2.right;
        _Rigidbody.MovePosition(_Rigidbody.position + vel);
    }

    private void FixedUpdate()
    {
        if (!CanMove || !_HealthSystem.IsAlive) return;

        UpdateDirection();
        Move();
        _Renderer.flipX = _Dir < 0;
    }
}
