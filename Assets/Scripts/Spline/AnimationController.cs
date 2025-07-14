using UnityEngine;
using System.Collections;
using StarterAssets;

/// <summary>
/// プレイヤーのアニメーション制御を行うクラス
/// </summary>
public class AnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    //[SerializeField] private float jumpHeight_ = 1.2f;

    [SerializeField] private float nowGravity_ = 0.0f;
    [SerializeField] private float jumpingGravity_ = -60.0f;
    [SerializeField] private float fallingGravity_ = -90.0f;


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
    private int _animIDIsAir;
    private int _animIDOnSmashed;
    // Components
    private Animator _animator;
    private bool _hasAnimator;

    // State
    [SerializeField] public bool smashed_ = false;
    [SerializeField] private bool grounded_ = true;
    [SerializeField] private float verticalVelocity_;
    [SerializeField] private bool _isAir;
    private float _jumpTimeoutDelta;
    [SerializeField] private float _fallTimeoutDelta;
    public bool jump;
    [SerializeField] private StarterAssetsInputs inputs_;
    // Properties
    public bool Grounded 
    { 
        get { return grounded_; } 
        set 
        { 
            grounded_ = value;
        } 
    }
    public bool IsAir
    {
        get { return _isAir; }
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
        //inputs_.GetComponent<StarterAssetsInputs>();
        if (_hasAnimator)
        {
            AssignAnimationIDs();
        }

        // Reset timeouts
        _jumpTimeoutDelta = jumpTimeout_;
        _fallTimeoutDelta = fallTimeout_;

        inputs_.onReleaseJumpBtn += ChangeGravity;
        
    }
    private void ChangeGravity()
    {
        if(!grounded_)
        {
            nowGravity_ = fallingGravity_;
        }
    }
    private void Update()
    {
        JumpAndGravity();
        UpdateAnimations();
        jump = _animator.GetBool(_animIDJump);
    }
    
    public void ResetVerticalVelocity()
    {
        verticalVelocity_ = 0;
    }
    public void OnSmash()
    {
        _animator.SetBool(_animIDOnSmashed, true);
    }
    public void FinishSmash()
    {
        _animator.SetBool(_animIDOnSmashed, false);
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
        _animIDIsAir = Animator.StringToHash("IsAir");
        _animIDOnSmashed = Animator.StringToHash("Smash");
    }

    private void UpdateAnimations()
    {
        if (!_hasAnimator) return;

        // Air state check
        if (_isAir && grounded_)
        {
            Debug.Log("grounded_");
            _animator.SetBool(_animIDGrounded, true);
            _isAir = false;
        }
        else
        {
            _animator.SetBool(_animIDGrounded, false);
        }
        _animator.SetBool(_animIDIsAir, _isAir);
        _animator.SetBool(_animIDJump, inputs_.jump);
    }

    private void JumpAndGravity()
    {
        if (grounded_)
        {
            _fallTimeoutDelta = fallTimeout_;

            if (verticalVelocity_ > 0.1f)
            {
                _animator.SetBool(_animIDJump, true);
            }
            else
            {
                _animator.SetBool(_animIDJump, false);
            }
            _animator.SetBool(_animIDFreeFall, false);

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
                    Debug.Log("freefall");
                }
                _isAir = true;
            }
            inputs_.jump = false;

            if(verticalVelocity_ < 0.0f)
            {
                ChangeGravity();
            }
        }
        
        // Apply gravity
        if (_isAir)
        {
            verticalVelocity_ += nowGravity_ * Time.deltaTime;
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

        _animator.SetBool(_animIDJump, true);
        nowGravity_ = jumpingGravity_;
        verticalVelocity_ = Mathf.Sqrt(height * -2f * jumpingGravity_);
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