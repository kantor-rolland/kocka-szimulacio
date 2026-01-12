//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class IrregularDice : MonoBehaviour
//{
//    [Header("Detection Settings")]
//    private float lastYPosition = 0;
//    private bool isFalling = true;
//    private float secondsSinceStopped = 0;
//    private int sideLanded = -1;
//    private Vector3 initialPosition;

//    // Each sphere represents a side of the dice
//    [SerializeField]
//    public SphereCollider sphere1, sphere2, sphere3, sphere4, sphere5, sphere6;

//    // kibovites
//    [Header("Physics Weighting")]
//    public float materialDensity = 1f; // kg/m3
//    public Vector3 voidCenter = new Vector3(0.0f, 0.45f, 0.0f); // Az üreg helye
//    public float voidRadius = 0.5f; // Az üreg mérete
//    public int samplePoints = 5000; // Pontosság (nagyobb=lassabb, pontosabb)

//    private Rigidbody rb;
//    private MeshCollider meshCollider;
//    private Bounds diceBounds; // ???

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        meshCollider = GetComponent<MeshCollider>();

//        if (meshCollider != null)
//        {
//            CalculateMassDistribution();
//        }
//        else
//        {
//            Debug.LogWarning("MeshCollider not found on dice! Physics mass-distribution will not be calculated.");
//        }


//        initialPosition = transform.position;
//        Time.timeScale = 1;
//        // Random rotation
//        transform.rotation = Random.rotation;
//        lastYPosition = this.transform.position.y;

//        SetupSphereColliders();
//        /*
//        sphere1.dice = this;
//        sphere1.side = 1;
//        sphere2.dice = this;
//        sphere2.side = 2;
//        sphere3.dice = this;
//        sphere3.side = 3;
//        sphere4.dice = this;
//        sphere4.side = 4;
//        sphere5.dice = this;
//        sphere5.side = 5;
//        sphere6.dice = this;
//        sphere6.side = 6;
//        */

//    }

//    private void SetupSphereColliders()
//    {
//        /*
//        if (sphere1) { sphere1.dice = this; sphere1.side = 1; }
//        if (sphere2) { sphere2.dice = this; sphere2.side = 2; }
//        if (sphere3) { sphere3.dice = this; sphere3.side = 3; }
//        if (sphere4) { sphere4.dice = this; sphere4.side = 4; }
//        if (sphere5) { sphere5.dice = this; sphere5.side = 5; }
//        if (sphere6) { sphere6.dice = this; sphere6.side = 6; }
//        */
//        sphere1.dice = this;
//        sphere1.side = 1;
//        sphere2.dice = this;
//        sphere2.side = 2;
//        sphere3.dice = this;
//        sphere3.side = 3;
//        sphere4.dice = this;
//        sphere4.side = 4;
//        sphere5.dice = this;
//        sphere5.side = 5;
//        sphere6.dice = this;
//        sphere6.side = 6;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // Check if the dice is falling
//        if (this.transform.position.y < lastYPosition)
//        {
//            lastYPosition = this.transform.position.y;
//            isFalling = true;
//            //secondsSinceStopped = 0;
//        }
//        else
//        {
//            isFalling = false;
//        }

//        if (!isFalling)
//        {
//            secondsSinceStopped += Time.deltaTime;
//        }

//        // If the dice has stopped for 2 seconds, reset the position and rotation
//        if (secondsSinceStopped > 4 && sideLanded != -1)
//        {
//            transform.position = initialPosition;
//            transform.rotation = Random.rotation;
//            lastYPosition = this.transform.position.y;
//            secondsSinceStopped = 0;

//            int onTop = Mathf.Abs(sideLanded - 7);
//            //print($"Side landed: {sideLanded}");
//            //print($"Side on top: {onTop}");

//            // Get object with tag "Summary"
//            GameObject gameObject1 = GameObject.FindGameObjectWithTag("Summary");
//            // Cast the object to a SummaryCreater
//            SummaryCreater summaryCreater = gameObject1.GetComponent<SummaryCreater>();
//            summaryCreater.addSideCount(onTop);
//        }
//    }

//    public void setSideLanded(int side)
//    {
//        this.sideLanded = side;
//    }

//    void CalculateMassDistribution()
//    {
//        Debug.Log("Tömeg- és súlypontszámítás indul...");
//        Bounds diceBounds = meshCollider.sharedMesh.bounds;

//        Vector3 centerOfMassSum = Vector3.zero;
//        int validPointCount = 0;
//        List<Vector3> validPoints = new List<Vector3>();
//        int volumeSamples = 25000;
//        int insideVolume = 0;
//        float boundsVolume = diceBounds.size.x * diceBounds.size.y * diceBounds.size.z;

//        for (int i = 0; i < Mathf.Max(samplePoints, volumeSamples); i++)
//        {
//            Vector3 point = new Vector3(
//                Random.Range(diceBounds.min.x, diceBounds.max.x),
//                Random.Range(diceBounds.min.y, diceBounds.max.y),
//                Random.Range(diceBounds.min.z, diceBounds.max.z)
//            );
//            bool valid = IsInsideMesh(point) && !IsInsideVoid(point);

