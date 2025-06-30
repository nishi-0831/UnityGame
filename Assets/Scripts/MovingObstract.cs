using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingObstract : MonoBehaviour
{
    
    [SerializeField] private float power=1f;
    [SerializeField] private Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
    }
    void Update()
    {
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"{hit.gameObject.name}Ç∆{this.name}Ç™è’ìÀÇµÇ‹ÇµÇΩ");
    }
    
}
