
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WanderingHusk : EnemyBase
{
    [SerializeField] private float _StunnedTime = 0.5f;
    [Space(5)]
    [SerializeField] private float _WalkSpeed = 1.2f;
    [SerializeField] private float _ChaseSpeed = 2f;
    [SerializeField] private float _ChaseRange = 5f;
    [SerializeField] private float _ChaseHeight = 0.3f;
    [Space(5)]
    [SerializeField] private Vector2 _Offset;
    [SerializeField] private float _GroundCheckDistance;
    [SerializeField] private float _SideCheckDistance;
    [SerializeField] private LayerMask _GroundLayer;

    private bool CanMove => Time.time - _LastHitTime > _StunnedTime;

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
        Vector2 diff = transform.position - _Target.position;
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
        if (_Animator != null)
            _Animator.SetBool("Chase", _CurrentState == State.Chase);
    }

    private bool WallAhead()
    {
        Vector2 point = _Rigidbody.position + _Offset;
        return Physics2D.Raycast(point, Vector2.right * _Dir, _SideCheckDistance, _GroundLayer);
    }

    private bool GroundAhead()
    {
        Vector2 point = _Rigidbody.position + _Offset;
        point += _Dir * _SideCheckDistance * Vector2.right;
        return Physics2D.Raycast(point, Vector2.down, _GroundCheckDistance, _GroundLayer);
    }

    private void SetDirections()
    {
        if (State.Chase == _CurrentState)
            _Dir = _ChaseDir;
        else
        {
            if (WallAhead() || !GroundAhead())
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
        float speed = _CurrentState == State.Patrol ? _WalkSpeed : _ChaseSpeed;
        Vector2 vel = speed * _Dir * Time.fixedDeltaTime * Vector2.right;
        _Rigidbody.MovePosition(_Rigidbody.position + vel);
    }

    private void FixedUpdate()
    {
        if (!CanMove || !_HealthSystem.IsAlive) return;

        SetState();
        SetDirections();
        Move();
        _Renderer.flipX = _Dir < 0;
    }
}
