using System;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDieStateBehaviour : StateMachineBehaviour
{
    //çÌèúÇ≥ÇÍÇÈÇ‹Ç≈ÇÃéûä‘
    [SerializeField] private float destroyDelay = 3f;
    private float stateEnterTime;
    [Tooltip("destroyDelayÇ™0Ç…Ç»Ç¡ÇΩÇ∆Ç´Ç…åƒÇŒÇÍÇÈ")]
    //private UnityEvent destroyAction;
    [SerializeField]
    private SplineMovementBase splineMovementBase;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateEnterTime = Time.time;
        splineMovementBase = animator.GetComponent<SplineMovementBase>();
        if(splineMovementBase == null )
        {
            Debug.LogWarning("splineMovementBase == null");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (destroyDelay <= 0f || Time.time - stateEnterTime >= destroyDelay)
        {
            splineMovementBase?.OnRequestDestroy();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //destroyAction.RemoveAllListeners();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
