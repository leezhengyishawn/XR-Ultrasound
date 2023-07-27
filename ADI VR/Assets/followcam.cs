using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followcam : MonoBehaviour
{
      
    public Transform screen;
    public Transform xrOrigin;
    private float screenHeight = 1.6f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        screen.transform.position = new Vector3(xrOrigin.transform.position.x, screenHeight, xrOrigin.transform.position.z);

        var euler = xrOrigin.rotation.eulerAngles;
        screen.transform.rotation = Quaternion.Euler(90,180,-euler.y);
    }
}
