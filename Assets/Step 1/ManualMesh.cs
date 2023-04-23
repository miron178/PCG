using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualMesh : MonoBehaviour
{
    [SerializeField]
    Vector3[] newVertices;

    [SerializeField]
    Vector2[] newUV;

    [SerializeField]
    int[] newTriangles;

    Mesh mesh = null;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
    }
}
