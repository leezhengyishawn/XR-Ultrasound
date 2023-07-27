using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Keeps a local list of 'snapshots' of rendered point cloud
/// </summary>
public class RecordPointCloud : MonoBehaviour
{
    public PointCloud m_masterPointCloud;
    public GameObject pointCloudPrefab;
    public List<GameObject> recordedClouds = new List<GameObject>();

    bool m_autoRecording = false;
    float m_recordRate = 0.5f;
    public Text recordingRateText;

    float timer;

    private void Update()
    {
        if (m_autoRecording)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = m_recordRate;
                RecordCloud();
            }
        }
    }

    public void ToggleAutoRecording()
    {
        m_autoRecording = !m_autoRecording;
        if (m_autoRecording)
        {
            timer = m_recordRate;
        }
    }

    public void ChangeAutoRecordingRate(System.Single newRate)
    {
        m_recordRate = newRate;
        recordingRateText.text = newRate.ToString();
    }

    public void RecordCloud()
    {
        var newCloud = Instantiate(pointCloudPrefab, m_masterPointCloud.GetCameraManager().transform);
        newCloud.transform.localPosition = Vector3.zero;
        newCloud.transform.localRotation = Quaternion.identity;


        Vector3[] newVerts = m_masterPointCloud.GetVertices();
        Color[] newCols = m_masterPointCloud.GetColors();


        newCloud.GetComponent<PointCloudRenderer>().UpdateMeshInfo(m_masterPointCloud.GetVertices(), m_masterPointCloud.GetColors());
        recordedClouds.Add(newCloud);
        newCloud.transform.parent = null;
    }

    bool isShown = true;

    public void HideCloud()
    {
        isShown = !isShown;
        foreach (var go in recordedClouds)
            go.SetActive(isShown);
    }

    public void DeleteCloud()
    {
        foreach (var go in recordedClouds)
            Destroy(go);
    }
}
