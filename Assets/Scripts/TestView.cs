using UnityEngine;

public class TestView : MonoBehaviour
{
    [SerializeField] TestScriptableObject testData;

    int a, b;
    string str;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        a = testData.num_a;
        b = testData.num_b;
        str = testData.str_a;

        Debug.Log(a+" "+str + " " + str);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
