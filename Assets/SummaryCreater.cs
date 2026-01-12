using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SummaryCreater : MonoBehaviour
{
    public int total = 0;
    public int[] sideCounts = new int[6];

    [SerializeField]
    public TextMeshProUGUI totalText, summaryText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addSideCount(int side)
    {
        total++;
        if (side < 0 || side > sideCounts.Length)
        {
            Debug.Log($"Invalid side received: {side}");
            Debug.LogError($"Invalid side received: {side}");
            throw new System.Exception("anyad");
        }
        sideCounts[side - 1]++;
        totalText.text = $"Total: {total}";
        summaryText.text = $"1: {((float)sideCounts[0] / (float)total * 100f).ToString("F1")}%\n" +
            $"2: {((float)sideCounts[1] / (float)total * 100f).ToString("F1")}%\n" +
            $"3: {((float)sideCounts[2] / (float)total * 100f).ToString("F1")}%\n" +
            $"4: {((float)sideCounts[3] / (float)total * 100f).ToString("F1")}%\n" +
            $"5: {((float)sideCounts[4] / (float)total * 100f).ToString("F1")}%\n" +
            $"6: {((float)sideCounts[5] / (float)total * 100f).ToString("F1")}%\n";
    }
}
