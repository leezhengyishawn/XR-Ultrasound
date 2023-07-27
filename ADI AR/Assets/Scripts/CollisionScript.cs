using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionScript : MonoBehaviour
{
    [Header("Angles")]
    [SerializeField] float angleXY;
    [SerializeField] float angleYZ;
    [SerializeField] bool isPenetrating;

    private Vector3 projectXY;
    private Vector2 projectYZ;
    public Collider probe;
    public Collider sphere;
    public Transform strengthBarMask;
    private float distance;
    private Vector3 direction;
    private Color color;
    private float hue;

    public void Update()
    {

        if (probe.transform.localPosition.magnitude == 0)
        {
            hue = 120f;
        }
        else
        {
            hue = Mathf.Round(Mathf.Lerp(0, 120f, 1 - Mathf.InverseLerp(0f, 10f, probe.transform.localPosition.magnitude * 100f))) / 545;
        }
        color = Color.HSVToRGB(hue, 1, 1);
        //sphere.GetComponent<Renderer>().material.SetColor("_Color", color);
        strengthBarMask.localScale = new Vector3(Mathf.Round(Mathf.Lerp(100,0, Mathf.InverseLerp(0f, 20f, probe.transform.localPosition.magnitude * 600f)))/100,1,1);
        probe.attachedRigidbody.velocity = new Vector3(0, 0, 0);
        probe.attachedRigidbody.angularVelocity = new Vector3(0, 0, 0);

    }

    private void OnCollisionStay(Collision other)
    {
        // Print how many points are colliding with this transform
        Debug.Log("Points colliding: " + other.contacts.Length);

        // Print the normal of the first point in the collision.
        Debug.Log("Normal of the first point: " + other.contacts[0].normal);

        // Draw a different colored ray for every normal in the collision
        Debug.DrawRay(other.contacts[0].point, other.contacts[0].normal * 1000, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
        projectXY = Vector3.ProjectOnPlane(other.contacts[0].normal, probe.transform.forward);
        projectYZ = Vector3.ProjectOnPlane(other.contacts[0].normal, probe.transform.right);
        angleXY = Mathf.Round(Vector3.Angle(projectXY, probe.transform.up));
        angleYZ = Mathf.Round(Vector3.Angle(projectYZ, probe.transform.up));
        isPenetrating = Physics.ComputePenetration(colliderA: sphere, positionA: sphere.transform.position, rotationA: sphere.transform.rotation, colliderB: probe, positionB: probe.transform.position, rotationB: probe.transform.rotation, direction: out direction, distance: out distance);
        
    }

    
    private void OnCollisionExit(Collision collision)
    {
        hue = 120f;
        //text.text = "0.0\n0.0\n0\n0";
        probe.transform.localPosition = new Vector3(0, 0, 0);
        probe.transform.localRotation = Quaternion.identity;
    }
    
}
