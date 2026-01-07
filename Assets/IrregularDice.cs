using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IrregularDice : MonoBehaviour
{
    [Header("Dice State")]
    private float lastYPosition = 0;
    private bool isFalling = true;
    private float secondsSinceStopped = 0;
    private int sideLanded = -1;
    private Vector3 initialPosition;

    [Header("Side Detection - Each sphere represents a side of the dice")]
    [SerializeField]
    public SphereCollider sphere1, sphere2, sphere3, sphere4, sphere5, sphere6;

    [Header("Dice Properties")]
    public float totalMass = 1f;

    [Header("Dice Bounds - Set to match your prefab mesh")]
    public Vector3 boundsMin = new Vector3(-0.5f, -0.5f, -0.5f);
    public Vector3 boundsMax = new Vector3(0.5f, 0.5f, 0.5f);
    public bool useAutoBounds = true;

    [Header("Void Properties")]
    public bool enableVoid = true;
    public Vector3 voidCenter = new Vector3(0.15f, 0.1f, 0f);
    public float voidRadius = 0.25f;

    [Header("Sampling Settings")]
    public int samplePoints = 10000;

    [Header("Visualization")]
    public bool showVoidVisualization = true;
    public Color diceColor = new Color(0.5f, 0.5f, 1f, 0.5f);
    public Color voidColor = new Color(1f, 0f, 0f, 0.8f);
    public Color centerOfMassColor = Color.green;

    private Rigidbody rb;
    private Bounds diceBounds;
    private MeshCollider meshCollider;
    private bool physicsCalculated = false;

    private GameObject voidSphere;
    private GameObject centerOfMassSphere;
    private Material diceMaterial;
    private Material originalMaterial;

    void Start()
    {
        initialPosition = transform.position;
        Time.timeScale = 1;
        transform.rotation = Random.rotation;
        lastYPosition = transform.position.y;

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

        rb = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();

        if (rb == null)
        {
            Debug.LogError("IrregularDice:  Rigidbody component not found!");
            return;
        }

        if (useAutoBounds)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                diceBounds = meshFilter.sharedMesh.bounds;
            }
            else
            {
                Debug.LogWarning("IrregularDice: MeshFilter not found, using manual bounds.");
                diceBounds = new Bounds();
                diceBounds.SetMinMax(boundsMin, boundsMax);
            }
        }
        else
        {
            diceBounds = new Bounds();
            diceBounds.SetMinMax(boundsMin, boundsMax);
        }

        SetupVisualization();
        CalculateMassDistribution();
    }

    void SetupVisualization()
    {
        SetupTransparentDice();

        if (showVoidVisualization && enableVoid)
        {
            CreateVoidSphere();
        }

        CreateCenterOfMassSphere();
    }

    void SetupTransparentDice()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;

            diceMaterial = new Material(Shader.Find("Standard"));
            diceMaterial.SetFloat("_Mode", 3);
            diceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            diceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            diceMaterial.SetInt("_ZWrite", 0);
            diceMaterial.DisableKeyword("_ALPHATEST_ON");
            diceMaterial.EnableKeyword("_ALPHABLEND_ON");
            diceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            diceMaterial.renderQueue = 3000;
            diceMaterial.color = diceColor;

            meshRenderer.material = diceMaterial;
        }
    }

    void CreateVoidSphere()
    {
        voidSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        voidSphere.name = "VoidVisualization";
        voidSphere.transform.SetParent(transform);
        voidSphere.transform.localPosition = voidCenter;
        voidSphere.transform.localScale = Vector3.one * voidRadius * 2f;

        Collider voidCollider = voidSphere.GetComponent<Collider>();
        if (voidCollider != null)
        {
            Destroy(voidCollider);
        }

        MeshRenderer voidRenderer = voidSphere.GetComponent<MeshRenderer>();
        if (voidRenderer != null)
        {
            Material voidMaterial = new Material(Shader.Find("Standard"));
            voidMaterial.SetFloat("_Mode", 3);
            voidMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            voidMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            voidMaterial.SetInt("_ZWrite", 0);
            voidMaterial.DisableKeyword("_ALPHATEST_ON");
            voidMaterial.EnableKeyword("_ALPHABLEND_ON");
            voidMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            voidMaterial.renderQueue = 3001;
            voidMaterial.color = voidColor;

            voidRenderer.material = voidMaterial;
        }
    }

    void CreateCenterOfMassSphere()
    {
        centerOfMassSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centerOfMassSphere.name = "CenterOfMassVisualization";
        centerOfMassSphere.transform.SetParent(transform);
        centerOfMassSphere.transform.localPosition = Vector3.zero;
        centerOfMassSphere.transform.localScale = Vector3.one * 0.1f;

        Collider comCollider = centerOfMassSphere.GetComponent<Collider>();
        if (comCollider != null)
        {
            Destroy(comCollider);
        }

        MeshRenderer comRenderer = centerOfMassSphere.GetComponent<MeshRenderer>();
        if (comRenderer != null)
        {
            Material comMaterial = new Material(Shader.Find("Standard"));
            comMaterial.SetFloat("_Mode", 0);
            comMaterial.color = centerOfMassColor;
            comMaterial.EnableKeyword("_EMISSION");
            comMaterial.SetColor("_EmissionColor", centerOfMassColor);

            comRenderer.material = comMaterial;
        }
    }

    void UpdateCenterOfMassVisualization()
    {
        if (centerOfMassSphere != null && rb != null)
        {
            centerOfMassSphere.transform.localPosition = rb.centerOfMass;
        }
    }

    void Update()
    {
        if (transform.position.y < lastYPosition)
        {
            lastYPosition = transform.position.y;
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }

        if (!isFalling)
        {
            secondsSinceStopped += Time.deltaTime;
        }

        if (secondsSinceStopped > 4)
        {
            transform.position = initialPosition;
            transform.rotation = Random.rotation;
            lastYPosition = transform.position.y;
            secondsSinceStopped = 0;

            int onTop = Mathf.Abs(sideLanded - 7);

            GameObject gameObject1 = GameObject.FindGameObjectWithTag("Summary");
            SummaryCreater summaryCreater = gameObject1.GetComponent<SummaryCreater>();
            summaryCreater.addSideCount(onTop);
        }
    }

    public void setSideLanded(int side)
    {
        this.sideLanded = side;
    }

    void CalculateMassDistribution()
    {
        Vector3 centerOfMassSum = Vector3.zero;
        int validPointCount = 0;
        Vector3[] validPoints = new Vector3[samplePoints];

        for (int i = 0; i < samplePoints; i++)
        {
            Vector3 point = new Vector3(
                Random.Range(diceBounds.min.x, diceBounds.max.x),
                Random.Range(diceBounds.min.y, diceBounds.max.y),
                Random.Range(diceBounds.min.z, diceBounds.max.z)
            );

            if (IsInsideMesh(point) && !IsInsideVoid(point))
            {
                validPoints[validPointCount] = point;
                centerOfMassSum += point;
                validPointCount++;
            }
        }

        if (validPointCount == 0)
        {
            Debug.LogWarning("IrregularDice:  No valid sample points found. Using default physics.  " +
                           "Check that MeshCollider is set up correctly and bounds are appropriate.");
            rb.mass = totalMass;
            rb.centerOfMass = Vector3.zero;
            physicsCalculated = false;
            UpdateCenterOfMassVisualization();
            return;
        }

        rb.mass = totalMass;

        Vector3 centerOfMass = centerOfMassSum / validPointCount;

        if (IsValidVector(centerOfMass))
        {
            rb.centerOfMass = centerOfMass;
        }
        else
        {
            Debug.LogWarning("IrregularDice: Invalid center of mass calculated.  Using default (0,0,0).");
            rb.centerOfMass = Vector3.zero;
        }

        Vector3 inertiaTensor = CalculateInertiaTensor(validPoints, validPointCount, centerOfMass, totalMass);

        if (IsValidVector(inertiaTensor) && inertiaTensor.x > 0 && inertiaTensor.y > 0 && inertiaTensor.z > 0)
        {
            rb.inertiaTensor = inertiaTensor;
        }
        else
        {
            Debug.LogWarning("IrregularDice: Invalid inertia tensor calculated. Using default.");
        }

        Quaternion inertiaTensorRotation = CalculateInertiaTensorRotation(validPoints, validPointCount, centerOfMass, totalMass);

        if (IsValidQuaternion(inertiaTensorRotation))
        {
            rb.inertiaTensorRotation = inertiaTensorRotation;
        }
        else
        {
            Debug.LogWarning("IrregularDice: Invalid inertia tensor rotation calculated. Using default.");
            rb.inertiaTensorRotation = Quaternion.identity;
        }

        physicsCalculated = true;

        UpdateCenterOfMassVisualization();

        Debug.Log($"Irregular Hollow Dice Physics Calculated:");
        Debug.Log($"  Mass: {totalMass:F4} kg");
        Debug.Log($"  Center of Mass: {rb.centerOfMass}");
        Debug.Log($"  Inertia Tensor:  {rb.inertiaTensor}");
        Debug.Log($"  Valid sample ratio: {(float)validPointCount / samplePoints:P1}");
    }

    bool IsValidVector(Vector3 v)
    {
        return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
               !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
    }

    bool IsValidQuaternion(Quaternion q)
    {
        return !float.IsNaN(q.x) && !float.IsNaN(q.y) && !float.IsNaN(q.z) && !float.IsNaN(q.w) &&
               !float.IsInfinity(q.x) && !float.IsInfinity(q.y) && !float.IsInfinity(q.z) && !float.IsInfinity(q.w);
    }

    bool IsInsideMesh(Vector3 point)
    {
        if (meshCollider == null)
        {
            return true;
        }

        Vector3 worldPoint = transform.TransformPoint(point);
        Vector3 rayOrigin = worldPoint + Vector3.up * 100f;
        Ray ray = new Ray(rayOrigin, Vector3.down);

        RaycastHit[] hits = Physics.RaycastAll(ray, 200f);
        int intersections = 0;

        foreach (var hit in hits)
        {
            if (hit.collider == meshCollider)
                intersections++;
        }

        return (intersections % 2) == 1;
    }

    bool IsInsideVoid(Vector3 point)
    {
        if (!enableVoid)
        {
            return false;
        }
        return Vector3.Distance(point, voidCenter) <= voidRadius;
    }

    Vector3 CalculateInertiaTensor(Vector3[] points, int count, Vector3 com, float mass)
    {
        if (count == 0)
        {
            return new Vector3(1f, 1f, 1f);
        }

        float massPerPoint = mass / count;

        float Ixx = 0f;
        float Iyy = 0f;
        float Izz = 0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 r = points[i] - com;

            Ixx += massPerPoint * (r.y * r.y + r.z * r.z);
            Iyy += massPerPoint * (r.x * r.x + r.z * r.z);
            Izz += massPerPoint * (r.x * r.x + r.y * r.y);
        }

        Ixx = Mathf.Max(Ixx, 0.001f);
        Iyy = Mathf.Max(Iyy, 0.001f);
        Izz = Mathf.Max(Izz, 0.001f);

        return new Vector3(Ixx, Iyy, Izz);
    }

    Quaternion CalculateInertiaTensorRotation(Vector3[] points, int count, Vector3 com, float mass)
    {
        if (count == 0)
        {
            return Quaternion.identity;
        }

        float massPerPoint = mass / count;

        float Ixx = 0f, Iyy = 0f, Izz = 0f;
        float Ixy = 0f, Ixz = 0f, Iyz = 0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 r = points[i] - com;

            Ixx += massPerPoint * (r.y * r.y + r.z * r.z);
            Iyy += massPerPoint * (r.x * r.x + r.z * r.z);
            Izz += massPerPoint * (r.x * r.x + r.y * r.y);

            Ixy -= massPerPoint * r.x * r.y;
            Ixz -= massPerPoint * r.x * r.z;
            Iyz -= massPerPoint * r.y * r.z;
        }

        Matrix4x4 inertiaTensorMatrix = new Matrix4x4();
        inertiaTensorMatrix.SetRow(0, new Vector4(Ixx, Ixy, Ixz, 0));
        inertiaTensorMatrix.SetRow(1, new Vector4(Ixy, Iyy, Iyz, 0));
        inertiaTensorMatrix.SetRow(2, new Vector4(Ixz, Iyz, Izz, 0));
        inertiaTensorMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

        Vector3 eigenvalues;
        Matrix4x4 eigenvectors;
        ComputeEigendecomposition(inertiaTensorMatrix, out eigenvalues, out eigenvectors);

        Quaternion rotation = QuaternionFromMatrix(eigenvectors);

        return rotation;
    }

    void ComputeEigendecomposition(Matrix4x4 matrix, out Vector3 eigenvalues, out Matrix4x4 eigenvectors)
    {
        float[,] a = new float[3, 3];
        float[,] v = new float[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                a[i, j] = matrix[i, j];
                v[i, j] = (i == j) ? 1f : 0f;
            }
        }

        int maxIterations = 50;
        for (int iter = 0; iter < maxIterations; iter++)
        {
            int p = 0, q = 1;
            float maxOffDiag = Mathf.Abs(a[0, 1]);

            if (Mathf.Abs(a[0, 2]) > maxOffDiag) { maxOffDiag = Mathf.Abs(a[0, 2]); p = 0; q = 2; }
            if (Mathf.Abs(a[1, 2]) > maxOffDiag) { maxOffDiag = Mathf.Abs(a[1, 2]); p = 1; q = 2; }

            if (maxOffDiag < 1e-10f) break;

            float theta;
            float diff = a[q, q] - a[p, p];
            if (Mathf.Abs(diff) < 1e-10f)
            {
                theta = Mathf.PI / 4f;
            }
            else
            {
                theta = 0.5f * Mathf.Atan2(2f * a[p, q], diff);
            }

            float c = Mathf.Cos(theta);
            float s = Mathf.Sin(theta);

            float app = a[p, p];
            float aqq = a[q, q];
            float apq = a[p, q];

            a[p, p] = c * c * app - 2f * s * c * apq + s * s * aqq;
            a[q, q] = s * s * app + 2f * s * c * apq + c * c * aqq;
            a[p, q] = 0f;
            a[q, p] = 0f;

            for (int i = 0; i < 3; i++)
            {
                if (i != p && i != q)
                {
                    float aip = a[i, p];
                    float aiq = a[i, q];
                    a[i, p] = c * aip - s * aiq;
                    a[p, i] = a[i, p];
                    a[i, q] = s * aip + c * aiq;
                    a[q, i] = a[i, q];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                float vip = v[i, p];
                float viq = v[i, q];
                v[i, p] = c * vip - s * viq;
                v[i, q] = s * vip + c * viq;
            }
        }

        eigenvalues = new Vector3(a[0, 0], a[1, 1], a[2, 2]);

        eigenvectors = Matrix4x4.identity;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                eigenvectors[i, j] = v[i, j];
            }
        }
    }

    Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        Vector3 forward = new Vector3(m[0, 2], m[1, 2], m[2, 2]);
        Vector3 up = new Vector3(m[0, 1], m[1, 1], m[2, 1]);

        if (forward.sqrMagnitude < 0.001f)
        {
            return Quaternion.identity;
        }

        if (up.sqrMagnitude < 0.001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(forward.normalized, up.normalized);
    }

    public void SetVisualizationEnabled(bool enabled)
    {
        showVoidVisualization = enabled;

        if (voidSphere != null)
        {
            voidSphere.SetActive(enabled && enableVoid);
        }

        if (centerOfMassSphere != null)
        {
            centerOfMassSphere.SetActive(enabled);
        }
    }

    public void SetDiceTransparency(float alpha)
    {
        diceColor.a = alpha;

        if (diceMaterial != null)
        {
            diceMaterial.color = diceColor;
        }
    }

    void OnDestroy()
    {
        if (diceMaterial != null)
        {
            Destroy(diceMaterial);
        }

        if (voidSphere != null)
        {
            Destroy(voidSphere);
        }

        if (centerOfMassSphere != null)
        {
            Destroy(centerOfMassSphere);
        }
    }

    void OnDrawGizmosSelected()
    {
        Bounds drawBounds;

        if (useAutoBounds)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                drawBounds = meshFilter.sharedMesh.bounds;
            }
            else
            {
                drawBounds = new Bounds();
                drawBounds.SetMinMax(boundsMin, boundsMax);
            }
        }
        else
        {
            drawBounds = new Bounds();
            drawBounds.SetMinMax(boundsMin, boundsMax);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position + drawBounds.center, drawBounds.size);

        if (enableVoid)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + voidCenter, voidRadius);
        }

        Rigidbody rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.TransformPoint(rigidBody.centerOfMass), 0.05f);
        }
    }
}