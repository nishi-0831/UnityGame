using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態を表すクラス
/// </summary>
[System.Serializable]
//テンプレートパラメータTStateEnumはSystem.Enum型でないといけない
public class State<TStateEnum> where TStateEnum : System.Enum
{
    [Header("状態名")]
    public TStateEnum stateType;
    
    // 状態に関連する処理
    private Action onEntry_;
    private Action onUpdate_;
    private Action onExit_;
    
    // 遷移条件のリスト
    private List<StateTransition<TStateEnum>> transitions_;

    public State(TStateEnum stateType)
    {
        this.stateType = stateType;
        transitions_ = new List<StateTransition<TStateEnum>>();
    }

    /// <summary>
    /// 状態設定メソッド
    /// </summary>
    public State<TStateEnum> SetCallbacks(Action onEntry = null, Action onUpdate = null, Action onExit = null)
    {
        onEntry_ = onEntry;
        onUpdate_ = onUpdate;
        onExit_ = onExit;
        return this;
    }

    /// <summary>
    /// 遷移条件を追加
    /// </summary>
    public State<TStateEnum> AddTransition(TStateEnum toState, Func<bool> condition)
    {
        transitions_.Add(new StateTransition<TStateEnum>(toState, condition));
        return this;
    }
    public State<TStateEnum> AddTransition(TStateEnum toState,ref Action triggerAction)
    {
        bool triggered = false;
        triggerAction += () => 
        {
            triggered = true;
        };
        //triggerAction.
        transitions_.Add(new StateTransition<TStateEnum>(toState, () => 
        {
            bool ret;
            if (triggered)
            {
                triggered = false;
                ret = true;
            }
            else
            {
                ret = false;
            }
                return ret;
        }));
        return this;
    }

    /// <summary>
    /// 状態に入った時の処理
    /// </summary>
    public void OnEntry()
    {
        onEntry_?.Invoke();
    }

    /// <summary>
    /// 状態の更新処理
    /// </summary>
    public void OnUpdate()
    {
        onUpdate_?.Invoke();
    }

    /// <summary>
    /// 状態を出る時の処理
    /// </summary>
    public void OnExit()
    {
        onExit_?.Invoke();
    }

    /// <summary>
    /// 遷移条件をチェックして、遷移先の状態を返す
    /// </summary>
    public bool TryGetNextState(out TStateEnum nextState)
    {
        foreach (var transition in transitions_)
        {
            if (transition.condition())
            {
                nextState = transition.toState;
                return true;
            }
        }
        nextState = default(TStateEnum);
        return false;
    }
}

/// <summary>
/// 状態遷移を表すクラス
/// </summary>
[System.Serializable]
public class StateTransition<TStateEnum> where TStateEnum : System.Enum
{
    public TStateEnum toState;
    public Func<bool> condition;

    public StateTransition(TStateEnum toState, Func<bool> condition)
    {
        this.toState = toState;
        this.condition = condition;
    }
}

/// <summary>
/// ステートマシン本体
/// </summary>
public class StateMachine<TStateEnum> where TStateEnum : System.Enum
{
    private Dictionary<TStateEnum, State<TStateEnum>> states_;
    private TStateEnum prevState_;
    private TStateEnum currentState_;
    private bool isInitialized_;

    public TStateEnum PrevState => prevState_;
    public TStateEnum CurrentState => currentState_;
    public bool IsInitialized => isInitialized_;

    public State<TStateEnum> this[TStateEnum key]
    {
        get
        {
            if(states_.TryGetValue(key,out State<TStateEnum> state))
            {
                return state;
            }
            throw new KeyNotFoundException($"State : {key} is not registered.");
        }
    }
    public StateMachine()
    {
        states_ = new Dictionary<TStateEnum, State<TStateEnum>>();
        isInitialized_ = false;
    }

    /// <summary>
    /// 状態を登録
    /// </summary>
    public State<TStateEnum> RegisterState(TStateEnum stateType)
    {
        var state = new State<TStateEnum>(stateType);
        states_[stateType] = state;
        return state;
    }

    /// <summary>
    /// 初期状態を設定してステートマシンを開始
    /// </summary>
    public void Start(TStateEnum initialState)
    {
        if (!states_.ContainsKey(initialState))
        {
            Debug.LogError($"State {initialState} is not registered!");
            return;
        }
        prevState_ = initialState;
        currentState_ = initialState;
        states_[currentState_].OnEntry();
        isInitialized_ = true;
    }

    /// <summary>
    /// ステートマシンの更新処理
    /// </summary>
    public void UpdateCurrent()
    {
        if (!isInitialized_)
            return;

        var currentStateObj = states_[currentState_];
        
        // 現在の状態の更新処理
        currentStateObj.OnUpdate();

        // 遷移条件をチェック
        if (currentStateObj.TryGetNextState(out TStateEnum nextState))
        {
            //遷移条件を満たしていた場合
            TransitionTo(nextState);
        }
    }

    /// <summary>
    /// 強制的に状態を変更
    /// </summary>
    public void TransitionTo(TStateEnum newState)
    {
        if (!states_.ContainsKey(newState))
        {
            Debug.LogError($"State {newState} is not registered!");
            return;
        }

        // 現在の状態の終了処理
        states_[currentState_].OnExit();

        //前回の状態を記憶
        prevState_ = currentState_;

        // 状態を変更
        //Debug.Log($"State transition: {currentState_} -> {newState}");
        currentState_ = newState;

        // 新しい状態の開始処理
        states_[currentState_].OnEntry();
    }

    /// <summary>
    /// 指定した状態が登録されているかチェック
    /// </summary>
    public bool HasState(TStateEnum stateType)
    {
        return states_.ContainsKey(stateType);
    }

    /// <summary>
    /// 現在の状態が指定した状態かチェック
    /// </summary>
    public bool IsCurrentState(TStateEnum stateType)
    {
        return isInitialized_ && currentState_.Equals(stateType);
    }
}
