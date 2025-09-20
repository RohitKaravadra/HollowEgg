using UnityEngine;

public class HuskHonehead : EnemyBase
{
    [SerializeField] private float _ChaseSpeed = 2f;
    [SerializeField] private float _ChaseRange = 5f;
    [SerializeField] private float _ChaseHeight = 0.3f;
    [Space(5)]
    [SerializeField] private Vector2 _Offset;
    [SerializeField] private float _GroundCheckDistance;
    [SerializeField] private float _SideCheckDistance;
    [SerializeField] private LayerMask _GroundLayer;

    private int _Dir = 1;
    private Transform _Target;

    enum State
    {
        Idle,
        Chase
    }

    State _CurrentState = State.Idle;

    public override void OnEnable() => base.OnEnable();

    public override void OnDisable() => base.OnDisable();

    public override void Start()
    {
        base.Start();
        _Target = EnemySharedData._PlayerTransform;
    }

    private bool CanChase()
    {
        if (_Target == null) return false;
        Vector2 diff = transform.position - _Target.position;
        _Dir = diff.x > 0 ? -1 : 1;
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

    private void SetState()
    {
        if (CanChase()) _CurrentState = State.Chase;
        else _CurrentState = State.Idle;

        if (_CurrentState == State.Chase)
        {
            if (WallAhead() || !GroundAhead())
                _CurrentState = State.Idle;
        }
    }

    private void Move()
    {
        if (State.Idle == _CurrentState) return;
        Vector2 vel = _ChaseSpeed * _Dir * Time.fixedDeltaTime * Vector2.right;
        _Rigidbody.MovePosition(_Rigidbody.position + vel);
    }

    private void FixedUpdate()
    {
        if (!_HealthSystem.IsAlive) return;

        SetState();
        Move();
        _Renderer.flipX = _Dir < 0;
    }
}
