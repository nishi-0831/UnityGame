using UnityEngine;
using UnityEngine.InputSystem;


public class TestInput : MonoBehaviour
{
    private Horizontalscrolling inputActions; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new Horizontalscrolling();
        inputActions.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if(inputActions.Player.Fire.triggered)
        {
            Debug.Log("Fire Pressed");
        }
    }
}
