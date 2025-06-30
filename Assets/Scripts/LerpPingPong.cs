using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum MoveState
{
    WAIT,
    GOING,
    COMBACKING
}
[RequireComponent(typeof(EaseInterpolator))]
public class LerpPingPong : MonoBehaviour
{
    [Header("��Ԃ̎n�_")]
    [SerializeField] private Transform _from;

    [Header("���`��Ԃ̏I�_")]
    [SerializeField] private Transform _to;
    
    [Header("�s���Ɋ|���鎞��")]
    [SerializeField] private float goingTime = 1;
    
    [Header("�A��Ɋ|���鎞��")]
    [SerializeField] private float comeBackTime = 3;
    
    [Header("�s���n�߂�܂ł̎���")]
    [SerializeField] private float againGoingTime = 2f;

    [Header("�߂�n�߂�܂ł̎���")]
    [SerializeField] private float againComebackTime = 2f;

    [Header("�ŏ��A�����o���܂ł̎���")]
    [SerializeField] private float firstTime = 0;

    [SerializeField] private MoveState prevState_;
    [SerializeField] private MoveState currentState_;

    public StateMachine<MoveState> stateMachine_;
    
    //�Փˎ��Ɋ|�����
    [SerializeField] private float power = 1f;
    private Rigidbody rb;
    
    //�s������������΂����ۂ�
    private bool addForceOnlyGoing = true;

    EaseInterpolator interpolator;
    private void Start()
    {
        power = power * this.GetComponent<Rigidbody>().mass;
        rb = this.GetComponent<Rigidbody>();
        interpolator = GetComponent<EaseInterpolator>();
        interpolator.onFinished_ += OnFinished;
        interpolator.from_ = _from.position;
        interpolator.to_ = _to.position;
        

        InitializeStateMachine();

        stateMachine_.Start(MoveState.WAIT);
        
        
        
    }
    private void InitializeStateMachine()
    {
        stateMachine_ = new StateMachine<MoveState>();

        //WAIT��Ԃ̐ݒ�
        /*�������Ȃ�*/
        stateMachine_.RegisterState(MoveState.WAIT).SetCallbacks(
            onEntry: () => 
            {
                switch (stateMachine_.PrevState)
                {
                    case MoveState.GOING:
                        StartCoroutine(delay(againComebackTime, MoveState.COMBACKING));
                        break;
                    case MoveState.COMBACKING:
                        StartCoroutine(delay(againGoingTime, MoveState.GOING));
                        break;
                    case MoveState.WAIT:
                        
                        StartCoroutine(delay(firstTime, MoveState.GOING));
                        break;
                    default:
                        
                        StartCoroutine(delay(firstTime, MoveState.GOING));
                        break;

                }
                
            },
            onUpdate: () => 
            {
                //interpolator.UpdateTime();
            },
            onExit: () => { }
            );
        
       
        //stateMachine_[MoveState.WAIT].AddTransition

        stateMachine_.RegisterState(MoveState.GOING).SetCallbacks(
            onEntry: () =>
            {
                //interpolator.from_ = _from.position;
                //interpolator.to_ = _to.position;
                interpolator.duration = goingTime;
                
                interpolator.isReverse_ = false;
                interpolator.Reset();
            },
            onUpdate: () =>
            {
                interpolator.UpdateTime();
                Move();
            },
            onExit: () =>
            {
                
            }).
            AddTransition(MoveState.WAIT, ref interpolator.onFinished_);

        stateMachine_.RegisterState(MoveState.COMBACKING).SetCallbacks(
            onEntry: () =>
            {
                
                interpolator.duration = comeBackTime;
                
                interpolator.isReverse_ = true;
                interpolator.Reset();
            },
            onUpdate: () =>
            {
                interpolator.UpdateTime();
                Move();
            },
            onExit: () =>
            {
                
            }).
            AddTransition(MoveState.WAIT,ref interpolator.onComeback_);
    }
    private async Task<bool> Wait(int delay)
    {
        await Task.Delay(delay * 1000).ConfigureAwait(false);
        return true;
    }
    
    
    private void Initialize()
    {
        Debug.Log("Initialize");
        interpolator.Reset();
        Vector3 from = new Vector3();
        Vector3 to = new Vector3();
        float duration = 0;
        switch (currentState_)
        {
            case MoveState.GOING:
                from = _from.position;
                to = _to.position;
                duration = goingTime;

                break;
            case MoveState.COMBACKING:
                from = _to.position;
                to = _from.position;
                duration = comeBackTime;
                break;
            default:
                break;
        }
        interpolator.from_ = from;
        interpolator.to_ = to;
        interpolator.duration = duration;
    }
    private void Update()
    {
        
        stateMachine_.UpdateCurrent();
        currentState_ = stateMachine_.CurrentState;
        prevState_ = stateMachine_.PrevState;
    }
    

    private void OnFinished()
    {
        
    }
    
     IEnumerator delay(float time,MoveState nextState)
    {
        yield return new WaitForSeconds(time);
        stateMachine_.TransitionTo(nextState);
    }
   
    private void Move()
    {
        Vector3 newPosition = interpolator.Interpolation();
        rb.MovePosition(newPosition);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // �ڐG�����I�u�W�F�N�g��Rigidbody���擾
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        // Rigidbody�����݂���ꍇ�A�͂�������
        if (rb != null && collision.gameObject.CompareTag("Player"))
        {
            if (addForceOnlyGoing == false || currentState_ != MoveState.WAIT)
            {
                // �͂�����������Ƌ�����ݒ�
                Vector3 forceDirection = transform.forward;
                //float forceMagnitude = 10.0f;
                Debug.Log("AddForce");
                // �͂�������
                rb.AddForce(forceDirection * power, ForceMode.Impulse);
            }
        }
    }
    
    
}
