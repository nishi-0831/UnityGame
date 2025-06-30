using UnityEngine;
using UnityEngine.UIElements;

public class ObjectController : MonoBehaviour
{
    [SerializeField]private float speed_;
    [SerializeField]private Rigidbody rigidbody_;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody_ = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (rigidbody_ != null)
            {
                Vector3 forceDir = new Vector3(0, 1, 0);
                float force = 10.0f;
                rigidbody_.AddForce(forceDir * force, ForceMode.Impulse);
            }
        }
        //    int dir = 0;
        //if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    dir = 1;
        //}
        //else if (Input.GetKey(KeyCode.LeftArrow))
        //{
        //    dir = -1;
        //}
        //else
        //{
        //    return;
        //}
        //float movement = speed_  * dir * UpdateTime.deltaTime;

        //transform.position += new Vector3(movement, 0, 0);


    }
    private void FixedUpdate()
    {
        var hori = Input.GetAxis("Horizontal");
        //var vert = Input.GetAxis("Vertical");

        rigidbody_.linearVelocity = new Vector3(hori * speed_, 0, 0);
    }


}
