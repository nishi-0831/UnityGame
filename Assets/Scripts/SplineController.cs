using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class SplineController : MonoBehaviour
{
    [SerializeField] SplineContainer splineContainer_;
    [SerializeField] GameObject followTarget_;
    [SerializeField] float t_;
    [SerializeField] float speed_;
    [SerializeField] float duration_;
    [SerializeField] float timer_;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splineContainer_ = GetComponent<SplineContainer>();
        //timer_ = 0.0f;
        //duration_ = 1.0f;
        //speed_ = 1.0f;
        //followTarget_ = GetComponent<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {
        timer_ += Time.deltaTime;




        //Sample();
        if (duration_ < timer_)
        {
            timer_ = 0.0f;
        }
        //t_ = timer_ / duration_;

        Sample3();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody rigidbody = followTarget_.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Vector3 dir = new Vector3(0, 1, 0);
                float force = 10.0f;
                rigidbody.AddForce(dir * force, ForceMode.Impulse);
            }

        }
    }

    void Sample(float t)
    {

        Vector3 pos = splineContainer_.EvaluatePosition(t);
        var tf = followTarget_.transform;
        tf.position = new Vector3(pos.x, tf.position.y, pos.z);
    }


    void Sample3()
    {
        int dir = 0;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            dir = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            dir = -1;
        }
        else
        {
            return;
        }
        float movementT = speed_ / splineContainer_.CalculateLength();

        t_ += (movementT * dir);
        Sample(t_);



        if (t_ < 0.0f)
        {
            t_ = 1.0f;
        }
        else if (t_ > 1.0f)
        {
            t_ = 0.0f;
        }

    }
}
