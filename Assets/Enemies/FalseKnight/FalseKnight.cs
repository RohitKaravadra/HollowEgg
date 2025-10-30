using UnityEngine;

public class FalseKnight : EnemyBase
{
    [System.Serializable]
    struct StateData
    {
        public State state;
        public float duration;
        public float timer;
    }

    // False Knight States
    enum State
    {
        Idle,
        JumpAttack,
        MultipleJumpAttack,
        SlashAttack
    }

    #region Editor Properties

    [SerializeField] bool _InAction = false;
    [SerializeField] StateData[] _StateLoop;
    [SerializeField] float _JumpForce = 10f;
    [SerializeField] float _Gravity;
    [Space(5)]
    [SerializeField] float _AttackAnimTime = 1.0f;
    [SerializeField] float _AttackDelay = 0.2f;
    [SerializeField] Vector2 _AttackOffset;
    [SerializeField] Vector2 _AttackSize;
    [SerializeField] LayerMask _AttackLayer;
    [Space(5)]
    [SerializeField] Rigidbody2D.SlideMovement _SlideMovement;
    [SerializeField] ContactFilter2D _GroundFilter;
    [SerializeField] ShakeData _LandShake;

    #endregion

    Collider2D _Collider;
    Transform _Target;

    bool _IsGrounded = false;
    bool _IsJumping = false;
    bool _IsSlashing = false;

    int _MultipleJumpCount = 0;
    int _MultipleSlashCount = 0;
    int _TargetDir = 0;

    int _CurrentStateIndex = 0;
    float _StateTimer = 0f;

    bool HasTarget => _Target != null;
    bool HasCollider => _Collider != null;
    bool HasStates => _StateLoop != null && _StateLoop.Length > 0;

    Vector2 _Velocity;

    public bool InAction
    {
        get => _InAction;
        set
        {
            if (value)
            {
                _InAction = true;
                _StateTimer = 0f;
                _CurrentStateIndex = 0;
                _MultipleJumpCount = 0;
                _MultipleSlashCount = 0;
            }
            else
            {
                _InAction = false;
                _Velocity = Vector2.zero;
                _StateTimer = 0f;
                _CurrentStateIndex = 0;
                _MultipleJumpCount = 0;
                _MultipleSlashCount = 0;
            }
        }
    }

    override public void Awake()
    {
        base.Awake();
        _Collider = GetComponent<Collider2D>();
        if (_Rigidbody != null)
        {
            _Rigidbody.gravityScale = 0;
        }
    }

    override public void Start()
    {
        base.Start();
        _Target = EnemySharedData._PlayerTransform;
        SetDirection();
    }

    override public void OnEnable()
    {
        base.OnEnable();
        GameManager.OnBossFightTriggered += () => InAction = true;
    }

    override public void OnDisable()
    {
        base.OnDisable();
        GameManager.OnBossFightTriggered -= () => InAction = true;

        CancelInvoke(nameof(AttackDamage));
        CancelInvoke(nameof(SetJump));
        CancelInvoke(nameof(SetSlash));
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        int dir = _TargetDir != 0 ? _TargetDir : 1;
        Vector2 attackPos = (Vector2)transform.position + _AttackOffset * dir;
        Gizmos.DrawWireCube(attackPos, _AttackSize);
    }

    private void Update()
    {
        if (_HealthSystem != null && !_HealthSystem.IsAlive)
            return;

        GroundCheck();

        _Velocity.y = _IsGrounded ? -_Gravity : _Velocity.y - _Gravity * Time.deltaTime;

        if (_InAction)
        {
            CollisionCheck();
            SetDirection();
            UpdateState();
        }

        SetAnimations();
    }

    private void FixedUpdate()
    {
        if (_Velocity.sqrMagnitude > 0)
        {
            Vector2 pos = _Rigidbody.Slide(_Velocity, Time.fixedDeltaTime, _SlideMovement).position;
            _Rigidbody.MovePosition(pos);
        }
    }

    private void SetAnimations()
    {
        if (_Animator != null)
        {
            _Animator.SetBool("Grounded", _IsGrounded);
            _Animator.SetFloat("SpeedY", _Velocity.y);
        }

        if (_TargetDir != 0 && _Renderer != null)
            _Renderer.flipX = _TargetDir < 0;
    }

    private void SetDirection()
    {
        if (!HasTarget || !_IsGrounded)
            return;

        float diff = _Target.position.x - transform.position.x;
        diff = Mathf.Abs(diff) < 0.2f ? 0 : diff;
        _TargetDir = diff == 0 ? 0 : (diff > 0 ? 1 : -1);
    }

    private void CollisionCheck()
    {
        RaycastHit2D[] hits = new RaycastHit2D[2];
        if (HasCollider && _Collider.Cast(Vector2.right * _TargetDir, _GroundFilter, hits, 0.4f) > 0)
        {
            _Velocity.x = 0;
        }
    }

    private void GroundCheck()
    {
        bool wasGrounded = _IsGrounded;
        _IsGrounded = false;

        if (_Velocity.y > 0)
            return;

        RaycastHit2D[] hits = new RaycastHit2D[2];
        _IsGrounded = HasCollider && _Collider.Cast(Vector2.down, _GroundFilter, hits, 0.4f) > 0;

        if (!wasGrounded && _IsGrounded && CameraManager.HasInstance)
            CameraManager.Instance.ApplyShake(_LandShake, transform.position);
    }

