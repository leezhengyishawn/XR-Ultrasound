using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ADI.XR
{
    public class ARRaycastHitResponse : MonoBehaviour
    {
        [SerializeField]
        private Camera arCamera;

        void Start()
        {
            HumanBodyTrackerUI.Instance.Print("Welcome"); 
            if (arCamera == null)
                arCamera = FindObjectOfType<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = arCamera.ScreenPointToRay(touch.position);
                    RaycastHit hitObject;


                    if(Physics.Raycast(ray, out hitObject))
                    {
                        //if (hitObject != null)
                        HumanBodyTrackerUI.Instance.Print(hitObject.collider.gameObject.name);                
                        HumanBodyTrackerUI.Instance.OpenModelEditingPanel(true);
                    }
                    else
                    {
                        HumanBodyTrackerUI.Instance.Print("MISS");                
                        //HumanBodyTrackerUI.Instance.OpenModelEditingPanel(false);
                    }
                }
            }
        }
/*
        ARRaycastManager m_RaycastManager;

        List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

        void Update()
        {
            if (Input.touchCount == 0)
                return;

            if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
            {
                // Only returns true if there is at least one hit
                HumanBodyTrackerUI.Instance.Print("MISS"); 
            }
        }
*/
    }
}