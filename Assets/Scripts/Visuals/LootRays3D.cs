using UnityEngine;
using UnityEngine.Rendering;

public class LootRays3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetVisual;
    [SerializeField] private Material rayMaterial;

    [Header("Rays")]
    [SerializeField, Range(3, 24)] private int rayCount = 10;
    [SerializeField] private float minimumLength = 0.45f;
    [SerializeField] private float maximumLength = 1.8f;
    [SerializeField] private float maximumWidth = 0.22f;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 0.7f;
    [SerializeField]
    private Vector3 rotationSpeed =
        new Vector3(12f, 30f, 8f);

    private Transform[] rays;
    private Quaternion[] startingRotations;
    private Mesh rayMesh;
    private float elapsedTime;

    private void Start()
    {
        if (rayMaterial == null)
        {
            Debug.LogWarning("Assign a ray material.", this);
            enabled = false;
            return;
        }

        rayMesh = CreateRayMesh();

        rays = new Transform[rayCount];
        startingRotations = new Quaternion[rayCount];

        // Distributes ray directions around a sphere.
        float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < rayCount; i++)
        {
            GameObject ray = new GameObject($"Ray_{i + 1}");
            ray.layer = gameObject.layer;
            ray.transform.SetParent(transform, false);

            MeshFilter filter = ray.AddComponent<MeshFilter>();
            filter.sharedMesh = rayMesh;

            MeshRenderer renderer = ray.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = rayMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            float y = 1f - 2f * (i + 0.5f) / rayCount;
            float radius = Mathf.Sqrt(1f - y * y);
            float angle = goldenAngle * i;

            Vector3 direction = new Vector3(
                Mathf.Cos(angle) * radius,
                y,
                Mathf.Sin(angle) * radius
            );

            rays[i] = ray.transform;
            startingRotations[i] =
                Quaternion.FromToRotation(Vector3.forward, direction);
        }

        AnimateRays();
    }

    private void LateUpdate()
    {
        elapsedTime += Time.deltaTime;

        if (targetVisual != null)
            transform.position = targetVisual.position;

        AnimateRays();
    }

    private void AnimateRays()
    {
        Quaternion orbit = Quaternion.Euler(rotationSpeed * elapsedTime);

        for (int i = 0; i < rays.Length; i++)
        {
            // Different phases and speeds prevent synchronised pulsing.
            float phase = i * 2.39996f;
            float speedVariation = 1f + (i % 4) * 0.13f;

            float pulse = 0.5f + 0.5f * Mathf.Sin(
                elapsedTime * pulseSpeed * speedVariation
                * Mathf.PI * 2f + phase
            );

            float length =
                Mathf.Lerp(minimumLength, maximumLength, pulse);

            float width =
                maximumWidth * Mathf.Lerp(0.35f, 1f, pulse);

            rays[i].localRotation = orbit * startingRotations[i];
            rays[i].localScale = new Vector3(width, width, length);
        }
    }

    private Mesh CreateRayMesh()
    {
        const int sides = 6;

        Vector3[] vertices = new Vector3[sides * 3];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[vertices.Length];

        for (int i = 0; i < sides; i++)
        {
            float angleA = i * Mathf.PI * 2f / sides;
            float angleB = (i + 1) * Mathf.PI * 2f / sides;

            int index = i * 3;

            // Narrow origin, widening towards the outer end.
            vertices[index] = Vector3.zero;

            vertices[index + 1] = new Vector3(
                Mathf.Cos(angleA),
                Mathf.Sin(angleA),
                1f
            );

            vertices[index + 2] = new Vector3(
                Mathf.Cos(angleB),
                Mathf.Sin(angleB),
                1f
            );

            uvs[index] = new Vector2(0.5f, 0f);
            uvs[index + 1] = new Vector2(0f, 1f);
            uvs[index + 2] = new Vector2(1f, 1f);

            triangles[index] = index;
            triangles[index + 1] = index + 1;
            triangles[index + 2] = index + 2;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Loot Ray Cone";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        return mesh;
    }

    private void OnDestroy()
    {
        if (rayMesh != null)
            Destroy(rayMesh);
    }
}