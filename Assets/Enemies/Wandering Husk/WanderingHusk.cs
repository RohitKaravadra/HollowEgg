
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WanderingHusk : EnemyBase
{
    [SerializeField] private float _WalkSpeed = 1.2f;
    [SerializeField] private float _ChaseSpeed = 2f;
    [SerializeField] private float _ChaseRange = 5f;
    [SerializeField] private float _ChaseHeight = 0.3f;
    [Space(5)]
    [SerializeField] private Vector2 _Offset;
    [SerializeField] private float _GroundCheckDistance;
    [SerializeField] private float _SideCheckDistance;
    [SerializeField] private LayerMask _GroundLayer;

    private bool _WallAhead = false;
    private bool _GroundAhead = false;
    private bool IsGrounded => _Rigidbody.IsTouchingLayers(_GroundLayer);
    private bool _TooCloseToTarget = false;

    private int _Dir = 1;
    private int _ChaseDir = 1;
    private Transform _Target;

    enum State
    {
        Patrol,
        Chase
    }
    State _CurrentState = State.Patrol;

    public override void Start()
    {
        base.Start();
        _Target = EnemySharedData._PlayerTransform;
        _Dir = Random.value > 0.5f ? 1 : -1;
    }

    public override void OnEnable() => base.OnEnable();
    public override void OnDisable() => base.OnDisable();

    private bool CanChase()
    {
        if (_Target == null) return false;

        if (_WallAhead && !_GroundAhead)
            return false;

        Vector2 diff = transform.position - _Target.position;

        if (_TooCloseToTarget = Mathf.Abs(diff.x) < 0.2f)
            return false;

        _ChaseDir = diff.x > 0 ? -1 : 1;
        return Mathf.Abs(diff.x) < _ChaseRange && Mathf.Abs(diff.y) < _ChaseHeight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector2 point = (Vector2)transform.position + _Offset;

        Gizmos.DrawLine(point, point + _Dir * _SideCheckDistance * Vector2.right);

        point += _Dir * _SideCheckDistance * Vector2.right;
        Gizmos.DrawLine(point, point + Vector2.down * _GroundCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(point + Vector2.down * _ChaseHeight, point + Vector2.up * _ChaseHeight);
        Gizmos.DrawWireSphere(transform.position, _ChaseRange);
    }

    private void Update()
    {
        if (!CanMove || !_HealthSystem.IsAlive)
            return;

        CheckCollisions();
        SetState();
        SetDirections();
        Move();
        _Renderer.flipX = _Dir < 0;

        _Animator.SetBool("Chase", _CurrentState == State.Chase);
    }

    private void CheckCollisions()
    {
        Vector2 point = _Rigidbody.position + _Offset;
        _WallAhead = Physics2D.Raycast(point, Vector2.right * _Dir, _SideCheckDistance, _GroundLayer);

        point += _Dir * _SideCheckDistance * Vector2.right;
        _GroundAhead = Physics2D.Raycast(point, Vector2.down, _GroundCheckDistance, _GroundLayer);
    }

    private void SetDirections()
    {
        if (!IsGrounded) return;
        if (State.Chase == _CurrentState)
        {
            _Dir = _ChaseDir;
        }
        else
        {
            if (_WallAhead || !_GroundAhead)
                _Dir *= -1;
        }
    }

    private void SetState()
    {
        if (CanChase()) _CurrentState = State.Chase;
        else _CurrentState = State.Patrol;
    }

    private void Move()
    {
        if (!IsGrounded || _TooCloseToTarget) return;
        float speed = _CurrentState == State.Patrol ? _WalkSpeed : _ChaseSpeed;
        _Rigidbody.linearVelocityX = speed * _Dir;
    }
}
