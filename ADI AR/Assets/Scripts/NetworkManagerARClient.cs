using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The network cloud is 256x192 cloud points per cloud
/// Sending the data for the vertices and color is too much data for one message
/// So we break it into multiple packets as stated by packetsToBreakInto
/// It should be a power2 number (16,32,64 etc)
/// </summary>
public class NetworkManagerARClient : MonoBehaviourPunCallbacks
{
    public int packetsToBreakInto = 32;

    public PointCloud m_masterPointCloud;
    public GameObject pointCloudPrefab;
    public Text connectionStatus;

    public void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.CleanupCacheOnLeave = false;
        PhotonNetwork.JoinOrCreateRoom("Main Room", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        connectionStatus.text = "CONNECTED";
    }

    public void CallRecordNetworkCloud()
    {
        StartCoroutine(RecordNetworkCloud());
    }

    public IEnumerator RecordNetworkCloud()
    {
        var newCloud = PhotonNetwork.Instantiate("PointCloudMeshRenderer",
                                  m_masterPointCloud.GetCameraManager().transform.position,
                                  m_masterPointCloud.GetCameraManager().transform.rotation,
                                  0);

        Vector3[] newVerts = m_masterPointCloud.GetVertices();
        Color[] newCols = m_masterPointCloud.GetColors();

        //Postion Vector: x,y,z of 4 bytes each. Color: r,g,b,a of 4 bytes each
        int size = (newVerts.Length * 3 * 4) + (newCols.Length * 4 * 4);
        connectionStatus.text = "Total Size " + size;

        //Send vertex info in X packages
        int packageLength = newVerts.Length / packetsToBreakInto;
        for (int i = 0; i < packetsToBreakInto; ++i)
        {
            Vector3[] vertsToSend = new Vector3[packageLength];

            for (int j = 0; j < packageLength; ++j)
                vertsToSend[j] = newVerts[i * packageLength + j];

            newCloud.GetComponent<NetworkPointCloud>().CallSetCloudInPackagesRPC(vertsToSend, i, true);
            yield return new WaitForSeconds(.1f);
        }

        //Send color info in X packages
        for (int i = 0; i < packetsToBreakInto; ++i)
        {
            Vector3[] colsToSend = new Vector3[packageLength];

            for (int j = 0; j < packageLength; ++j)
            {
                Color col = newCols[i * packageLength + j];
                colsToSend[j] = new Vector3(col.r, col.g, col.b);
            }
            newCloud.GetComponent<NetworkPointCloud>().CallSetCloudInPackagesRPC(colsToSend, i, false);
            yield return new WaitForSeconds(.1f);
        }

        //newCloud.GetComponent<NetworkPointCloud>().CallRPC(newVerts, newCols);
        connectionStatus.text = string.Format("Total:{0} Verts:{1} Cols:{2}", size, newVerts.Length, newCols.Length);
        //newCloud.GetComponent<PointCloudRenderer>().UpdateMeshInfo(newVerts, newCols);

        FindObjectOfType<RecordPointCloud>().recordedClouds.Add(newCloud);
    }

    public void CallRPCConfirmCloud()
    {
    }
}
