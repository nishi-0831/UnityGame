using System.Collections;
using UnityEngine;

public enum MoveState
{
    WAIT,
    GOING,
    COMBACKING
}

[RequireComponent(typeof(EaseInterpolator))]
[RequireComponent(typeof(Rigidbody))]
public class LerpPingPong : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] public Vector3 _from;
    [SerializeField] public Vector3 _to;

    [Header("Time (seconds)")]
    [SerializeField] private float goingTime = 2.0f;        // 押す時間
    [SerializeField] private float comeBackTime = 0.6f;     // 戻る時間
    [SerializeField] private float firstTime = 2.0f;        // 最初だけ待つ
    public float FirstTime => firstTime;

    [SerializeField] private float againGoingTime = 1.5f;   // 次に押すまで
    [SerializeField] private float againComebackTime = 0.0f;// 押し切ってから戻るまで

    private MoveState currentState_;
    public MoveState CurrentState => currentState_;

    private StateMachine<MoveState> stateMachine_;
    private EaseInterpolator interpolator;
    private Rigidbody rb;
    private Coroutine waitCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interpolator = GetComponent<EaseInterpolator>();
        stateMachine_ = new StateMachine<MoveState>();
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        // WAIT
        stateMachine_.RegisterState(MoveState.WAIT).SetCallbacks(
            onEntry: () =>
            {
                StopWaitCoroutine();

                float waitTime = firstTime;
                MoveState next = MoveState.GOING;

                switch (stateMachine_.PrevState)
                {
                    case MoveState.GOING:
                        waitTime = againComebackTime;
                        next = MoveState.COMBACKING;
                        break;

                    case MoveState.COMBACKING:
                        waitTime = againGoingTime;
                        next = MoveState.GOING;
                        break;
                }

                waitCoroutine = StartCoroutine(WaitAndTransition(waitTime, next));
            }
        );

        // GOING（プレス）
        stateMachine_.RegisterState(MoveState.GOING).SetCallbacks(
            onEntry: () =>
            {
                interpolator.duration = goingTime;
                interpolator.isReverse_ = false;
                interpolator.from_ = _from;
                interpolator.to_ = _to;
                interpolator.Reset();
            },
            onUpdate: () =>
            {
                interpolator.UpdateTime();
                Move();
            }
        ).AddTransition(MoveState.WAIT, ref interpolator.onFinished_);

        // COMBACKING（戻り）
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
            }
        ).AddTransition(MoveState.WAIT, ref interpolator.onComeback_);
    }

    public void StartPingPong()
    {
        StopWaitCoroutine();
        stateMachine_.Start(MoveState.WAIT);
    }

    private void Update()
    {
        stateMachine_.UpdateCurrent();
        currentState_ = stateMachine_.CurrentState;
    }

    private void Move()
    {
        rb.MovePosition(interpolator.Interpolation());
    }

    private IEnumerator WaitAndTransition(float time, MoveState next)
    {
        if (time > 0f)
            yield return new WaitForSeconds(time);

        stateMachine_.TransitionTo(next);
    }

    private void StopWaitCoroutine()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
    }

    // 外部（PressMachine）から調整用
    public void SetMoveTime(float going, float comeback)
    {
        goingTime = going;
        comeBackTime = comeback;
    }
}
