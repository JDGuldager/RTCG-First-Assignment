using UnityEngine;

public class CreateGrid : MonoBehaviour
{
    public Transform prefab;
    public int gridResolution = 10;
    public Transform[] grid;
    public Vector3[] startPos;

    // Cache IDs and MPB to avoid allocations
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();

        grid = new Transform[gridResolution * gridResolution * gridResolution];
        startPos = new Vector3[gridResolution * gridResolution * gridResolution];

        for (int i = 0, z = 0; z < gridResolution; z++)
        {
            for (int y = 0; y < gridResolution; y++)
            {
                for (int x = 0; x < gridResolution; x++, i++)
                {
                    grid[i] = CreateGridPoint(x, y, z);
                    startPos[i] = GetCoordinates(x, y, z);
                }
            }
        }
    }

    Transform CreateGridPoint(int x, int y, int z)
    {
        Transform point = Instantiate(prefab);
        point.localPosition = GetCoordinates(x, y, z);

        // Unique per-cube color (your existing gradient)
        Color color = new Color(
            (float)x / gridResolution,
            (float)y / gridResolution,
            (float)z / gridResolution,
            1f
        );

        // Set per-renderer color without instantiating materials
        var renderer = point.GetComponent<MeshRenderer>();
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor(BaseColorId, color);
        renderer.SetPropertyBlock(mpb);

        return point;
    }

    Vector3 GetCoordinates(int x, int y, int z)
    {
        return new Vector3(
            x - (gridResolution - 1) * 0.5f,
            y - (gridResolution - 1) * 0.5f,
            z - (gridResolution - 1) * 0.5f
        );
    }
}
