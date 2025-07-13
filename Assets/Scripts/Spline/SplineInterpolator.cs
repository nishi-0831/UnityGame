using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent (typeof(SplineAnimate))]
public class SplineInterpolator : MonoBehaviour
{
    SplineAnimate splineAnimate_;
    [SerializeField] float t_;
    [SerializeField] GameObject child_;
    [SerializeField] float waitTime_ = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splineAnimate_ = GetComponent<SplineAnimate>();
    }

    // Update is called once per frame
    void Update()
    {

        splineAnimate_.Completed += WaitInterpolator;
    }

    void WaitInterpolator()
    {
        StartCoroutine(WaitInterpolatorCoroutine());
    }
    IEnumerator WaitInterpolatorCoroutine()
    {
        splineAnimate_.Pause();
        yield return new WaitForSeconds(waitTime_);
        splineAnimate_.Restart(true);
    }
}
