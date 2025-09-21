using UnityEngine;

public class Mosquito : EnemyBase
{
    [SerializeField] float _Speed;
    [SerializeField] float _ChaseSpeed;
    [SerializeField] Vector2 _Range;
    [SerializeField] float _DirUpdateInterval = 2f;
    [Space(5)]
    [SerializeField] float _CheckDistane;
    [SerializeField] LayerMask _ObstacleLayer;

    Vector2 _Dir;
    CircleCollider2D _Collider;
    private float _LastDirUpdateTime;

    override public void Awake()
    {
        base.Awake();
        _Dir = Random.insideUnitCircle.normalized;
        _Rigidbody.gravityScale = 0;
        _Collider = GetComponent<CircleCollider2D>();
    }

    override public void Start()
    {
        base.Start();
    }

    override public void OnEnable()
    {
        base.OnEnable();
    }

    override public void OnDisable()
    {
        base.OnDisable();
    }

    private void FixedUpdate()
    {
        if (_HealthSystem != null && !_HealthSystem.IsAlive)
            return;

        UpdateDirection();
        ComputeDirection();
        Move();
    }

    private void OnDrawGizmos()
    {
        // draw a rectangle to represent the range
        Gizmos.color = Color.red;
        if (Application.isPlaying)
            Gizmos.DrawWireCube(_InitPosition, _Range);
        else
            Gizmos.DrawWireCube(transform.position, _Range);
    }

    private void UpdateDirection()
    {
        if (Time.time - _LastDirUpdateTime < _DirUpdateInterval)
            return;
        _Dir = Random.insideUnitCircle.normalized;
        _LastDirUpdateTime = Time.time;
    }

    private void ComputeDirection()
    {
        Vector2 origin = _Rigidbody.position;
        RaycastHit2D hit = Physics2D.CircleCast(origin, _Collider.radius, _Dir, _CheckDistane, _ObstacleLayer);
        if (hit)
        {
            _Dir = Vector2.Reflect(_Dir, hit.normal);
            return;
        }

        Vector2 newPos = origin + _Speed * Time.fixedDeltaTime * _Dir;
        if (Mathf.Abs(newPos.x - _InitPosition.x) > _Range.x * 0.5f)
            _Dir.x = -_Dir.x;
        if (Mathf.Abs(newPos.y - _InitPosition.y) > _Range.y * 0.5f)
            _Dir.y = -_Dir.y;

        _Renderer.flipX = _Dir.x < 0;
    }

    private void Move()
    {
        Vector2 vel = _Dir * _Speed * Time.fixedDeltaTime;
        _Rigidbody.MovePosition(_Rigidbody.position + vel);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        _Rigidbody.gravityScale = 4;
        // Additional death behavior for Mosquito can be added here
    }

    protected override void OnHit()
    {
        base.OnHit();
        // Additional hit behavior for Mosquito can be added here
    }

    protected override void Reset()
    {
        _Rigidbody.gravityScale = 0;
        base.Reset();
    }
}
