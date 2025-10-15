using System;
using UnityEngine;
using PlayerController;

namespace PlayerControllerNamespace // Changed to avoid class/namespace name clash
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        private bool _onWall;
        private int _wallDirection;
        private bool _dashing;
        private float _dashStartTime;
        private float _lastDashTime;
        private bool _dashBuffered;
        private float _timeDashWasBuffered;
        private float _lastFacingDirection = 1f;
        private bool CanDash => !_dashing && _time > _lastDashTime + _stats.DashCooldown;
        private bool HasBufferedDash => _dashBuffered && _time < _timeDashWasBuffered + _stats.DashBuffer;
        private bool _wallSliding;
        private Vector3 _startingPosition;

        #region Interface
        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazard"))
        {
            Debug.Log("Hazard touched! Resetting position.");
            transform.position = _startingPosition;
        }
    }
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _startingPosition = transform.position;
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                DashPressed = Input.GetKeyDown(KeyCode.LeftShift)
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _dashBuffered = true;
                _timeDashWasBuffered = _time;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();
            HandleDash();
            ApplyMovement();
            HandleWallSlide();
        }

        private void HandleDash()
        {
            // Store input for buffering
            if (_frameInput.DashPressed)
            {
                _dashBuffered = true;
                _timeDashWasBuffered = _time;
            }

            // Start dash if allowed
            if ((HasBufferedDash || _frameInput.DashPressed) && CanDash)
            {
                _dashing = true;
                _dashStartTime = _time;
                _lastDashTime = _time;
                _dashBuffered = false; // Clear buffer on use
            }

            // End dash if duration exceeded
            if (_dashing && _time > _dashStartTime + _stats.DashDuration)
            {
                _dashing = false;
            }

            // Apply dash movement
            if (_dashing)
            {
                float dashDir = _frameInput.Move.x != 0 ? Mathf.Sign(_frameInput.Move.x) : _lastFacingDirection;
                _frameVelocity = new Vector2(dashDir * _stats.DashSpeed, 0f);
                return; // Skip normal movement while dashing
            }
        }

        #region Collisions

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }
            // Wall checks
            bool wallLeft = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.left, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool wallRight = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.right, _stats.GrounderDistance, ~_stats.PlayerLayer);

            _onWall = (!_grounded && (wallLeft || wallRight));
            _wallDirection = wallLeft ? -1 : (wallRight ? 1 : 0);
            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            // Jump Buffer
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0)
                _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            // Coyote Jump and Wall Jump criteria
            if (_grounded || CanUseCoyote)
            {
                ExecuteJump();
            }
            else if (_onWall)
            {
                ExecuteWallJump();
            }

            _jumpToConsume = false;
        }

        private void HandleWallSlide()
        {
            _wallSliding = false;
            // Checks if the player is in contact with the Wall and moving in the direction of it
            if (_onWall && !_grounded && _frameVelocity.y < 0 && _frameInput.Move.x == _wallDirection)
            {
            //if so then the Wallslide state becomes true
                _wallSliding = true;
                _frameVelocity.y = Mathf.Max(_frameVelocity.y, -_stats.WallSlideSpeed);
            }
        }
        private void ExecuteWallJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            // Jump away from wall and up
            _frameVelocity.x = -_wallDirection * _stats.MaxSpeed;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }
        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }

            if (_frameInput.Move.x != 0)
            {
                _lastFacingDirection = Mathf.Sign(_frameInput.Move.x);

                // Flip the character’s sprite by changing localScale
                Vector3 scale = transform.localScale;
                scale.x = _lastFacingDirection;
                transform.localScale = scale;
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else if (_onWall && _frameVelocity.y < 0)
            {
                _frameVelocity.y = Mathf.Max(_frameVelocity.y, -_stats.MaxFallSpeed * 0.3f);
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0)
                    inAirGravity *= _stats.JumpEndEarlyGravityModifier;

                _frameVelocity.y = Mathf.MoveTowards(
                _frameVelocity.y,
                -_stats.MaxFallSpeed,
                inAirGravity * Time.fixedDeltaTime
);
            }
        }


        #endregion
        private void ApplyMovement() => _rb.velocity = _frameVelocity;
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;

        public bool DashPressed;
        public Vector2 Move;

    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }

    #region Hazards and Enemies
    public class HazardController : MonoBehaviour
    {
        private Vector3 _startingPosition;
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"Triggered by: {other.name}, Tag: {other.tag}");
            if (other.CompareTag("Hazard"))

                if (other.CompareTag("Hazard"))
                {
                    ResetPlayerPosition();
                }
        }

        private void ResetPlayerPosition()
        {
            transform.position = _startingPosition;
        }
    }
#endregion
}