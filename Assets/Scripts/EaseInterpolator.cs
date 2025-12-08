using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 時間ベースのイージング補間ユーティリティとコルーチンを提供する
/// 設定されたイージング関数を使用して補間を行うため、GameObject にアタッチして使用
/// </summary>
public class EaseInterpolator : MonoBehaviour
{
    /// <summary>
    /// 正規化された進捗パラメータ(範囲 [0,1])
    /// </summary>
    public float t;

    /// <summary>
    /// Reset からの経過時間(秒)
    /// </summary>
    public float elapsedTime {  get; private set; }


    /// <summary>
    /// 補間の継続時間(秒)
    /// </summary>
    public float duration = 0.1f;

    /// <summary>
    /// 正規化パラメータ <see cref="t"/> に適用するイージング関数
    /// </summary>
    public EasingFunc func;

    bool isFinish = false;

    [HideInInspector]
    /// <summary>
    /// Vector3 補間の開始値
    /// </summary>
    public Vector3 from_;

    [HideInInspector]
    /// <summary>
    /// Vector3 補間の終了値
    /// </summary>
    public Vector3 to_;

    /// <summary>
    /// 補間が終了したとき（t &gt; 1）に呼ばれるコールバック
    /// </summary>
    public  Action onFinished_;

    /// <summary>
    /// 補間が 0 未満になったとき（t &lt; 0）に呼ばれるコールバック
    /// </summary>
    public Action onComeback_;

    /// <summary>
    /// true の場合、補間は逆方向(t が減少)で進行
    /// </summary>
    public bool isReverse_ = false;
    // Update is called once per frame

    /// <summary>
    /// 内部タイマーと正規化パラメータ <see cref="t"/> を進めます
    /// <c>t &lt; 0</c> のとき <see cref="onComeback_"/> を、<c>t &gt; 1</c> のとき <see cref="onFinished_"/> を呼び出します
    /// </summary>
    public void UpdateTime()
    {
        elapsedTime += UnityEngine.Time.deltaTime;

        if (isReverse_)
        {
            t -= UnityEngine.Time.deltaTime / duration;
        }
        else
        {
            t += UnityEngine.Time.deltaTime / duration;
        }

        if (t < 0)
        {
            onComeback_?.Invoke();
        }
        else if (t > 1.0f)
        {
            onFinished_?.Invoke();
        }
    }

    /// <summary>
    /// <see cref="t"/> が 1 に達したら補間を完了済みに設定
    /// </summary>
    public void CheckTime()
    {
        if(t >= 1)
        {
            isFinish = true;
        }
    }

    /// <summary>
    /// 現在の正規化パラメータに対して <see cref="func"/> を適用した値を返す
    /// </summary>
    /// <returns>イージング後の値（範囲 [0,1]）。</returns>
    public float GetEase()
    {
        return func(t);
    }

    /// <summary>
    /// 補間が完了しているかを返します。
    /// </summary>
    public bool IsFinish()
    { 
        return isFinish;
    }

    /// <summary>
    /// 内部タイマーと <see cref="t"/> をリセットする。<see cref="isReverse_"/> によって開始位置を決定する
    /// </summary>
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
    }

    /// <summary>
    /// <see cref="from_"/> と <see cref="to_"/> の間で、パラメータ <see cref="t"/> を使った線形補間を行う
    /// </summary>
    /// <returns>補間された Vector3。</returns>
    public Vector3 Interpolation()
    {
        return Vector3.Lerp(from_, to_, t);
    }

    /// <summary>
    /// 指定した 2 つの Vector3 間を、設定されたイージング関数を用いて補間するコルーチン
    /// </summary>
    /// <param name="a">開始値</param>
    /// <param name="b">終了値</param>
    /// <param name="duration_">継続時間(秒)</param>
    /// <param name="vec3">各フレームで現在の補間値を受け取るコールバック</param>
    public IEnumerator Interpolation(Vector3 a, Vector3 b, float duration_, Action<Vector3> vec3)
    {
        duration = duration_;
        while(t<1)
        {
            UpdateTime();
            float t = GetEase();
            Vector3 value = Vector3.Lerp(a, b, t);
            vec3.Invoke(value);
            yield return null;
        }
        CheckTime();
    }

    /// <summary>
    /// 指定した 2 つの float 値間を、設定されたイージング関数を用いて補間するコルーチン
    /// </summary>
    /// <param name="a">開始値</param>
    /// <param name="b">終了値</param>
    /// <param name="duration_">継続時間(秒)</param>
    /// <param name="f">各フレームで現在の補間値を受け取るコールバック</param>
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

    /// <summary>
    /// イージング関数のデリゲート。入力 x は [0,1]、戻り値も [0,1] を期待する
    /// </summary>
    public delegate float EasingFunc(float x);

    static public float OutExpo(float x)
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

    static public float OutCirc(float x)
    {
        float value = Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
        return value;
    }

    static public float OutElastic(float x)
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

    static public float OutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    static public float OutSine(float x)
    {
        return Mathf.Sin((x * Mathf.PI) / 2);
    }

    static public float OutBounce(float x)
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

    static public float InQuint(float x)
    {
        return x * x * x * x * x;
    }

    static public float InSine(float x)
    {
        return 1 - Mathf.Cos((x*Mathf.PI)/2);
    }

    static public float InOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }

    static public float InCirc(float x)
    {
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
    }

    static public float InExpo(float x)
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

    static public float InCubic(float x)
    {
        return x * x * x;
    }
}
