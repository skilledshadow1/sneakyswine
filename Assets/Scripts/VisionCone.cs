using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class VisionCone : MonoBehaviour
{
    private Vector3 visionTarget;
    private Transform[] eyes;
    public bool isPeripheral = false;
    public bool playerInView;

    [Header("Vision Objects")]
    [SerializeField] private GameObject visionOrigin;
    
    [Header("Vision Stats")]
    [SerializeField] float distance = 10;
    [SerializeField] float angle = 30;
    [SerializeField] private float height = 1.0f;
    [SerializeField] private int segments = 10;
    [SerializeField] private int scanFrequency = 30;
    [SerializeField] private Color coneColor = Color.red;
    [SerializeField] private Color hiddenColor = Color.red;
    [SerializeField] private Color foundColor = Color.green;
    
    [Header("VisionLayers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    [HideInInspector] public List<GameObject> gameObjects = new List<GameObject>();
    private Collider[] colliders = new Collider[50];
    private int count;
    private float scanInterval;
    private float scanTimer;
    private Mesh wedgeMesh;
    // Start is called before the first frame update
    void Start()
    {
        scanInterval = 1.0f / scanFrequency;
        scanTimer = scanInterval;
        eyes = new Transform[visionOrigin.transform.childCount];
        for (int i = 0; i < visionOrigin.transform.childCount; i++)
        {
            eyes[i] = visionOrigin.transform.GetChild(i);
            Debug.Log(eyes[i].name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, playerLayer, QueryTriggerInteraction.Collide);
        
        gameObjects.Clear();
        for (int i = 0; i < count; i++)
        {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj))
            {
                playerInView = true;
                gameObjects.Add(obj);
                return;
            }
        }
        playerInView = false;
    }

    public bool IsInSight(GameObject obj)
    {
        Vector3 origin = visionOrigin.transform.position;
        Vector3 dest = obj.transform.position;
        
        //Gets local position so you are able so the tilt of the farmer's head doesn't affect this
        Vector3 localTarget = visionOrigin.transform.InverseTransformPoint(dest);
        
        Vector3 topCenterWorld = visionOrigin.transform.position + visionOrigin.transform.up * height;
        Vector3 bottomCenterWorld = visionOrigin.transform.position + visionOrigin.transform.up * -height;
        
        float yMax = visionOrigin.transform.InverseTransformPoint(topCenterWorld).y;;
        float yMin = visionOrigin.transform.InverseTransformPoint(bottomCenterWorld).y;
        
        if (yMin >= localTarget.y || localTarget.y > yMax)
        {
            return false;
        }

        
        Vector2 flatTarget = new Vector2(localTarget.x, localTarget.z);

        
        float targetAngle = Mathf.Atan2(flatTarget.x, flatTarget.y) * Mathf.Rad2Deg;
        if (Mathf.Abs(targetAngle) > angle)
        {
            return false;
        }
        
        visionTarget = dest;
        
        
        int eyesBlocked = 0;
        foreach(Transform eye in eyes)
        {
            if(Physics.Linecast(eye.position, dest, obstacleLayer)) eyesBlocked++;
        }

        return eyesBlocked < eyes.Length;
    }


    Mesh CreateWedgeMesh()
    {
        Mesh mesh = new Mesh();

        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        // Use visionOrigin's local position as the base
        Vector3 localOrigin = visionOrigin.transform.localPosition;

        Quaternion localRotation = visionOrigin.transform.localRotation;

        // Bottom and top centers
        Vector3 bottomCenter = localOrigin + Vector3.up * -height;
        Vector3 bottomLeft = localRotation * Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance + localOrigin + Vector3.up * -height;;
        Vector3 bottomRight = localRotation * Quaternion.Euler(0, angle, 0) * Vector3.forward * distance + localOrigin + Vector3.up * -height;

        Vector3 topCenter = bottomCenter + Vector3.up * 2 * height;
        Vector3 topLeft = bottomLeft + Vector3.up * 2 * height;
        Vector3 topRight = bottomRight + Vector3.up * 2 * height;

        int currentVertex = 0;

        // Add vertices for the sides and segments
        vertices[currentVertex++] = bottomCenter;
        vertices[currentVertex++] = bottomLeft;
        vertices[currentVertex++] = topLeft;

        vertices[currentVertex++] = topLeft;
        vertices[currentVertex++] = topCenter;
        vertices[currentVertex++] = bottomCenter;

        vertices[currentVertex++] = bottomCenter;
        vertices[currentVertex++] = topCenter;
        vertices[currentVertex++] = topRight;

        vertices[currentVertex++] = topRight;
        vertices[currentVertex++] = bottomRight;
        vertices[currentVertex++] = bottomCenter;

        float currentAngle = -angle; // Leftmost angle
        float deltaAngle = (angle * 2) / segments;

        for (int i = 0; i < segments; i++)
        {
            bottomLeft = localRotation * Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance +
                         localOrigin + -height * Vector3.up;
            bottomRight =
                localRotation * Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance +
                localOrigin + -height * Vector3.up;

            topLeft = bottomLeft + Vector3.up * 2 * height;
            topRight = bottomRight + Vector3.up * 2 * height;

            vertices[currentVertex++] = bottomLeft;
            vertices[currentVertex++] = bottomRight;
            vertices[currentVertex++] = topRight;

            vertices[currentVertex++] = topRight;
            vertices[currentVertex++] = topLeft;
            vertices[currentVertex++] = bottomLeft;

            vertices[currentVertex++] = topCenter;
            vertices[currentVertex++] = topLeft;
            vertices[currentVertex++] = topRight;

            vertices[currentVertex++] = bottomCenter;
            vertices[currentVertex++] = bottomRight;
            vertices[currentVertex++] = bottomLeft;

            currentAngle += deltaAngle;
        }
        for (int i = 0; i < numVertices; i++)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnDrawGizmos()
    {
        wedgeMesh = CreateWedgeMesh();
        if (wedgeMesh)
        {
            Gizmos.color = coneColor;
            Gizmos.DrawMesh(wedgeMesh, visionOrigin.transform.position, visionOrigin.transform.rotation);
        }
        
        Gizmos.color = hiddenColor;
        Gizmos.DrawWireSphere(visionOrigin.transform.position, distance);
        for (int i = 0; i < count; ++i)
        {
            Gizmos.DrawSphere(colliders[i].transform.position, 0.2f);
        }

        Gizmos.color = foundColor;
        foreach (var obj in gameObjects)
        {
            Gizmos.DrawSphere(obj.transform.position, 0.2f);
        }
        
        Gizmos.color = Color.black;
        
        foreach (var obj in gameObjects)
        {
            foreach(Transform eye in eyes)
            {
                if (!Physics.Linecast(eye.position, visionTarget, obstacleLayer))
                {
                    Gizmos.DrawLine(eye.position, visionTarget); 
                }
                    
            }
        }
        
        
    }
}
