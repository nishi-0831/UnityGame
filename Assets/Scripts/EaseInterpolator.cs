using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EaseInterpolator : MonoBehaviour
{
    public float t;
    //public float t { get; private set; }

    public float elapsedTime {  get; private set; }
    public float time;
    public float duration = 0;
    public EasingFunc func;
    bool isFinish = false;

    [HideInInspector]public Vector3 from_;
    [HideInInspector] public Vector3 to_;

    public  Action onFinished_;
    public Action onComeback_;

    public bool isReverse_ = false;
    // Update is called once per frame
    
    public void UpdateTime()
    {
        elapsedTime += UnityEngine.Time.deltaTime;

        if(isReverse_)
        {
            t -= UnityEngine.Time.deltaTime / duration;
        }
        else
        {
            t += UnityEngine.Time.deltaTime / duration;
        }
            
        //t = Mathf.Clamp01(elapsedTime / duration);
        if(t<0)
        {
            Debug.Log("Comeback");
            onComeback_?.Invoke();
        }
        else if(t > 1.0f)
        {
            Debug.Log("Finish");
            onFinished_?.Invoke();
        }
    }
    public void CheckTime()
    {
        if(t >= 1)
        {
            isFinish = true;
        }
    }
    public float GetEase()
    {
        return func(t);
    }
    public bool IsFinish()
    { 
        return isFinish;
    }
    public void Reset()
    {
        elapsedTime = 0;
        if(isReverse_)
        {
            t = 1.0f;
        }
        else
        {
            t = 0.0f;
        }
            
        isFinish = false;
        //Debug.Log("Reset");
    }
    public Vector3 Interpolation()
    {
        return Vector3.Lerp(from_, to_, t);
    }
    public IEnumerator Interpolation(Vector3 a, Vector3 b, float duration_, Action<Vector3> vec3)
    {
        duration = duration_;
        //while (IsFinish() == false)
        while(t<1)
        {
            UpdateTime();
            float t = GetEase();
            Vector3 value = Vector3.Lerp(a, b, t);
            vec3.Invoke(value);
            yield return null;
        }
        CheckTime();
        //vec3.Invoke(b);
        
        //Reset();
    }
    public IEnumerator Interpolation(float a,float b,float duration_,Action<float> f)
    {
        duration = duration_;
        while(t<1)
        {
            UpdateTime();
            float t = GetEase();
            float value = Mathf.Lerp(a, b, t);
            f.Invoke(value);
            yield return null;
        }
        if(t >= 1)
        {
            isFinish = true;
        }
    }
    public delegate float EasingFunc(float x);

    public float OutExpo(float x)
    {
        if (x == 1)
        {
            x = 1;
        }
        else
        {
            x = 1 - (Mathf.Pow(2, -10 * x));
        }
        return x;
    }
    public float OutCirc(float x)
    {
        float value = Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
        return value;
    }
    public float OutElastic(float x)
    {
        const float c4 = (2 * Mathf.PI) / 3;

        if(x ==0)
        {
            return 0;
        }
        else if(x ==1)
        {
            return 1;
        }
        else
        {
            return Mathf.Pow(2,-10 * x) * Mathf.Sin((x * 10-0.75f)*c4)+1;
        }
    }
    public float OutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }
    public float OutSine(float x)
    {
        return Mathf.Sin((x * Mathf.PI) / 2);
    }
    public float OutBounce(float x)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if(x<1/d1)
        {
            return n1 * x * x;
        }
        else if(x < 2/d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if(x < 2.5 /d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
    public float InQuint(float x)
    {
        return x * x * x * x * x;
    }
    public float InSine(float x)
    {
        return 1 - Mathf.Cos((x*Mathf.PI)/2);
    }
    public float InOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }
    public float InCirc(float x)
    {
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
    }
    public float InExpo(float x)
    {
        if(x ==0)
        {
            return 0;
        }
        else
        {
            return Mathf.Pow(2, 10 * x - 10);
        }
    }
    public float InCubic(float x)
    {
        return x * x * x;
    }
}
