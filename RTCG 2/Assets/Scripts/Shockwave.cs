using UnityEngine;

public class Shockwave : MonoBehaviour
{
    [Header("References")]
    public CreateGrid createGrid;              // Drag your CreateGrid object here
    public Camera cam;                         // Main camera (or leave empty to auto-find)

    [Header("Wave")]
    public float waveSpeed = 12f;              // units per second
    public float waveWidth = 1.5f;             // thickness of the ring
    public float waveMaxRadius = 50f;          // how far it travels before stopping

    [Header("Visual Kick")]
    public float scaleKick = 0.6f;             // how much cubes scale up at wave front
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Glow (optional)")]
    public bool driveGlow = true;
    public float glowKick = 8f;                // added emission strength at wave front
    public string glowProperty = "_EmissionStrength";

    // Internal state
    private bool active;
    private Vector3 origin;
    private float radius;

    // MPB for per-renderer overrides (no material instancing)
    private MaterialPropertyBlock mpb;
    private int glowId;

    public LayerMask groundMask; // set to Ground in Inspector


    private void Awake()
    {
        if (!cam) cam = Camera.main;
        mpb = new MaterialPropertyBlock();
        glowId = Shader.PropertyToID(glowProperty);
    }

    private void Update()
    {
        // Click to start wave (raycast into scene)
        if (Input.GetMouseButtonDown(0))
        {
            TryStartWaveFromMouse();
        }

        if (!active || createGrid == null || createGrid.grid == null) return;

        radius += waveSpeed * Time.deltaTime;

        // Stop after max radius
        if (radius > waveMaxRadius)
        {
            active = false;
            return;
        }

        // Apply wave to cubes
        ApplyWave(radius);
    }

    private void TryStartWaveFromMouse()
    {
        if (!cam) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // You can raycast against:
        // 1) A collider in your scene (floor plane recommended), or
        // 2) Your cubes if they have colliders.
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
        {
            origin = hit.point;
            radius = 0f;
            active = true;
        }
    }

    private void ApplyWave(float r)
    {
        float halfWidth = Mathf.Max(0.001f, waveWidth * 0.5f);

        for (int i = 0; i < createGrid.grid.Length; i++)
        {
            Transform t = createGrid.grid[i];
            if (!t || !t.gameObject.activeInHierarchy) continue;

            float d = Vector3.Distance(origin, t.position);

            // Ring: strongest when d is close to radius
            float band = 1f - Mathf.Clamp01(Mathf.Abs(d - r) / halfWidth);

            if (band <= 0f)
            {
                // Optional: if you drive glow, you may want to reset it when not affected.
                // We leave it untouched here to avoid fighting your existing animation.
                continue;
            }

            float k = falloff.Evaluate(band);

            // Scale kick: multiply current scale (plays nicely with your transformations)
            float s = 1f + (k * scaleKick);
            t.localScale = t.localScale * s;

            // Optional glow kick: add emission temporarily using MPB
            if (driveGlow)
            {
                var renderer = t.GetComponent<MeshRenderer>();
                if (renderer)
                {
                    renderer.GetPropertyBlock(mpb);

                    // Read current value if present; if not, assume 0
                    // Note: MaterialPropertyBlock doesn't provide direct getters for floats reliably,
                    // so we simply set a value each frame. Keep your shader's base emission in the material.
                    mpb.SetFloat(glowId, glowKick * k);

                    renderer.SetPropertyBlock(mpb);
                }
            }
        }
    }
}
