using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGeneration : MonoBehaviour
{
    [SerializeField]
    Camera useCamera;

    [SerializeField]
    GameObject roofPrefab;

    [SerializeField]
    GameObject wallsPrefab;

    [SerializeField]
    public int randSeed = 0;

    [SerializeField]
    [Range(1, 10)]
    int minSections = 1;

    [SerializeField]
    [Range(1, 10)]
    int maxSections = 4;

    [SerializeField]
    [Range(1, 20)]
    int minFloors = 1;

    [SerializeField]
    [Range(1, 20)]
    int maxFloors = 10;

    [SerializeField]
    [Range(4, 10)]
    int minSides = 4;

    [SerializeField]
    [Range(4, 10)]
    int maxSides = 8;

    [SerializeField]
    [Range(2, 30)]
    float minRadius = 4;

    [SerializeField]
    [Range(2, 30)]
    float maxRadius = 10;

    [SerializeField]
    [Range(2, 3)]
    float floorHeight= 2.5f;

    [SerializeField]
    [Range(1, 10)]
    float roofRepeat = 2f;

    [SerializeField]
    [Range(1, 10)]
    float wallRepeat = 2f;

    [SerializeField]
    [Range(1, 20)]
    float destroyDelay = 5f;
    float destroyTime = Mathf.Infinity;

    bool box = false;

    public delegate void OnDestroyCallback(int randSeed);
    public OnDestroyCallback onDestroy;

    struct Section
    {
        public LinkedList<Vector2> poly;
        public int numOfFloors;
    };

    struct BuildingParams
    {
        public int numSections;
        public Section[] sections;
        public Bounds bounds;
    };

    BuildingParams buildingParams;

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

    void AddCentre(ref Vector3[] vert, ref Vector2[] uv, Outline.Box box)
    {
        Vector2 size = new Vector2(box.max.x - box.min.x, box.max.y - box.min.y);
        float x = size.x * 0.5f + box.min.x;
        float y = size.y * 0.5f + box.min.y;
        vert[0] = new Vector3(x, 0, y);
        uv[0].x = (x - box.min.x) / roofRepeat;
        uv[0].y = (y - box.min.y) / roofRepeat;
    }

    GameObject GenerateRoof(LinkedList<Vector2> poly)
    {
        GameObject roof = GameObject.Instantiate(roofPrefab);
        MeshFilter meshFilter = roof.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int count = poly.Count;

        Vector3[] newVertices = new Vector3[count + 1];
        Vector2[] newUV = new Vector2[count + 1];
        int[] newTriangles = new int[3 * count];

        Outline.Box box = Outline.BoundingBox(poly);
        AddCentre(ref newVertices, ref newUV, box);

        float scaleUV = 1f / roofRepeat;

        LinkedListNode<Vector2> vert2d = poly.First;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector3 vert3d = new Vector3(vert2d.Value.x, 0, vert2d.Value.y);

            //vert coordinate
            newVertices[i + 1] = vert3d;

            //texture
            newUV[i + 1].x = (vert2d.Value.x - box.min.x) * scaleUV;
            newUV[i + 1].y = (vert2d.Value.y - box.min.y) * scaleUV;

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
        
        return roof;
    }

    GameObject GenerateWalls(LinkedList<Vector2> poly, int numFloors)
    {
        GameObject walls = GameObject.Instantiate(wallsPrefab);
        MeshFilter meshFilter = walls.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int count = poly.Count;

        //Texture scale to repeat wall pattern
        float scaleU = 1 / wallRepeat;
        float distanceU = 0;

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
            newUV[i].x = distanceU * scaleU;
            newUV[i].y = numFloors;

            //texture bottom
            newUV[i + bottomOffset].x = newUV[i].x;
            newUV[i + bottomOffset].y = 0f;

            LinkedListNode<Vector2> next = Outline.NextVertex(vert2d);
            distanceU += (next.Value - vert2d.Value).magnitude;
            vert2d = next;
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

        return walls;
    }

    GameObject GenerateSection(LinkedList<Vector2> poly, int numOfFloors)
    {
        GameObject section = new GameObject("section");
        GameObject roof = GenerateRoof(poly);
        GameObject walls = GenerateWalls(poly, numOfFloors);

        roof.transform.parent = section.transform;
        walls.transform.parent = section.transform;
        return section;
    }

    BuildingParams GenerateParams(int seed)
    {
        Random.InitState(seed);

        BuildingParams bp = new BuildingParams();
        bp.numSections = Random.Range(minSections, maxSections);

        LinkedList<Vector2> poly = null;
        Vector2 origin = Vector2.zero;
        float height = 0;

        bp.sections = new Section[bp.numSections];
        for (int i = 0; i < bp.numSections; i++)
        {
            float angle = Random.Range(0f, 360f);
            int numOfSides = Random.Range(minSides, maxSides);
            float radius = Random.Range(minRadius, maxRadius);
            bp.sections[i].numOfFloors = Random.Range(minFloors, maxFloors);

            LinkedList<Vector2> newPoly = CreatePoly(origin, angle, numOfSides, radius);
            poly = poly == null ? newPoly : Outline.Add(poly, newPoly);
            bp.sections[i].poly = poly;

            foreach(var vertex in poly)
            {
                bp.bounds.Encapsulate(new Vector3(vertex.x, height, vertex.y));
            }
            //prepare for the next section
            int start = Random.Range(0, poly.Count);
            origin = FindVertex(poly.First, start).Value;
            height += floorHeight * bp.sections[i].numOfFloors;
        }

        //add the last section height - one vertex will do
        bp.bounds.Encapsulate(new Vector3(poly.First.Value.x, height, poly.First.Value.y));

        return bp;
    }

    void GenerateBuilding(BuildingParams bp)
    {
        float offset = bp.bounds.size.y;
        for (int i = 0; i < bp.numSections; i++)
        {
            GameObject section = GenerateSection(bp.sections[i].poly, bp.sections[i].numOfFloors);
            section.transform.parent = transform;
            section.transform.localPosition = new Vector3(0, offset, 0);

            offset -= floorHeight * bp.sections[i].numOfFloors;
        }
    }

    void GenerateBox(BuildingParams bp)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = transform;
        cube.transform.localPosition = new Vector3(0, bp.bounds.center.y, 0);
        cube.transform.localScale = bp.bounds.size;
    }

    private void Clear()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
    }

    bool IsVisible()
    {
        Plane[] frustrum = GeometryUtility.CalculateFrustumPlanes(useCamera);
        Bounds world = buildingParams.bounds;
        world.center = transform.TransformPoint(world.center);
        bool visible = GeometryUtility.TestPlanesAABB(frustrum, world);
        return visible;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (useCamera == null)
        {
            useCamera = Camera.main;
        }

        buildingParams = GenerateParams(randSeed);
        GenerateBuilding(buildingParams);


        if (IsVisible())
        {
            box = false;
            Clear();
            GenerateBuilding(buildingParams);
        }
        else
        {
            box = true;
            Clear();
            GenerateBox(buildingParams);
            Invoke(nameof(Destroy), destroyDelay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool visible = IsVisible();
        if (box && visible)
        {
            CancelInvoke();
            box = false;
            Clear();
            GenerateBuilding(buildingParams);
        }
        else if (!box && !visible)
        {
            box = true;
            Clear();
            GenerateBox(buildingParams);
            Invoke(nameof(Destroy), destroyDelay);
        }
    }

    private void Destroy()
    {
        GameObject.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (onDestroy != null)
        {
            onDestroy(randSeed);
        }
    }
}
