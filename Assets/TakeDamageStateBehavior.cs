using UnityEngine;

public class TakeDamageStateBehavior : StateMachineBehaviour
{
    //入力受付を開始するまでの時間
    [SerializeField] private float inputEnableTime = 0.5f;
    private float stateEnterTime;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateEnterTime = Time.time;
        animator.SetBool("CanInputReact", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(inputEnableTime <= 0f || Time.time - stateEnterTime >= inputEnableTime)
        {
            //入力受付を許可
            animator.SetBool("CanInputReact", true);
        }
        else
        {
            //まだ入力受付不可
            animator.SetBool("CanInputReact", false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("CanInputReact", true);
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
