using UnityEngine;

public class Transformations : MonoBehaviour
{
    CreateGrid createGrid;

    Vector3[] startPos;

    [Header("Movement Controls")]
    public float moveFrequency = 2f;
    public float moveAmplitude = 5f;
    public float moveOffset = 0f;

    Vector3 startScale = Vector3.zero;

    [Header("Scale Controls")]
    public float scaleFrequency = 2f;
    public float scaleAmplitude = 5f;
    public float scaleOffset = 0f;

    [Header("Rotation Controls")]
    public float rotationSpeed = 1f;

    private void Start()
    {
        createGrid = GetComponent<CreateGrid>();
        startPos = createGrid.startPos;
        startScale = createGrid.grid[0].localScale;
    }

    void Update()
    {
        for (int i = 0, z = 0; z < createGrid.gridResolution; z++)
        {
            for (int y = 0; y < createGrid.gridResolution; y++)
            {
                for (int x = 0; x < createGrid.gridResolution; x++, i++)
                {
                    createGrid.grid[i].localPosition = startPos[i] + Vector3.up * Mathf.Sin(Time.time * moveFrequency + moveOffset) * moveAmplitude;

                    float s = (Mathf.Sin(Time.time * scaleFrequency) + 1f) * 0.5f; // 0..1
                    createGrid.grid[i].localScale = startScale * (s * scaleAmplitude) + Vector3.one * scaleOffset;

                    createGrid.grid[i].Rotate(Vector3.forward * Time.deltaTime * rotationSpeed);
                }
            }
        }
    }

}
