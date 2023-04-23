using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walls : MonoBehaviour
{
    [SerializeField]
    [Range(3, 10)]
    int numOfSides = 3;

    [SerializeField]
    [Range(0, 9)]
    int start2 = 0;

    [SerializeField]
    [Range(3, 10)]
    int numOfSides2 = 3;

    [SerializeField]
    float radius = 1;

    [SerializeField]
    float radius2 = 1;

    [SerializeField]
    float angle = 1;

    [SerializeField]
    float angle2 = 1;


    [SerializeField]
    int numFloors = 1;

    [SerializeField]
    float floorHeight = 1f;

    Mesh mesh = null;

    public static LinkedList<Vector2> CreatePoly(Vector2 origin, float angle, int numOfSides, float radius)
    {
        LinkedList<Vector2> poly = new LinkedList<Vector2>();

        float angleStep = 360f / numOfSides;
        for (int i = 0; i < numOfSides; i++)
        {
            //rotate to face next vert
            float angle2 = angle + i * angleStep;
            Quaternion rotation = Quaternion.AngleAxis(angle2, Vector3.up);
            Vector3 rotatedPoint = rotation * Vector3.right * radius;

            poly.AddLast(new Vector2(rotatedPoint.x, rotatedPoint.z) + origin);
        }
        return poly;
    }

    LinkedListNode<Vector2> FindVertex(LinkedListNode<Vector2> node, int count)
    {
        while (count > 0)
        {
            count--;
            node = Outline.NextVertex(node);
        }
        return node;
    }

    void Generate(LinkedList<Vector2> poly)
    {
        int count = poly.Count;
        //We need to duplicate the first point for texturing UV wrap
        //For example with square shape we need 5 points on top and bottom
        //where the first and the last point have identical position but
        //different UV coordinates: 0 for the 1st and 1 for the last point
        int numVertices = count + 1;

        //We put all points for the top and then all points for the bottom
        int bottomOffset = numVertices;

        Vector3[] newVertices = new Vector3[numVertices * 2];
        Vector2[] newUV = new Vector2[numVertices * 2];
        int[] newTriangles = new int[3 * count * 2];

        //newVertices[0] = Vector3.zero;
        //newUV[0] = new Vector2(0.5f, 0.5f);

        LinkedListNode<Vector2> vert2d = poly.First;
        float angleStep = 360f / count;
        float textureSep = 1f / count;
        Vector3 floorStep = Vector3.down * floorHeight * numFloors;
        for (int i = 0; i < numVertices; i++)
        {
            //rotate to face next vert
            //force angle back to exactly 0 on the last (repeated) step
            //float angle = i == count ? 0 : i * angleStep;

            // Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            //Vector3 rotatedPoint = rotation * Vector3.right;

            //vert coordinate top
            //newVertices[i] = rotatedPoint * radius;
            newVertices[i] = new Vector3(vert2d.Value.x, 0, vert2d.Value.y);


            //vert coordinate bottom
            newVertices[i + numVertices] = newVertices[i] + floorStep;

            //texture top
            newUV[i].x = i * textureSep;
            newUV[i].y = numFloors;

            //texture bottom
            newUV[i + bottomOffset].x = i * textureSep;
            newUV[i + bottomOffset].y = 0f;

            vert2d = Outline.NextVertex(vert2d);
        }

        for (int i = 0; i < count; i++)
        {
            //make triangle 1
            //
            //            0
            //           /|
            //          / |
            //         2--1
            //
            newTriangles[6 * i + 0] = i + 0;
            newTriangles[6 * i + 1] = bottomOffset + i + 0;
            newTriangles[6 * i + 2] = bottomOffset + i + 1;

            //make triangle 2
            //
            //         4--5
            //         | /
            //         |/
            //         3
            //
            newTriangles[6 * i + 3] = bottomOffset + i + 1;
            newTriangles[6 * i + 4] = i + 1;
            newTriangles[6 * i + 5] = i + 0;
        }

        //update mesh
        mesh.triangles = new int[0];
        mesh.uv = new Vector2[0];

        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        LinkedList<Vector2> poly1 = CreatePoly(Vector2.zero, angle, numOfSides, radius);
        LinkedList<Vector2> poly2 = CreatePoly(FindVertex(poly1.First, start2).Value, angle2, numOfSides2, radius2);
        LinkedList<Vector2> poly = Outline.Add(poly1, poly2);
        Generate(poly);
    }
}
