using Photon.Pun;
using UnityEngine;

/// <summary>
/// The networking messages of the point cloud
/// localVerts and localCols record the point cloud data as its being transmitted from Photon
/// Then it's sent to the PointCloudRenderer.
/// </summary>
public class NetworkPointCloud : MonoBehaviourPunCallbacks
{
    //Hard code 256 and 192 since its the size of iPhone point cloud
    Vector3[] localVerts = new Vector3[256 * 192];
    Color[] localCols = new Color[256 * 192];

    public void CallRPC(Vector3[] vec, Color[] col)
    {
        GetComponent<PhotonView>().RPC("NetworkSetCloud", RpcTarget.AllBuffered, vec, col);
    }

    public void CallSetCloudInPackagesRPC(Vector3[] vec, int index, bool setVertex)
    {
        if (setVertex)
            GetComponent<PhotonView>().RPC("NetworkSetCloudVertices", RpcTarget.AllBuffered, index, vec);
        else
            GetComponent<PhotonView>().RPC("NetworkSetCloudColors", RpcTarget.AllBuffered, index, vec);
    }


    [PunRPC]
    void NetworkSetCloud(Vector3[] vert, Color[] col)
    {
        GetComponent<PointCloudRenderer>().UpdateMeshInfo(vert, col);
    }

    [PunRPC]
    void NetworkSetCloudVertices(int index, Vector3[] vert)
    {
        for (int i = 0; i < vert.Length; ++i)
            localVerts[index * vert.Length + i] = vert[i];

        GetComponent<PointCloudRenderer>().UpdateMeshInfo(localVerts, localCols);
    }

    [PunRPC]
    void NetworkSetCloudColors(int index, Vector3[] col)
    {
        for (int i = 0; i < col.Length; ++i)
            localCols[index * col.Length + i] = new Color(col[i].x, col[i].y, col[i].z);

        GetComponent<PointCloudRenderer>().UpdateMeshInfo(localVerts, localCols);
    }

    [PunRPC]
    void NetworkConfirmPointCloud()
    {

    }
}