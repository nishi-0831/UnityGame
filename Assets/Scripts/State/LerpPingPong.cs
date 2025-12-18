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
    [SerializeField] private float goingTime = 2.0f;
    [SerializeField] private float comeBackTime = 0.6f;
    [SerializeField] private float firstTime = 2.0f;
    public float FirstTime => firstTime;

    [SerializeField] private float againGoingTime = 1.5f;
    [SerializeField] private float againComebackTime = 0.0f;

    [Header("警告演出（WAIT 残り70%）")]
    [SerializeField] private Color blinkColor = Color.yellow;
    [SerializeField] private float blinkSpeed = 2.0f;
    [SerializeField] private float emissionIntensity = 2.0f;

    private MoveState currentState_;
    public MoveState CurrentState => currentState_;

    private StateMachine<MoveState> stateMachine_;
    private EaseInterpolator interpolator;
    private Rigidbody rb;

    private Coroutine waitCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine blinkDelayCoroutine;

    private Renderer[] renderers;
    private Color[] baseEmissionColors;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interpolator = GetComponent<EaseInterpolator>();

        // FBX子オブジェクト含め Renderer 取得
        renderers = GetComponentsInChildren<Renderer>();
        baseEmissionColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;
            mat.EnableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor"))
                baseEmissionColors[i] = mat.GetColor("_EmissionColor");
        }

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
                StopBlink();
                StopBlinkDelay();

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

                // WAIT残り70%で点滅開始
                if (waitTime > 0f)
                    blinkDelayCoroutine = StartCoroutine(StartBlinkLate(waitTime));

                waitCoroutine = StartCoroutine(WaitAndTransition(waitTime, next));
            },
            onExit: () =>
            {
                StopBlink();
                StopBlinkDelay();
            }
        );

        // GOING
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

        // COMBACKING
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

    // 点滅制御

    private IEnumerator StartBlinkLate(float waitTime)
    {
        yield return new WaitForSeconds(waitTime * 0.3f); // 残り時間70%到達で点滅
        StartBlink();
    }

    private void StopBlinkDelay()
    {
        if (blinkDelayCoroutine != null)
        {
            StopCoroutine(blinkDelayCoroutine);
            blinkDelayCoroutine = null;
        }
    }

    private void StartBlink()
    {
        StopBlink();
        blinkCoroutine = StartCoroutine(BlinkEmission());
    }

    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // 元のEmissionに戻す
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_EmissionColor"))
                renderers[i].material.SetColor("_EmissionColor", baseEmissionColors[i]);
        }
    }

    private IEnumerator BlinkEmission()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            Color emit = blinkColor * Mathf.Lerp(0f, emissionIntensity, t);

            foreach (var r in renderers)
            {
                if (r.material.HasProperty("_EmissionColor"))
                    r.material.SetColor("_EmissionColor", emit);
            }

            yield return null;
        }
    }

    // 外部調整用
    public void SetMoveTime(float going, float comeback)
    {
        goingTime = going;
        comeBackTime = comeback;
    }
}
