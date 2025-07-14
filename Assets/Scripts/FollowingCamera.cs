// These codes are licensed under CC0.
// http://creativecommons.org/publicdomain/zero/1.0/deed.ja
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

using Debug = UnityEngine.Debug;
/// <summary>
/// The freeCameraObj added this script will follow the specified object.
/// The freeCameraObj can be moved by left mouse drag and mouse wheel.
/// </summary>
[ExecuteInEditMode, DisallowMultipleComponent]
//https://qiita.com/sakano/items/918c090f484c0610619d
//この記事を参考にしました。
public class FollowingCamera : MonoBehaviour
{
    public GameObject target; // an object to follow
    //offsetとDistanceの数値によってはtargetを中心に据えなくなるので要注意
    public Vector3 offset; // offset form the target object

   
    [SerializeField] private float distance = 4.0f; // distance from following object
    [SerializeField] private float polarAngle = 90.0f; // angle with y-vec 極角
    [SerializeField] private float azimuthalAngle = 90.0f; // angle with x-vec 方位角

    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 9.0f;
    [SerializeField] private float minPolarAngle = 5.0f;
    [SerializeField] private float maxPolarAngle = 75.0f;
    [SerializeField] public float mouseXSensitivity = 5.0f;
    [SerializeField] public float mouseYSensitivity = 5.0f;
    [SerializeField] public float stickXSensitivity = 0.5f;
    [SerializeField] public float stickYSensitivity = 0.5f;
    [SerializeField] public float sensitivity = 5.0f;
    [SerializeField] public float scrollSensitivity = 5.0f;

    
    //[SerializeField] private float movedFoV = 65.0f;
    //[SerializeField] private float FoV = 50.0f;
    //[SerializeField] private float waitTime = 2;
    [SerializeField] private float FovIncreaseDuration;
    [SerializeField] private float FovDecreaseDuration;
    //private float changeTime = 2.5f;
    [SerializeField] private bool isCalled = false;
    //[SerializeField] private bool isIncreasingFoV = true;
    //[SerializeField] float elapsedTime = 0f;
    [SerializeField] float nextFoV;
    [SerializeField] float currFoV;
    
    //private Camera camera;
    [SerializeField] Camera subCamera;
    public static FollowingCamera instance;
    void Start()
    {
        Init();
        //director = directorObj.GetComponent<stateDirector>();
    }
    //カメラは必ずLateUpdate()で！！！
 
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            
        }
        else
        {
            Destroy(this);
        }
        //camera = GetComponent<Camera>();
    }
    public void Init()
    {
        //target = GameObject.FindWithTag("Player");
        Debug.Log(target.name);
    }
    void Update()
    {
        if (isCalled == true)
        {
            
        }

    }
    void LateUpdate()
    {

        FollowTarget(0, 0);
    }
    public void FollowTarget(float x,float y)
    {
        
        UpdateAngle(x, y);
        
        var lookAtPos = target.transform.position + offset;
        
        UpdatePosition(lookAtPos);
        transform.LookAt(lookAtPos);
    }
   

    void UpdateAngle(float x,float y)
    {
       
       
        x = azimuthalAngle - x;
        azimuthalAngle = Mathf.Repeat(x, 360);
         
        y = polarAngle + y;
        polarAngle = Mathf.Clamp(y, minPolarAngle, maxPolarAngle);
        //polarAngle = Mathf.Repeat(y, 360);
       
    }

    void UpdateDistance(float scroll)
    {
        scroll = distance - scroll * scrollSensitivity;
        distance = Mathf.Clamp(scroll, minDistance, maxDistance);
    }

    void UpdatePosition(Vector3 lookAtPos)
    {
        //Deg2Rad →Degree(度数法)をRadian(弧度法)に変換
        var da = azimuthalAngle * Mathf.Deg2Rad;
        var dp = polarAngle * Mathf.Deg2Rad;
        this.transform.position = new Vector3(
            lookAtPos.x + distance * Mathf.Sin(dp) * Mathf.Cos(da),
            lookAtPos.y + distance * Mathf.Cos(dp),
            lookAtPos.z + distance * Mathf.Sin(dp) * Mathf.Sin(da));
    }
    
  
    public void ChangeAngle(Vector3 vec)
    {
        azimuthalAngle = Mathf.Atan2(vec.z, vec.x) * Mathf.Rad2Deg;
        polarAngle= Mathf.Acos(vec.y / vec.magnitude) * Mathf.Rad2Deg;
    }
}