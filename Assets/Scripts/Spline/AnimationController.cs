using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーのアニメーション制御を行うクラス
/// </summary>
public class AnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float jumpHeight_ = 1.2f;
    [SerializeField] private float gravity_ = -15.0f;
    [SerializeField] private float jumpTimeout_ = 0.50f;
    [SerializeField] private float fallTimeout_ = 0.15f;

    [Header("Audio")]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDTakeDamage;
    private int _animIDDying;
    private int _animIDCanInputReact;

    // Components
    private Animator _animator;
    private bool _hasAnimator;

    // State
    [SerializeField] private bool grounded_ = true;
    [SerializeField] private float verticalVelocity_;
    private bool _isAir;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Properties
    public bool Grounded 
    { 
        get { return grounded_; } 
        set 
        { 
            grounded_ = value;
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, grounded_);
            }
        } 
    }

    public float VerticalVelocity => verticalVelocity_;

    public bool IsStunned 
    { 
        get 
        {
            return _hasAnimator && !_animator.GetBool(_animIDCanInputReact);
        } 
    }

    private void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        
        if (_hasAnimator)
        {
            AssignAnimationIDs();
        }

        // Reset timeouts
        _jumpTimeoutDelta = jumpTimeout_;
        _fallTimeoutDelta = fallTimeout_;
    }

    private void Update()
    {
        JumpAndGravity();
        UpdateAnimations();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDTakeDamage = Animator.StringToHash("TakeDamage");
        _animIDCanInputReact = Animator.StringToHash("CanInputReact");
        _animIDDying = Animator.StringToHash("Dying");
    }

    private void UpdateAnimations()
    {
        if (!_hasAnimator) return;

        // Air state check
        if (_isAir && grounded_)
        {
            Debug.Log("Landed");
            _isAir = false;
        }
    }

    private void JumpAndGravity()
    {
        if (grounded_)
        {
            _fallTimeoutDelta = fallTimeout_;

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            if (verticalVelocity_ < 0.0f)
            {
                verticalVelocity_ = 0.0f;
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = jumpTimeout_;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
                _isAir = true;
            }
        }

        // Apply gravity
        if (!grounded_)
        {
            verticalVelocity_ += gravity_ * Time.deltaTime;
        }
    }

    public void TakeDamage()
    {
        if (_hasAnimator)
        {
            _animator.SetTrigger(_animIDTakeDamage);
        }
        else
        {
            Debug.LogWarning("Animator is not assigned, cannot play TakeDamage animation.");
        }
    }

    public void Dying()
    {
        if (_hasAnimator)
        {
            _animator.SetTrigger(_animIDDying);
            _animator.SetBool(_animIDCanInputReact, false);
        }
        else
        {
            Debug.LogWarning("Animator is not assigned, cannot play Dying animation.");
        }
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, Mathf.Abs(moveInput.x));
        }
    }

    public void AddVerticalForce(float height)
    {
        if (IsStunned) return;

        if (_hasAnimator)
        {
            _animator.SetBool(_animIDJump, true);
        }
        
        verticalVelocity_ = Mathf.Sqrt(height * -2f * gravity_);
    }

    // Animation Events
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, FootstepAudioVolume);
        }
    }
}