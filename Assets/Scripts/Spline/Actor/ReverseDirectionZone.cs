using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
[RequireComponent (typeof(Rigidbody))]
public class ReverseDirectionZone : SplineMovementBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        SplineController splineController;
        bool success = other.TryGetComponent<SplineController>(out splineController);
        if (success)
        {
            splineController.Reverse();
        }
    }
}
