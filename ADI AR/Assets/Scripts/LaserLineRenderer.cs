using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserLineRenderer : MonoBehaviour
{
    public LineRenderer laserLineRenderer;
    public float laserWidth = 1f;
    public float laserMaxLength = 1f;

    void Start()
    {
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        laserLineRenderer.SetPositions(initLaserPositions);
        //        laserLineRenderer.SetWidth(laserWidth, laserWidth);
        laserLineRenderer.startWidth = laserLineRenderer.endWidth = laserWidth;
    }

    void Update()
    {
        ShootLaserFromTargetPosition(transform.position, new Vector3(0, 1, 0), laserMaxLength);
    }

    void ShootLaserFromTargetPosition(Vector3 targetPosition, Vector3 direction, float length)
    {
        //Get the direction of the parent transform
        direction = transform.parent.transform.up;

        Ray ray = new Ray(targetPosition, direction);
        RaycastHit raycastHit;
        Vector3 endPosition = targetPosition + (length * direction);

        if (Physics.Raycast(ray, out raycastHit, length))
        {
            endPosition = raycastHit.point;
        }

        laserLineRenderer.SetPosition(0, targetPosition);
        laserLineRenderer.SetPosition(1, endPosition);
    }

}
