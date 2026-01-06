using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IrregularDice : MonoBehaviour
{
    private float lastYPosition = 0;
    private bool isFalling = true;
    private float secondsSinceStopped = 0;

    //private int total = 0;
    //private int[] sidesCount = new int[6];

    // Each sphere represents a side of the dice
    [SerializeField]
    public SphereCollider sphere1, sphere2, sphere3, sphere4, sphere5, sphere6;

    private int sideLanded = -1;
    private Vector3 initialPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = transform.position;
        Time.timeScale = 1;
        // Random rotation
        transform.rotation = Random.rotation;
        lastYPosition = this.transform.position.y;

        sphere1.dice = this;
        sphere1.side = 1;
        sphere2.dice = this;
        sphere2.side = 2;
        sphere3.dice = this;
        sphere3.side = 3;
        sphere4.dice = this;
        sphere4.side = 4;
        sphere5.dice = this;
        sphere5.side = 5;
        sphere6.dice = this;
        sphere6.side = 6;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the dice is falling
        if (this.transform.position.y < lastYPosition)
        {
            lastYPosition = this.transform.position.y;
            isFalling = true;
            //secondsSinceStopped = 0;
        }
        else
        {
            isFalling = false;
        }

        if (!isFalling)
        {
            secondsSinceStopped += Time.deltaTime;
        }

        // If the dice has stopped for 2 seconds, reset the position and rotation
        if (secondsSinceStopped > 4)
        {
            transform.position = initialPosition;
            transform.rotation = Random.rotation;
            lastYPosition = this.transform.position.y;
            secondsSinceStopped = 0;

            int onTop = Mathf.Abs(sideLanded - 7);
            //print($"Side landed: {sideLanded}");
            //print($"Side on top: {onTop}");

            // Get object with tag "Summary"
            GameObject gameObject1 = GameObject.FindGameObjectWithTag("Summary");
            // Cast the object to a SummaryCreater
            SummaryCreater summaryCreater = gameObject1.GetComponent<SummaryCreater>();
            summaryCreater.addSideCount(onTop);
        }
    }

    public void setSideLanded(int side)
    {
        this.sideLanded = side;
    }
}