//            // a) a tömeg/tehetetlenség-eloszlás csak a 'samplePoints' első valid pontján
//            if (valid && validPointCount < samplePoints)
//            {
//                validPoints.Add(point);
//                centerOfMassSum += point;
//                validPointCount++;
//            }
//            // b) ugyanebből minta alapján kb. volumet számolunk
//            if (i < volumeSamples && valid) insideVolume++;
//        }

//        // valódi üreges kockatérfogat (m³)
//        float actualVolume = (float)insideVolume / volumeSamples * boundsVolume;
//        float actualMass = actualVolume * materialDensity;
//        rb.mass = actualMass; // kritikus! (ne static mass legyen)

//        if (validPointCount > 0)
//        {
//            // Súlypont
//            rb.centerOfMass = centerOfMassSum / validPointCount;
//            // Tehetetlenségi tensor (diagonális)
//            rb.inertiaTensor = CalculateInertiaTensor(validPoints.ToArray(), validPointCount, rb.centerOfMass, rb.mass);
//            Debug.Log($"[IrregularDice] Mass={actualMass:F4}, COM={rb.centerOfMass}");
//        }
//        else Debug.LogError("Egyetlen érvényes belső pont sincs - hibás bounds vagy mesh?");
//    }

//    bool IsInsideMesh(Vector3 point)
//    {
//        Vector3 worldPoint = transform.TransformPoint(point);
//        Ray ray = new Ray(worldPoint, Vector3.up);

//        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
//        int intersections = 0;
//        foreach (var hit in hits)
//            if (hit.collider == meshCollider) intersections++;
//        return (intersections % 2) == 1;
//    }

//    bool IsInsideVoid(Vector3 point) => Vector3.Distance(point, voidCenter) <= voidRadius;

//    Vector3 CalculateInertiaTensor(Vector3[] points, int count, Vector3 com, float totalMass)
//    {
//        float massPerPoint = totalMass / count;
//        float Ixx = 0f, Iyy = 0f, Izz = 0f;
//        for (int i = 0; i < count; i++)
//        {
//            Vector3 r = points[i] - com;
//            Ixx += massPerPoint * (r.y * r.y + r.z * r.z);
//            Iyy += massPerPoint * (r.x * r.x + r.z * r.z);
//            Izz += massPerPoint * (r.x * r.x + r.y * r.y);
//        }
//        return new Vector3(Ixx, Iyy, Izz);
//    }

//    void OnDrawGizmosSelected()
//    {
//        // Piros gömb az üregre
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.TransformPoint(voidCenter), voidRadius);

//        // Zöld gömb a tényleges súlypontra
//        if (rb != null)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.05f);
//        }
//    }
//}

///*
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class IrregularDice : MonoBehaviour
//{
//    private float lastYPosition = 0;
//    private bool isFalling = true;
//    private float secondsSinceStopped = 0;

//    //private int total = 0;
//    //private int[] sidesCount = new int[6];

//    // Each sphere represents a side of the dice
//    [SerializeField]
//    public SphereCollider sphere1, sphere2, sphere3, sphere4, sphere5, sphere6;

//    private int sideLanded = -1;
//    private Vector3 initialPosition;

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        initialPosition = transform.position;
//        Time.timeScale = 1;
//        // Random rotation
//        transform.rotation = Random.rotation;
//        lastYPosition = this.transform.position.y;

//        sphere1.dice = this;
//        sphere1.side = 1;
//        sphere2.dice = this;
//        sphere2.side = 2;
//        sphere3.dice = this;
//        sphere3.side = 3;
//        sphere4.dice = this;
//        sphere4.side = 4;
//        sphere5.dice = this;
//        sphere5.side = 5;
//        sphere6.dice = this;
//        sphere6.side = 6;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // Check if the dice is falling
//        if (this.transform.position.y < lastYPosition)
//        {
//            lastYPosition = this.transform.position.y;
//            isFalling = true;
//            //secondsSinceStopped = 0;
//        }
//        else
//        {
//            isFalling = false;
//        }

//        if (!isFalling)
//        {
//            secondsSinceStopped += Time.deltaTime;
//        }

//        // If the dice has stopped for 2 seconds, reset the position and rotation
//        if (secondsSinceStopped > 4)
//        {
//            transform.position = initialPosition;
//            transform.rotation = Random.rotation;
//            lastYPosition = this.transform.position.y;
//            secondsSinceStopped = 0;

//            int onTop = Mathf.Abs(sideLanded - 7);
//            //print($"Side landed: {sideLanded}");
//            //print($"Side on top: {onTop}");

//            // Get object with tag "Summary"
//            GameObject gameObject1 = GameObject.FindGameObjectWithTag("Summary");
//            // Cast the object to a SummaryCreater
//            SummaryCreater summaryCreater = gameObject1.GetComponent<SummaryCreater>();
//            summaryCreater.addSideCount(onTop);
//        }
//    }

//    public void setSideLanded(int side)
//    {
//        this.sideLanded = side;
//    }
//}
//*/