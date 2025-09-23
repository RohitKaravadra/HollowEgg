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
    LayerMask _MyLayer;
    private float _LastDirUpdateTime;

    override public void Awake()
    {
        base.Awake();
        _Dir = Random.insideUnitCircle.normalized;
        _Rigidbody.gravityScale = 0;
        _Collider = GetComponent<CircleCollider2D>();
        _MyLayer = gameObject.layer;
    }

    private void Update()
    {
        if (_HealthSystem != null && !_HealthSystem.IsAlive)
            return;

        if (!ComputeDirection())
            UpdateDirection();
        Move();
        _Renderer.flipX = _Dir.x < 0;
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

    private bool ComputeDirection()
    {
        Vector2 origin = _Rigidbody.position;
        RaycastHit2D hit = Physics2D.CircleCast(origin, _Collider.radius, _Dir, _CheckDistane, _ObstacleLayer);
        if (hit)
        {
            _Dir = Vector2.Reflect(_Dir, hit.normal);
            return true;
        }

        Vector2 newPos = origin + _Speed * Time.fixedDeltaTime * _Dir;
        Vector2 diff = newPos - _InitPosition;
        bool changed = false;
        if (Mathf.Abs(diff.x) > _Range.x * 0.5f)
        {
            _Dir.x = -_Dir.x;
            changed = true;
        }
        if (Mathf.Abs(diff.y) > _Range.y * 0.5f)
        {
            _Dir.y = -_Dir.y;
            changed = true;
        }
        return changed;
    }

    private void Move()
    {
        Vector2 vel = _Speed * Time.fixedDeltaTime * _Dir;
        _Rigidbody.MovePosition(_Rigidbody.position + vel);
    }

    protected override void OnDeath()
    {
        CancelInvoke();
        _Renderer.color = Color.gray;
        if (_Animator != null) _Animator.SetBool("Dead", true);
        gameObject.layer = LayerMask.NameToLayer("Ignore Player Collision");
        _Rigidbody.gravityScale = 4;
    }

    protected override void Reset()
    {
        gameObject.layer = _MyLayer;
        _Rigidbody.gravityScale = 0;
        base.Reset();
    }
}
