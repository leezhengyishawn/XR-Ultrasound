using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//[ExecuteInEditMode]
public class ComputePenetration : MonoBehaviour
{
    public Collider Body;
    public Collider Probe;
    public LineRenderer LineRenderer;

    [Space(22)]
    [Header("Results")]
    [SerializeField] float distance;
    [SerializeField] Vector3 direction;
    [SerializeField] bool isPenetrating;

    void Start()
    {
        Gizmos.color = Color.blue;
    }
    
    // Update is called once per frame
    void Update()
    {
        isPenetrating = Physics.ComputePenetration(colliderA: Body, positionA: Body.transform.position, rotationA: Body.transform.rotation, colliderB: Probe, positionB: Probe.transform.position, rotationB: Probe.transform.rotation, direction: out direction, distance: out distance);
        //Debug.DrawLine(Vector3.zero, direction);

        Gizmos.DrawLine(Vector3.zero, direction);
        //LineRenderer.SetPosition(0, Vector3.zero);
        //LineRenderer.SetPosition(1, direction);

    }

    private void OnCollisionEnter(Collision other)
    {
        
        // Print how many points are colliding with this transform
        //Debug.Log("Points colliding: " + other.contacts.Length);

        // Print the normal of the first point in the collision.
        //Debug.Log("Normal of the first point: " + other.contacts[0].normal);


        // Draw a different colored ray for every normal in the collision
        /*
        foreach (ContactPoint contact in other.contacts)
        {
            Debug.Log("Hit");
            Debug.DrawRay(contact.point, contact.normal, Color.red);
        }
        */
    }


}
