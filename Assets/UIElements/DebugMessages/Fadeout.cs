using UnityEngine;
using UnityEngine.UI;

public class Fadeout : MonoBehaviour
{
    public float hangTime = 5.0f;
    public float fadeTime = 2.0f;
    float timeNow = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeNow += Time.deltaTime;
        if (timeNow > hangTime)
        {
            float alpha = 1.0f - ((timeNow - hangTime) / fadeTime);
            if (alpha > 0.0f)
            {
                Color color = GetComponent<Text>().color;
                color.a = alpha;
                GetComponent<Text>().color = color;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
