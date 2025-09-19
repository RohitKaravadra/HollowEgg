using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _MoveSpeed = 10f;
    [SerializeField] float _Acceleration = 10f;
    [SerializeField][Range(0.0001f, 0.1f)] float _MoveThreshold;
    [Space(5)]
    [SerializeField] float _JumpForce = 8f;
    [SerializeField] float _JumpGravity;
    [SerializeField] float _AirialDrag;
    [SerializeField][Range(1, 10)] int _MaxJumpCount;
    [SerializeField] float _CoyoteTime;
    [Space(5)]
    [SerializeField] float _Gravity;
    [SerializeField] float _MaxGravity;
    [Space(5)]
    [SerializeField][Range(1, 20)] int _MaxIteration;
    [SerializeField][Range(0, 1)] float _GroundDistance;
    [SerializeField][Range(0, 1)] float _CellingDistance;
    [SerializeField][Range(0, 1)] float _WallDistance;
    [Space(5)]
    [SerializeField][Range(0, 90)] float _GravitySlideAngle;
    [SerializeField][Range(0, 90)] float _SurfaceSlideAngle;
    [Space(5)]
    [SerializeField] LayerMask _GroundLayers;
    [SerializeField] float _RespawnTime;
    [Space(5)]
    [SerializeField] CapsuleCollider2D _Collider;
    [SerializeField] Animator _Animator;
    [SerializeField] Transform _Visuals;

    // Input variables
    Vector2 _MoveInput;

    Rigidbody2D _Rb;
    Rigidbody2D.SlideMovement _SlideData;

    ContactFilter2D _GroundFilter;
    RaycastHit2D[] _HitResults = new RaycastHit2D[2];
    Rigidbody2D _Ground = null;

    bool _IsAlive;
    bool _IsGrounded;
    bool _IsJumping;
    bool _IsMoving;

    bool _IsHeadCollide;
    bool _IsWallCollide;
    bool _CanJump;
    bool IsCoyote => !_IsJumping && Time.time - _LastGroundTime < _CoyoteTime;

    float _LastGroundTime;
    int _JumpCount = 0;

    int _XDirection = 1;
    Vector2 _Velocity = Vector2.zero;

    Vector2 _CheckpointPos;

    private void Awake()
    {
        _Rb = GetComponent<Rigidbody2D>();
        _IsAlive = true;
    }

    private void Start()
    {
        _CheckpointPos = transform.position;
        SetData();
        if (CameraManager.HasInstance)
            CameraManager.Instance.FollowTarget = transform;
    }

    private void OnEnable()
    {
        if (InputManager.HasInstance)
        {
            InputManager.Instance.Player.Jump.performed += ctx => OnJump(true);
            InputManager.Instance.Player.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
            InputManager.Instance.Player.Move.canceled += ctx => OnMove(Vector2.zero);
        }
    }

    private void OnDisable()
    {
        if (InputManager.HasInstance)
        {
            InputManager.Instance.Player.Jump.performed -= ctx => OnJump(true);
            InputManager.Instance.Player.Move.performed -= ctx => OnMove(ctx.ReadValue<Vector2>());
            InputManager.Instance.Player.Move.canceled -= ctx => OnMove(Vector2.zero);
        }
    }

    private void Update()
    {
        if (!_IsAlive)
            return;

        CheckCollision();                   // check head and foot collisions
        UpdateJump();                       // update jump data
        SetAnimations();                    // set animation states
        UpdateMovement(Time.deltaTime);     // update movement velocities
    }

    private void FixedUpdate()
    {
        if (!_IsAlive)
            return;
        Move(Time.fixedDeltaTime);  // apply movement
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(OnRespawn));
    }

    /// <summary>
    /// Set default Slide data
    /// </summary>
    private void SetData()
    {
        // inititalize variables
        _XDirection = 1;
        _Velocity = Vector2.zero;

        _LastGroundTime = Time.time;

        // Set ground filter for foot and head collision check
        _GroundFilter.useLayerMask = true;
        _GroundFilter.layerMask = _GroundLayers;

        // set slide data for movement collisions
        _SlideData.maxIterations = _MaxIteration;
        _SlideData.gravitySlipAngle = _GravitySlideAngle;
        _SlideData.surfaceSlideAngle = _SurfaceSlideAngle;

        _SlideData.gravity = Vector2.down;
        _SlideData.surfaceUp = Vector2.up;

        _SlideData.SetLayerMask(_GroundLayers);
        _SlideData.useSimulationMove = false;
    }

    private void SetYVelocity(float deltaTime)
    {
        // check for head collision
        if (_IsHeadCollide && _Velocity.y > 0)
        {
            _Velocity.y = 0;
            return;
        }

        float gravity = _Velocity.y > 0 ? _JumpGravity : _Gravity;

        // update gravity
        _Velocity.y = (_IsGrounded ? 0 : _Velocity.y) - gravity * deltaTime;
        _Velocity.y = Mathf.Clamp(_Velocity.y, -_MaxGravity, _MaxGravity);
    }

    private void SetXVelocity(float deltaTime)
    {
        // check if jumping and collides 
        if (!_IsGrounded && _IsWallCollide)
        {
            _Velocity.x = -_Velocity.x * 0.3f;
            return;
        }

        // update speed and acceleration
        float speed = (_IsMoving ? _MoveSpeed : 0);

        // udate direction
        _XDirection = _IsMoving ? (_MoveInput.x > 0 ? 1 : -1) : _XDirection;
        speed *= _XDirection;

        // set velocity
        _Velocity.x = Mathf.Abs(_Velocity.x - speed) > _MoveThreshold ?
            Mathf.Lerp(_Velocity.x, speed, deltaTime * _Acceleration)
                : speed;

        // calculate air drag
        if (!_IsGrounded)
            _Velocity.x = Mathf.Lerp(_Velocity.x, 0, deltaTime * _AirialDrag);

        // swap visuals if needed
        if (_Velocity.x != 0)
            _Visuals.localScale = new Vector3(_Velocity.x < 0 ? -1 : 1, 1, 1);
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
        Vector2 pos = _Rb.Slide(_Velocity, deltaTime, _SlideData).position;

        // apply moving objects velocity
        if (_Ground != null)
            pos.x += _Ground.linearVelocityX * deltaTime;

        // update new position of rigidbody
        _Rb.MovePosition(pos);
    }

    void UpdateJump()
    {
        // reset jumping
        _IsJumping = _IsJumping && !_IsGrounded;

        // reset jump count on grounded
        if (_IsGrounded && _JumpCount > 0)
            _JumpCount = 0;

        _CanJump = _IsGrounded || (_IsJumping && _JumpCount < _MaxJumpCount) || IsCoyote;
    }

    void SetJump()
    {
        if (!_CanJump)
            return;

        _Velocity.y = _JumpForce;
        _IsJumping = true;
        _JumpCount++;
    }

    void CheckCollision()
    {
        bool wasGrounded = _IsGrounded;

        _IsGrounded = _Velocity.y <= 0 && _Collider.Cast(Vector2.down, _GroundFilter, _HitResults, _GroundDistance) > 0;     // foot collision

        if (!_IsGrounded || !_HitResults[0].transform.TryGetComponent(out _Ground))
            _Ground = null;

        _IsHeadCollide = _Velocity.y > 0 && _Collider.Cast(Vector2.up, _GroundFilter, _HitResults, _CellingDistance) > 0;    // head collision
        _IsWallCollide = _Collider.Cast(Vector2.right * _XDirection, _GroundFilter, _HitResults, _WallDistance) > 0;         // wall collision

        if (wasGrounded && !_IsGrounded)
            _LastGroundTime = Time.time;
    }

    private void OnDeath()
    {
        if (!_IsAlive)
            return;

        _IsAlive = false;
        _Rb.simulated = false;

        if (InputManager.HasInstance)
            InputManager.Instance.Player.Disable();

        Invoke(nameof(OnRespawn), _RespawnTime);
    }

    private void OnRespawn()
    {
        transform.position = _CheckpointPos;

        _IsAlive = true;
        _Rb.simulated = true;

        if (InputManager.Instance != null)
            InputManager.Instance.Player.Enable();
    }

    void SetAnimations()
    {
        if (_Animator == null)
            return;

        _Animator.SetBool("Move", _IsMoving);
        _Animator.SetBool("Grounded", _IsGrounded);
    }

    private void OnJump(bool _val)
    {
        if (_val) SetJump();
    }

    private void OnMove(Vector2 value) => _MoveInput = value;

    private void OnDrawGizmos()
    {
        //debug head collision ray
        if (_Collider != null)
        {
            float hBody = _Collider.size.y / 2;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_Collider.transform.position + Vector3.up * hBody, Vector2.up * _CellingDistance);
            Gizmos.DrawRay(_Collider.transform.position + Vector3.down * hBody, Vector2.down * _GroundDistance);
            Gizmos.DrawRay(_Collider.transform.position + (_Collider.size.x * _XDirection * Vector3.right / 2),
                _WallDistance * _XDirection * Vector2.right);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Danger"))
        {
            OnDeath();
        }
    }

    private void OnCheckPoint(Vector2 pos) => _CheckpointPos = pos;
}
