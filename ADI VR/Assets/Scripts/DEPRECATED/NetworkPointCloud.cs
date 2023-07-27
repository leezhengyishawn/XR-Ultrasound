using Photon.Pun;
using UnityEngine;

public class NetworkPointCloud : MonoBehaviourPunCallbacks
{
    Vector3[] localVerts = new Vector3[256 * 192];
    Color[] localCols = new Color[256 * 192];
    //int chunkCount = 0;

    //public void CallRPC(Vector3[] vec, Color[] col)
    //{
    //    GetComponent<PhotonView>().RPC("NetworkSetCloud", RpcTarget.AllBuffered, vec, col);

    //}
    private void Start()
    {
        gameObject.transform.SetParent(GameObject.Find("PointCloudRoot").transform);
    }

    public void CallSetCloudInPackagesRPC(Vector3[] vec, int index, bool setVertex)
    {
        if (setVertex)
            GetComponent<PhotonView>().RPC("NetworkSetCloudVertices", RpcTarget.AllBuffered, index, vec);
        else
            GetComponent<PhotonView>().RPC("NetworkSetCloudColors", RpcTarget.AllBuffered, index, vec);
    }


    //[PunRPC]
    //void NetworkSetCloud(Vector3[] vert, Color[] col)
    //{
    //    GetComponent<PointCloudRenderer>().UpdateMeshInfo(vert, col);
    //}

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

        if (index * col.Length + col.Length - 1 == 192 * 256 - 1)
            this.photonView.RequestOwnership();
    }
}