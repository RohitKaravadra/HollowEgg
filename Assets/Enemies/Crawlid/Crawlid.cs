
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Crawlid : EnemyBase
{
    [SerializeField] private float _WalkSpeed = 1.2f;
    [Space(5)]
    [SerializeField] private Vector2 _Offset;
    [SerializeField] private float _GroundCheckDistance;
    [SerializeField] private float _SideCheckDistance;
    [SerializeField] private LayerMask _GroundLayer;

    private bool _WallAhead = false;
    private bool _GroundAhead = false;
    private bool IsGrounded => _Rigidbody.IsTouchingLayers(_GroundLayer);

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

    private void CheckCollisions()
    {
        Vector2 point = _Rigidbody.position + _Offset;
        _WallAhead = Physics2D.Raycast(point, Vector2.right * _Dir, _SideCheckDistance, _GroundLayer);

        point += _Dir * _SideCheckDistance * Vector2.right;
        _GroundAhead = Physics2D.Raycast(point, Vector2.down, _GroundCheckDistance, _GroundLayer);
    }

    private void UpdateDirection()
    {
        if (!IsGrounded) return;
        if (_WallAhead || !_GroundAhead)
            _Dir *= -1;
    }

    private void Move() => _Rigidbody.linearVelocityX = _WalkSpeed * _Dir;

    private void Update()
    {
        if (!CanMove || !_HealthSystem.IsAlive) return;

        CheckCollisions();
        UpdateDirection();
        Move();
        _Renderer.flipX = _Dir < 0;
    }
}
