using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [SerializeField] float speed_;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Vector3 forceDir = new Vector3(0, 1, 0);
                float force = 10.0f;
                rigidbody.AddForce(forceDir * force, ForceMode.Impulse);
            }
        }
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
        float movement = speed_  * dir * Time.deltaTime;

        transform.position += new Vector3(movement, 0, 0);


    }
}
