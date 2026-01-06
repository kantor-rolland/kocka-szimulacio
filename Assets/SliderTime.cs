using UnityEngine;
using UnityEngine.UI;

public class SliderTime : MonoBehaviour
{
    [SerializeField]
    public int timeScaleValue = 50;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        Time.timeScale = timeScaleValue;
    }
}
