using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ԃ�\���N���X
/// </summary>
[System.Serializable]
//�e���v���[�g�p�����[�^TStateEnum��System.Enum�^�łȂ��Ƃ����Ȃ�
public class State<TStateEnum> where TStateEnum : System.Enum
{
    [Header("��Ԗ�")]
    public TStateEnum stateType;
    
    // ��ԂɊ֘A���鏈��
    private Action onEntry_;
    private Action onUpdate_;
    private Action onExit_;
    
    // �J�ڏ����̃��X�g
    private List<StateTransition<TStateEnum>> transitions_;

    public State(TStateEnum stateType)
    {
        this.stateType = stateType;
        transitions_ = new List<StateTransition<TStateEnum>>();
    }

    /// <summary>
    /// ��Ԑݒ胁�\�b�h
    /// </summary>
    public State<TStateEnum> SetCallbacks(Action onEntry = null, Action onUpdate = null, Action onExit = null)
    {
        onEntry_ = onEntry;
        onUpdate_ = onUpdate;
        onExit_ = onExit;
        return this;
    }

    /// <summary>
    /// �J�ڏ�����ǉ�
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
    /// ��Ԃɓ��������̏���
    /// </summary>
    public void OnEntry()
    {
        onEntry_?.Invoke();
    }

    /// <summary>
    /// ��Ԃ̍X�V����
    /// </summary>
    public void OnUpdate()
    {
        onUpdate_?.Invoke();
    }

    /// <summary>
    /// ��Ԃ��o�鎞�̏���
    /// </summary>
    public void OnExit()
    {
        onExit_?.Invoke();
    }

    /// <summary>
    /// �J�ڏ������`�F�b�N���āA�J�ڐ�̏�Ԃ�Ԃ�
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
/// ��ԑJ�ڂ�\���N���X
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
/// �X�e�[�g�}�V���{��
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
    /// ��Ԃ�o�^
    /// </summary>
    public State<TStateEnum> RegisterState(TStateEnum stateType)
    {
        var state = new State<TStateEnum>(stateType);
        states_[stateType] = state;
        return state;
    }

    /// <summary>
    /// ������Ԃ�ݒ肵�ăX�e�[�g�}�V�����J�n
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
    /// �X�e�[�g�}�V���̍X�V����
    /// </summary>
    public void UpdateCurrent()
    {
        if (!isInitialized_)
            return;

        var currentStateObj = states_[currentState_];
        
        // ���݂̏�Ԃ̍X�V����
        currentStateObj.OnUpdate();

        // �J�ڏ������`�F�b�N
        if (currentStateObj.TryGetNextState(out TStateEnum nextState))
        {
            //�J�ڏ����𖞂����Ă����ꍇ
            TransitionTo(nextState);
        }
    }

    /// <summary>
    /// �����I�ɏ�Ԃ�ύX
    /// </summary>
    public void TransitionTo(TStateEnum newState)
    {
        if (!states_.ContainsKey(newState))
        {
            Debug.LogError($"State {newState} is not registered!");
            return;
        }

        // ���݂̏�Ԃ̏I������
        states_[currentState_].OnExit();

        //�O��̏�Ԃ��L��
        prevState_ = currentState_;

        // ��Ԃ�ύX
        //Debug.Log($"State transition: {currentState_} -> {newState}");
        currentState_ = newState;

        // �V������Ԃ̊J�n����
        states_[currentState_].OnEntry();
    }

    /// <summary>
    /// �w�肵����Ԃ��o�^����Ă��邩�`�F�b�N
    /// </summary>
    public bool HasState(TStateEnum stateType)
    {
        return states_.ContainsKey(stateType);
    }

    /// <summary>
    /// ���݂̏�Ԃ��w�肵����Ԃ��`�F�b�N
    /// </summary>
    public bool IsCurrentState(TStateEnum stateType)
    {
        return isInitialized_ && currentState_.Equals(stateType);
    }
}
