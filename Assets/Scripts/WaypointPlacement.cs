using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaypointPlacement : MonoBehaviour
{
    [SerializeField] private float desiredYCoordinate = 0f;
    [ContextMenu("Reset Y Coordinate")]
    void ResetY()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        int lineRendererLength = lineRenderer.positionCount;

        for(int i = 0; i < lineRendererLength; i++) 
        {
            Vector3 pos = lineRenderer.GetPosition(i);
            lineRenderer.SetPosition(i, new Vector3(Round(pos.x), desiredYCoordinate, Round(pos.z)));
        }


    }

    float Round(float number) 
    {
        return Mathf.Round(number * 100f) / 100f;
    }

}

