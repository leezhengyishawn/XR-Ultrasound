using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deals with only rendering the point cloud
/// </summary>
public class PointCloudRenderer : MonoBehaviour
{
    Mesh mesh;
    int[] indices;

    public void UpdateMeshInfo(Vector3[] vertices, Color[] colors)
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            //PointCloud
            indices = new int[256 * 192];
            for (int i = 0; i < vertices.Length; i++)
                indices[i] = i;

            //mesh
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);

            //mesh
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
        }
        else
        {
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.RecalculateBounds();
        }
    }
}
