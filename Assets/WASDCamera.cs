using UnityEngine;

public class WASDCamera : MonoBehaviour
{
    [SerializeField]
    Camera m_Camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal");
        float zAxisValue = Input.GetAxis("Vertical");
        m_Camera.transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue));
    }
}
