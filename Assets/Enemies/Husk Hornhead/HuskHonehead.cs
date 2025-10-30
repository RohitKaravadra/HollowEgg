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

    private bool _WallAhead = false;
    private bool _GroundAhead = false;
    private bool IsGrounded => _Rigidbody.IsTouchingLayers(_GroundLayer);

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
        _Dir = Random.value > 0.5f ? 1 : -1;
        _Renderer.flipX = _Dir < 0;
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
        Move();
        _Renderer.flipX = _Dir < 0;

        if (_Animator != null)
            _Animator.SetBool("Chase", _CurrentState == State.Chase);
    }

    private bool CanChase()
    {
        if (_Target == null)
            return false;

        if (_WallAhead && !_GroundAhead)
            return false;

        Vector2 diff = transform.position - _Target.position;

        if (Mathf.Abs(diff.x) < 0.2f)
            return false;

        bool canChase = Mathf.Abs(diff.x) < _ChaseRange && Mathf.Abs(diff.y) < _ChaseHeight;
        if (canChase)
            _Dir = diff.x > 0 ? -1 : 1;
        return canChase;
    }

    private void CheckCollisions()
    {
        Vector2 point = _Rigidbody.position + _Offset;
        _WallAhead = Physics2D.Raycast(point, Vector2.right * _Dir, _SideCheckDistance, _GroundLayer);

        point += _Dir * _SideCheckDistance * Vector2.right;
        _GroundAhead = Physics2D.Raycast(point, Vector2.down, _GroundCheckDistance, _GroundLayer);
    }

    private void SetState()
    {
        if (CanChase())
            _CurrentState = State.Chase;
        else
            _CurrentState = State.Idle;

        if (_CurrentState == State.Chase)
        {
            if (_WallAhead || !_GroundAhead)
                _CurrentState = State.Idle;
        }
    }

    private void Move()
    {
        if (!IsGrounded)
            return;
        _Rigidbody.linearVelocityX = _CurrentState == State.Chase ? _ChaseSpeed * _Dir : 0;
    }

    protected override void ResetEnemy()
    {
        base.ResetEnemy();
        _CurrentState = State.Idle;
        _LastHitTime = 0f;
    }
}
