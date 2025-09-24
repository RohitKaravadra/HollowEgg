using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Testing Parameters")]
    [SerializeField] bool _GodMode = false;
    [Space(10)]
    [SerializeField] float _MoveSpeed = 10f;
    [SerializeField] float _Acceleration = 10f;
    [SerializeField][Range(0.0001f, 0.1f)] float _MoveThreshold;
    [Space(10)]
    [SerializeField] float _DashSpeed = 20f;
    [SerializeField] float _DashDistance = 2f;
    [SerializeField] float _DashCooldown = 2f;
    [Space(10)]
    [SerializeField] float _JumpForce = 8f;
    [SerializeField] float _AirialDrag;
    [SerializeField][Range(1, 10)] int _MaxJumpCount;
    [SerializeField] float _CoyoteTime;
    [Space(10)]
    [SerializeField] float _UpGravity;
    [SerializeField] float _DownGravity;
    [SerializeField] float _MaxGravity;
    [Space(10)]
    [SerializeField][Range(1, 20)] int _MaxIteration;
    [SerializeField][Range(0, 1)] float _GroundDistance;
    [SerializeField][Range(0, 1)] float _CellingDistance;
    [SerializeField][Range(0, 1)] float _WallDistance;
    [Space(10)]
    [SerializeField] bool _UseTriggerColliders = false;
    [SerializeField][Range(0, 90)] float _GravitySlideAngle;
    [SerializeField][Range(0, 90)] float _SurfaceSlideAngle;
    [SerializeField] LayerMask _GroundLayers;
    [Space(10)]
    [SerializeField] float _InvincibilityTime = 0.5f;
    [SerializeField] float _RespawnTime;
    [SerializeField] float _DamageForce;
    [SerializeField] ShakeData _CameraShake;
    [Space(10)]
    [SerializeField] PlayerAttack _PlayerAttack;
    [SerializeField] PlayerAudio _PlayerAudio;
    [SerializeField] CapsuleCollider2D _Collider;
    [SerializeField] Animator _Animator;
    [SerializeField] Transform _Visuals;

    // Input variables
    Vector2 _MoveInput;

    Rigidbody2D _Rb;
    Rigidbody2D.SlideMovement _SlideData;
    HealthSystem _HealthSystem;

    ContactFilter2D _GroundFilter;
    Rigidbody2D _Ground = null;

    bool _IsGrounded;
    bool _IsMoving;

    bool _IsJumping;
    bool _CanJump;
    int _JumpCount = 0;
    float _LastGroundTime;

    bool _IsDashing;
    float _LastDashTime;
    float _CurDashDist;

    bool _IsCeilingAbove;
    bool _IsWallAhead;

    float _lastHitTime;

    private bool DashEnabled { get; set; }
    private bool DoubleJumpEnabled { get; set; }

    bool CanDash => (_GodMode || DashEnabled) && _MoveInput.x != 0 && Time.time - _LastDashTime > _DashCooldown;
    bool CanAttack => _PlayerAttack != null && _PlayerAttack.CanAttack && !_IsDashing;
    bool IsCoyote => !_IsJumping && Time.time - _LastGroundTime < _CoyoteTime;
    bool HasCollider => _Collider != null;
    bool IsInvincible => _GodMode || _IsDashing || Time.time - _lastHitTime < _InvincibilityTime;
    public bool IsAlive => _HealthSystem != null && _HealthSystem.IsAlive;

    int _XDirection = 1;
    Vector2 _Velocity = Vector2.zero;
    Vector2 _DamageResponseForce;
    Vector2 _CheckpointPos;

    private void Awake()
    {
        _Rb = GetComponent<Rigidbody2D>();
        _HealthSystem = GetComponent<HealthSystem>();

        EnemySharedData._PlayerTransform = transform;
        if (HasCollider) _Collider.isTrigger = _UseTriggerColliders;
    }

    private void Start()
    {
        SetData();

        if (CameraManager.HasInstance) CameraManager.Instance.FollowTarget = transform;

        _HealthSystem.Reset();
        UIManager.OnHealthUpdate?.Invoke(_HealthSystem.Health, _HealthSystem.MaxHealth);
    }

    private void OnEnable()
    {
        GameManager.OnPlayerRespawn += OnRespawn;

        if (InputManager.HasInstance)
        {
            InputManager.Instance.BindPlayerAttack(Attack, true);
            InputManager.Instance.BindPlayerDash(OnDash, true);
            InputManager.Instance.BindPlayerJump(OnJump, true);
            InputManager.Instance.BindPlayerMove(OnMove, true);
        }

        if (_HealthSystem != null)
            _HealthSystem.OnDeath.AddListener(OnDeath);
    }

    private void OnDisable()
    {
        GameManager.OnPlayerRespawn -= OnRespawn;

        if (InputManager.HasInstance)
        {
            InputManager.Instance.BindPlayerAttack(Attack, false);
            InputManager.Instance.BindPlayerDash(OnDash, false);
            InputManager.Instance.BindPlayerJump(OnJump, false);
            InputManager.Instance.BindPlayerMove(OnMove, false);
        }

        if (_HealthSystem != null)
            _HealthSystem.OnDeath.RemoveListener(OnDeath);
    }

    private void Update()
    {
        CheckCollision();                   // check head and foot collisions
        UpdateJump();                       // update jump data
        UpdateMovement(Time.deltaTime);     // update movement velocities
        SetAnimations();                    // set animation states
        SetVisuals();                       // set player visuals

        if (_IsGrounded && _IsMoving)
            _PlayerAudio.PlayFootStep();
    }

    private void FixedUpdate() => Move(Time.fixedDeltaTime);  // apply movement

    private void OnDestroy() => CancelInvoke();

    private void OnDrawGizmos()
    {
        if (_Collider != null)
        {
            float hBody = _Collider.size.y / 2;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_Collider.transform.position + Vector3.up * hBody, Vector2.up * _CellingDistance);
            Gizmos.DrawRay(_Collider.transform.position + Vector3.down * hBody, Vector2.down * _GroundDistance);
            Gizmos.DrawRay(_Collider.transform.position + (_Collider.size.x * _XDirection * Vector3.right / 2),
                _WallDistance * _XDirection * Vector2.right);
        }

        _PlayerAttack?.OnDrawGizmos(transform.position, ComputeAttackDir());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Danger"))
            ApplyDamage(100);
        else if (collision.CompareTag("Checkpoint"))
        {
            OnCheckPoint(collision.transform.position);
            Checkpoint.OnCheckpointHit?.Invoke(collision.transform);
        }
        else if (collision.CompareTag("Consumable"))
        {
            if (collision.TryGetComponent(out Consumable ability))
            {
                DashEnabled |= ability.IsDash;
                DoubleJumpEnabled |= ability.IsDoubleJump;
                ability.OnConsume();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsInvincible || !IsAlive) return;
        if (collision.CompareTag("Enemy"))
            OnHit(collision.transform.position);
    }

    private void SetData()
    {
        // set initial checkpoint
        _CheckpointPos = transform.position;

        // inititalize variables
        _XDirection = 1;
        _Velocity = Vector2.zero;

        _LastGroundTime = Time.time;
        _LastDashTime = Time.time;

        // Set ground filter for foot and head collision check
        _GroundFilter.useLayerMask = true;
        _GroundFilter.layerMask = _GroundLayers;
        _GroundFilter.useTriggers = _UseTriggerColliders;

        // set slide data for movement collisions
        _SlideData.maxIterations = _MaxIteration;
        _SlideData.gravitySlipAngle = _GravitySlideAngle;
        _SlideData.surfaceSlideAngle = _SurfaceSlideAngle;

        _SlideData.gravity = Vector2.down;
        _SlideData.surfaceUp = Vector2.up;

        _SlideData.SetLayerMask(_GroundLayers);
        _SlideData.useSimulationMove = false;
        _SlideData.useNoMove = true;
        _SlideData.useAttachedTriggers = _UseTriggerColliders;
    }

    private void SetYVelocity(float deltaTime)
    {
        // check for head collision
        if (_IsCeilingAbove && _Velocity.y > 0)
        {
            _Velocity.y = 0;
            return;
        }

        float gravity = _Velocity.y > 0 ? _UpGravity : _DownGravity;

        // update gravity
        _Velocity.y = (_IsGrounded ? 0 : _Velocity.y) - gravity * deltaTime;
        _Velocity.y = Mathf.Clamp(_Velocity.y, -_MaxGravity, _MaxGravity);
    }

    private void SetXVelocity(float deltaTime)
    {
        // udate direction
        _XDirection = _IsMoving ? (_MoveInput.x > 0 ? 1 : -1) : _XDirection;

        // check if jumping and collides 
        if (!_IsGrounded && _IsWallAhead)
        {
            _Velocity.x = 0;
            if (_IsDashing) ResetDash();
            return;
        }

        if (_IsDashing)
        {
            if (_IsWallAhead)
                ResetDash();
            else if (UpdateDash())
                return;
        }

        // update speed and acceleration
        float speed = (_IsMoving ? _MoveSpeed : 0);
        speed *= _XDirection;

        // set velocity
        _Velocity.x = Mathf.Abs(_Velocity.x - speed) > _MoveThreshold ?
            Mathf.Lerp(_Velocity.x, speed, deltaTime * _Acceleration)
                : speed;

        // calculate air drag
        if (!_IsGrounded)
            _Velocity.x = Mathf.Lerp(_Velocity.x, 0, deltaTime * _AirialDrag);
    }

    private void UpdateMovement(float deltaTime)
    {
        _IsMoving = _MoveInput.x != 0;
        SetYVelocity(deltaTime);    // Set vertical velocity
        SetXVelocity(deltaTime);    // Set horizontal velocity
    }

    private void Move(float deltaTime)
    {
        // set ground snap distance to prevent snapping when jump
        _SlideData.surfaceAnchor = new Vector2(0, _IsJumping ? 0 : -_GroundDistance);

        // update position
        Vector2 pos = _Rb.Slide(_Velocity + GetDamageForce(), deltaTime, _SlideData).position;

        // apply moving objects velocity
        if (_Ground != null)
            pos.x += _Ground.linearVelocityX * deltaTime;

        // update dash distance
        if (_IsDashing)
            _CurDashDist += Mathf.Abs(transform.position.x - pos.x);

        // update new position of rigidbody
        _Rb.MovePosition(pos);
    }

    private void SetVisuals()
    {
        if (_Velocity.x != 0)
            _Visuals.localScale = new Vector3(_Velocity.x < 0 ? -1 : 1, 1, 1);
    }

    void SetJump()
    {
        if (!_CanJump)
            return;

        _Velocity.y = _JumpForce;
        _IsJumping = true;
        _JumpCount++;
    }

    void UpdateJump()
    {
        // reset jumping
        _IsJumping = _IsJumping && !_IsGrounded;

        // reset jump count on grounded
        if (_IsGrounded && _JumpCount > 0)
            _JumpCount = 0;

        _CanJump = _IsGrounded || IsCoyote || ((DoubleJumpEnabled || _GodMode) && _IsJumping && _JumpCount < _MaxJumpCount);
    }

    void SetDash()
    {
        if (_IsDashing || !CanDash)
            return;

        _IsDashing = true;
        _LastDashTime = Time.time;

        if (_Animator != null)
            _Animator.SetBool("Dash", true);

        _PlayerAudio.PlayDash();
    }

    void ResetDash()
    {
        _IsDashing = false;
        _CurDashDist = 0;
        if (_Animator != null)
            _Animator.SetBool("Dash", false);
    }

    bool UpdateDash()
    {
        if (_CurDashDist >= _DashDistance || Time.time - _LastDashTime > 1f) // temporal fix for stuck dash
        {
            ResetDash();
            return false;
        }

        float speed = _DashSpeed * _XDirection;
        // set velocity
        _Velocity.x = Mathf.Abs(_Velocity.x - speed) > _MoveThreshold ?
            Mathf.Lerp(_Velocity.x, speed, Time.deltaTime * _Acceleration)
                : speed;

        return true;
    }

    private Vector2 ComputeAttackDir()
    {
        Vector2 dir = new(_XDirection, _MoveInput.y);
        return dir;
    }

    private void Attack()
    {
        if (CanAttack)
        {
            if (_Animator != null)
                _Animator.SetTrigger("Attack");

            _PlayerAudio.PlaySlash();
            if (_PlayerAttack.Attack(transform.position, ComputeAttackDir()))
            {
                if (CameraManager.HasInstance)
                    CameraManager.Instance.ApplyShake(_CameraShake);
            }
        }
    }

    void CheckCollision()
    {
        bool wasGrounded = _IsGrounded;
        _IsGrounded = _IsWallAhead = _IsCeilingAbove = false;

        if (HasCollider)
        {
            Vector2 pos = transform.position;
            pos += _Collider.offset;

            RaycastHit2D[] _HitResults = new RaycastHit2D[2];
            if (Physics2D.CapsuleCast(pos, _Collider.size, _Collider.direction, 0, Vector2.down, _GroundFilter, _HitResults, _GroundDistance) > 0)
                _IsGrounded = _Velocity.y <= 0 && _Collider.Cast(Vector2.down, _GroundFilter, _HitResults, _GroundDistance) > 0;     // foot collision

            if (!_IsGrounded || !_HitResults[0].transform.TryGetComponent(out _Ground))
                _Ground = null;

            if (Physics2D.CapsuleCast(pos, _Collider.size, _Collider.direction, 0, Vector2.up, _GroundFilter, _HitResults, _CellingDistance) > 0)
                _IsCeilingAbove = _Velocity.y > 0 && _HitResults[0].point.y > transform.position.y; // head collision

            _IsWallAhead = Physics2D.CapsuleCast(pos, _Collider.size * 0.9f, _Collider.direction, 0, Vector2.right * _XDirection,
                _GroundFilter, _HitResults, _WallDistance) > 0; // wall collision

            if (wasGrounded && !_IsGrounded)
                _LastGroundTime = Time.time;

            if (!wasGrounded && _IsGrounded)
                _PlayerAudio.PlayLand();
        }
    }

    void SetAnimations()
    {
        if (_Animator == null)
            return;

        _Animator.SetBool("Move", _IsMoving);
        _Animator.SetBool("Grounded", _IsGrounded);
        _Animator.SetFloat("SpeedY", _Velocity.y);
        _Animator.SetBool("Invincible", IsInvincible);
    }

    private Vector2 GetDamageForce()
    {
        Vector2 force = _DamageResponseForce;
        _DamageResponseForce = Vector2.zero;
        return force;
    }

    private void ApplyDamage(int damage)
    {
        if (IsInvincible) return;
        if (_HealthSystem != null)
        {
            _HealthSystem.TakeDamage(damage);
            UIManager.OnHealthUpdate?.Invoke(_HealthSystem.Health, _HealthSystem.MaxHealth);
        }
        if (CameraManager.Instance)
            CameraManager.Instance.ApplyShake(_CameraShake);
    }

    private void OnHit(Vector2 hitPos)
    {
        if (IsInvincible) return;

        ApplyDamage(1);

        Vector2 dir = ((Vector2)transform.position - hitPos).normalized;
        _DamageResponseForce += dir * _DamageForce;
        _lastHitTime = Time.time;
    }

    private void OnDeath()
    {
        if (InputManager.HasInstance)
            InputManager.Instance.SetPlayerInput(false);

        GameManager.OnPlayerDead?.Invoke();
        // Invoke(nameof(OnRespawn), _RespawnTime);
    }

    private void OnRespawn()
    {
        _HealthSystem.Reset();
        UIManager.OnHealthUpdate?.Invoke(_HealthSystem.Health, _HealthSystem.MaxHealth);

        _Rb.simulated = false;
        transform.position = _CheckpointPos;
        _Rb.simulated = true;

        if (InputManager.Instance != null)
            InputManager.Instance.SetPlayerInput(true);
    }

    private void OnCheckPoint(Vector2 pos) => _CheckpointPos = pos;

    #region Inputs
    void OnDash() => SetDash();

    private void OnJump(bool _val)
    {
        if (_val) SetJump();
    }

    private void OnMove(Vector2 value) => _MoveInput = new(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y));
    #endregion
}
