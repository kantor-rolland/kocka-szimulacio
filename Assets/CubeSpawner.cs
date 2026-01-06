using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject cubePrefab;

    [SerializeField]
    public int numberOfCubes = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Spawn cubes
        for (int i = 0; i < numberOfCubes; i++)
        {
            GameObject cube = Instantiate(cubePrefab, new Vector3(0, 10, (i + 1) * 5), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
