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
    [Header("補間の始点")]
    [SerializeField] public Vector3 _from;

    [Header("線形補間の終点")]
    [SerializeField] public Vector3 _to;
    
    [Header("行きに掛かる時間")]
    [SerializeField] private float goingTime = 1;
    
    [Header("帰りに掛かる時間")]
    [SerializeField] private float comeBackTime = 3;
    
    [Header("行き始めるまでの時間")]
    [SerializeField] private float againGoingTime = 2f;

    [Header("戻り始めるまでの時間")]
    [SerializeField] private float againComebackTime = 2f;

    [Header("最初、動き出すまでの時間")]
    [SerializeField] private float firstTime = 0;

    [SerializeField] private MoveState currentState_;

    public MoveState CurrentState
    {
        get { return currentState_; }
    }
    public StateMachine<MoveState> stateMachine_;
    
    private Rigidbody rb;
    
    [SerializeField]EaseInterpolator interpolator;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        interpolator = GetComponent<EaseInterpolator>();
        stateMachine_ = new StateMachine<MoveState>();
    }
    private void Start()
    {
    }
    private void InitializeStateMachine()
    {

        //WAIT状態の振る舞い
        /*何もしない*/
        stateMachine_.RegisterState(MoveState.WAIT).SetCallbacks(
            onEntry: () => 
            {
                // 前回の状態を参考にして一定時間待機、次の状態へ遷移
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
            },
            onExit: () => { }
            );
        
       
        // GOING状態の振る舞い
        stateMachine_.RegisterState(MoveState.GOING).SetCallbacks(
            onEntry: () =>
            {
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

        // COMBACKING状態の振る舞い
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
    
    
    /// <summary>
    /// 状態機を初期化し、始点と終点の座標を設定して往復移動を開始する
    /// </summary>
    public void StartPingPong()
    {
        InitializeStateMachine();
        interpolator.onFinished_ += OnFinished;
        interpolator.from_ = _from;
        interpolator.to_ = _to;

        stateMachine_.Start(MoveState.WAIT);
    }
    private void Update()
    {
        stateMachine_.UpdateCurrent();
        currentState_ = stateMachine_.CurrentState;
    }
    

    private void OnFinished()
    {
        
    }
    
     IEnumerator delay(float time,MoveState nextState)
    {
        yield return new WaitForSeconds(time);
        stateMachine_.TransitionTo(nextState);
    }
   
    /// <summary>
    /// 始点から終点を補間した座標へ移動(Rigidbody.MovePosition を使用)
    /// </summary>
    private void Move()
    {
        Vector3 newPosition = interpolator.Interpolation();
        rb.MovePosition(newPosition);
    }
}
