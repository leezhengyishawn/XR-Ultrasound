using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    // Apply these values in the editor
    public LineRenderer LineRenderer;
    //public Transform TransformOne;
    public Vector3 normal;

    void Start()
    {
        // set the color of the line
        LineRenderer.startColor = Color.red;
        LineRenderer.endColor = Color.red;

        // set width of the renderer
        LineRenderer.startWidth = 1f;
        LineRenderer.endWidth = 1f;

        // set the position
        LineRenderer.SetPosition(0, Vector3.zero);
        LineRenderer.SetPosition(1, normal);
    }
}