    Vector2 CalculateJumpVelocity(Vector2 start, Vector2 end, float height)
    {
        float g = _Gravity; // gravity magnitude (positive)

        // Vertical velocity to reach fixed jump height
        float velocityY = Mathf.Sqrt(2 * g * height);

        // Times
        float timeUp = velocityY / g;
        float displacementY = start.y + height - end.y;
        float timeDown = Mathf.Sqrt(2 * displacementY / g);
        float totalTime = timeUp + timeDown;

        // Horizontal velocity (only X in 2D)
        float velocityX = (end.x - start.x) / totalTime;

        return new Vector2(velocityX, velocityY);
    }

    private void UpdateState()
    {
        if (!HasStates)
            return;

        _StateTimer += Time.deltaTime;
        switch (_StateLoop[_CurrentStateIndex].state)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.JumpAttack:
                UpdateJump(1);
                break;
            case State.MultipleJumpAttack:
                UpdateJump(4);
                break;
            case State.SlashAttack:
                UpdateSlash(5);
                break;
        }
    }

    private void StartState()
    {
        if (!HasStates)
            return;
        switch (_StateLoop[_CurrentStateIndex].state)
        {
            case State.Idle:
                StartIdle();
                break;
            case State.JumpAttack:
                StartJump();
                break;
            case State.MultipleJumpAttack:
                StartJump();
                break;
            case State.SlashAttack:
                StartSlash();
                break;
        }
    }

    private void NextState()
    {
        if (!HasStates)
            return;

        _CurrentStateIndex = (_CurrentStateIndex + 1) % _StateLoop.Length;
        _StateTimer = 0f;
        StartState();
    }

    private void AttackDamage()
    {
        int dir = _TargetDir != 0 ? _TargetDir : 1;
        Vector2 attackPos = (Vector2)transform.position + _AttackOffset * dir;
        Collider2D hit = Physics2D.OverlapBox(attackPos, _AttackSize, 0f, _AttackLayer);
        if (hit != null && hit.TryGetComponent(out HealthSystem health))
            health.TakeDamage(1);

#if UNITY_EDITOR
        // draw debug box
        Debug.DrawLine(attackPos - _AttackSize / 2, attackPos + new Vector2(_AttackSize.x / 2, -_AttackSize.y / 2), Color.red, 0.2f);
        Debug.DrawLine(attackPos - _AttackSize / 2, attackPos + new Vector2(-_AttackSize.x / 2, _AttackSize.y / 2), Color.red, 0.2f);
        Debug.DrawLine(attackPos + _AttackSize / 2, attackPos + new Vector2(_AttackSize.x / 2, -_AttackSize.y / 2), Color.red, 0.2f);
        Debug.DrawLine(attackPos + _AttackSize / 2, attackPos + new Vector2(-_AttackSize.x / 2, _AttackSize.y / 2), Color.red, 0.2f);
#endif

    }

    private void Attack()
    {
        _Animator.SetTrigger("Attack");
        CancelInvoke(nameof(AttackDamage));
        Invoke(nameof(AttackDamage), _AttackDelay);
    }

    private void SetSlash()
    {
        _MultipleSlashCount++;
        _IsSlashing = true;
    }

    private void StartSlash()
    {
        SetSlash();
        _MultipleSlashCount = 1;
    }

    private void UpdateSlash(int maxJump)
    {
        if (_IsGrounded && _IsSlashing)
        {
            _IsSlashing = false;
            Attack();
            if (_MultipleSlashCount < maxJump)
            {
                _Velocity = Vector2.zero;
                Invoke(nameof(SetSlash), _AttackAnimTime);
            }
            else
            {
                _MultipleSlashCount = 0;
                NextState();
            }
        }
    }

    private void SetJump()
    {
        _Velocity = CalculateJumpVelocity(_Rigidbody.position, _Target.position, _JumpForce);
        _MultipleJumpCount++;
        _IsJumping = true;
    }

    private void StartJump()
    {
        SetJump();
        _MultipleJumpCount = 1;
    }

    private void UpdateJump(int maxJump)
    {
        if (_IsGrounded && _IsJumping)
        {
            _IsJumping = false;
            Attack();
            if (_MultipleJumpCount < maxJump)
            {
                _Velocity = Vector2.zero;
                Invoke(nameof(SetJump), _AttackAnimTime);
            }
            else
            {
                _MultipleJumpCount = 0;
                NextState();
            }
        }
    }

    private void StartIdle() => _Velocity = Vector2.zero;

    private void UpdateIdle()
    {
        if (_StateLoop[_CurrentStateIndex].duration <= _StateTimer)
            NextState();
    }

    protected override void ResetEnemy()
    {
        CancelInvoke();

        InAction = false;
        _IsJumping = false;
        _IsSlashing = false;
        _IsGrounded = true;

        _Velocity = Vector2.zero;
        base.ResetEnemy();
    }

    protected override void OnDeath()
    {
        Debug.Log("False Knight defeated!");
        base.OnDeath();
        InAction = false;
        GameManager.OnBossDeathTriggered?.Invoke();
    }
}
