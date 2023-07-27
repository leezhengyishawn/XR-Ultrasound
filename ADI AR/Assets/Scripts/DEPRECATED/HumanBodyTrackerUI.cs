using TMPro;
using UnityEngine;
using UnityEngine.UI;

//DEPRECATED

namespace ADI.XR
{
    public class HumanBodyTrackerUI : MonoBehaviour
    {
        public HumanBodyTracker m_bodyTracker;

        public static HumanBodyTrackerUI Instance { get{return FindObjectOfType<HumanBodyTrackerUI>();} }

        public TextMeshProUGUI debugLog;

        public TextMeshProUGUI xOffsetText, yOffsetText, zOffsetText;

        public GameObject sendDataMessage;

        public GameObject editModelPanel;





        // Start is called before the first frame update
        void Awake()
        {
            m_bodyTracker = FindObjectOfType<HumanBodyTracker>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void IncrementX(float x = 1.0f)
        {
            m_bodyTracker.xOffset += x;
            xOffsetText.text = "X: " + m_bodyTracker.xOffset.ToString();
        }

        public void IncrementY(float y = 1.0f)
        {
            m_bodyTracker.yOffset += y;
            yOffsetText.text = "Y: " + m_bodyTracker.yOffset.ToString();
        }

        public void IncrementZ(float z = 1.0f)
        {
            m_bodyTracker.zOffset += z;
            zOffsetText.text = "Z: " + m_bodyTracker.zOffset.ToString();
        }


        public void Print(string message)
        {
            debugLog.text = message;
        }
    
        public void SendData()
        {
            sendDataMessage.SetActive(true);
        }

        public void OpenModelEditingPanel(bool set = true)
        {
            editModelPanel.SetActive(set);
        }

        public void ScaleModel(float sizeIncrement = 0.1f)
        {
            Vector3 newScale = GameObject.Find("Spine2").transform.localScale + new Vector3(sizeIncrement, sizeIncrement, sizeIncrement);

            GameObject.Find("Spine2").transform.localScale = newScale;
            GameObject.Find("Spine3").transform.localScale = new Vector3(1f / newScale.x, 1f / newScale.y,1f / newScale.z);
        }

    }    
}