using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent (typeof(SplineController))]
public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private SplineController splineController_;
    
    [SerializeField] ThirdPersonController thirdPersonController_;
    [SerializeField] CameraController cameraController_;
    [SerializeField] GameObject followTarget_;
    public float speed_;
    [SerializeField][Range(0f, 30f)] float verticalForce_;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (thirdPersonController_ == null)
        {
            thirdPersonController_ = GetComponentInChildren<ThirdPersonController>();
        }

        splineController_ = GetComponent<SplineController>();

        splineController_.splineDirection_ = 1;
        splineController_.onMaxT += splineController_.MoveOtherSplineMinOrMax;
        splineController_.onMinT += splineController_.MoveOtherSplineMinOrMax;

        UnityEngine.Vector3 pos = splineController_.currentSplineContainer_.EvaluatePosition(0.0f);
        followTarget_.transform.position = pos;
        thirdPersonController_.myEvent = splineController_.CheckUnderSpline;
    }

    // Update is called once per frame
    void Update()
    {
        int inputAxis = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            splineController_.isMovingLeft = true;   
            inputAxis = -1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            splineController_.isMovingLeft = false;
            inputAxis = 1;
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            thirdPersonController_.AddVerticalForce(verticalForce_);
        }
        // �A�j���[�V�����p�̓��͐ݒ�
        UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
        moveInput.x = inputAxis;
        thirdPersonController_.SetMoveInput(moveInput);
        
        // Spline���t�X�V
        splineController_.UpdateT(speed_, inputAxis);

        transform.rotation = splineController_.GetSplineRot();

        // ThirdPersonController�ɓn���ړ��ʂ��v�Z
        // Spline�̈ړ��� + Y���̏d��/�W�����v����
        Vector3 splineMovement = splineController_.GetSplineMovementDelta();
        Vector3 verticalMovement = new Vector3(0, thirdPersonController_.VerticalVelocity * Time.deltaTime, 0);
        
        Vector3 totalMovement = splineMovement + verticalMovement;
        
        thirdPersonController_.Move(totalMovement);

        if (cameraController_ != null)
        {
            cameraController_.isMovingLeft_ = splineController_.isMovingLeft;
        }
    }
}
