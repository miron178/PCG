using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CiryGenerator : MonoBehaviour
{
    [SerializeField]
    Vector2 spacing;

    [SerializeField]
    GameObject buildingPrefab;

    Camera useCamera;
    Bounds bounds;
    Dictionary<int, GameObject> created = null;

    int Hash(int a, int b)
    {
        int A = a >= 0 ? 2 * a : -2 * a - 1;
        int B = b >= 0 ? 2 * b : -2 * b - 1;
        return A >= B ? A * A + A + B : A + B * B;
    }

    void OnBuildingDestroy(int hash)
    {
        created.Remove(hash);
    }

    void GenerateCity(Bounds bounds)
    {
        int minX = Mathf.FloorToInt(bounds.min.x / spacing.x);
        int minZ = Mathf.FloorToInt(bounds.min.z / spacing.y);

        int maxX = Mathf.CeilToInt(bounds.max.x / spacing.x);
        int maxZ = Mathf.CeilToInt(bounds.max.z / spacing.y);

        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int hash = Hash(x, z);
                if (!created.ContainsKey(hash))
                {
                    GameObject spot = new GameObject("Spot " + x + ","  + z);
                    spot.transform.position = new Vector3(x * spacing.x, 0, z * spacing.y);
                    GameObject building = GameObject.Instantiate(buildingPrefab);
                    building.transform.parent = spot.transform;
                    building.transform.localPosition = Vector3.zero;

                    BuildingGeneration bg = building.GetComponent<BuildingGeneration>();
                    bg.randSeed = hash;
                    bg.onDestroy = OnBuildingDestroy;

                    created.Add(hash, building);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        useCamera = GetComponent<Camera>();
        created = new Dictionary<int, GameObject>();
        bounds = new Bounds();
    }

    // Update is called once per frame
    void Update()
    {
        bounds.center = transform.position;
        bounds.size = Vector3.zero;

        Vector3[] frustumCorners = new Vector3[4];
        useCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), useCamera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        for (int i = 0; i < frustumCorners.Length; i++)
        {
            var worldSpaceCorner = useCamera.transform.TransformPoint(frustumCorners[i]);
            bounds.Encapsulate(worldSpaceCorner);
        }
        bounds.Encapsulate(useCamera.transform.position);

        GenerateCity(bounds);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(
            new Vector3(bounds.center.x, 0, bounds.center.z),
            new Vector3(bounds.size.x, 1, bounds.size.z));
    }
}