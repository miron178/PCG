using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePoly : MonoBehaviour
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

    void AddCentre(ref Vector3[] vert, ref Vector2[] uv,  Outline.Box box)
    {
        float x = (box.max.x - box.min.x) * 0.5f + box.min.x;
        float y = (box.max.y - box.min.y) * 0.5f + box.min.y;
        vert[0] = new Vector3(x, 0, y);
        uv[0]   = new Vector2(0.5f, 0.5f);
    }

    void Generate(LinkedList<Vector2> poly)
    {
        int count = poly.Count;

        Vector3[] newVertices = new Vector3[count + 1];
        Vector2[] newUV = new Vector2[count + 1];
        int[] newTriangles = new int[3 * count];

        Outline.Box box = Outline.BoundingBox(poly);
        AddCentre(ref newVertices, ref newUV, box);

        Vector2 offset = box.min;
        Vector2 size = new Vector2(box.max.x - box.min.x, box.max.y - box.min.y);
        Vector2 sizeInv = new Vector2(1f / size.x, 1f / size.y);

        LinkedListNode<Vector2> vert2d = poly.First;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector3 vert3d = new Vector3(vert2d.Value.x, 0, vert2d.Value.y);

            //vert coordinate
            newVertices[i + 1] = vert3d;

            //texture
            newUV[i + 1].x = (vert2d.Value.x - offset.x) * sizeInv.x;
            newUV[i + 1].y = (vert2d.Value.y - offset.y) * sizeInv.y;

            //make triangle
            newTriangles[3 * i + 0] = 0;
            newTriangles[3 * i + 1] = i + 1;
            newTriangles[3 * i + 2] = i + 2;

            vert2d = vert2d.Next;
        }
        //return to first point
        newTriangles[3 * poly.Count - 1] = 1;

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
