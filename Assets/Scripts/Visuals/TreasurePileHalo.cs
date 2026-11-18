using UnityEngine;
using UnityEngine.Rendering;

public class TreasurePileHalo : MonoBehaviour
{
    [SerializeField] private Material haloMaterial;

    [Header("Fit around the pile")]
    [SerializeField]
    private Vector3 sizeMultiplier =
        new Vector3(1.15f, 1.1f, 1.15f);

    [SerializeField]
    private Vector3 centreOffset =
        Vector3.zero;

    private GameObject halo;
    private Vector3 localCentre;
    private Vector3 localSize;

    private void Start()
    {
        if (haloMaterial == null)
        {
            Debug.LogWarning("Assign a halo material.", this);
            enabled = false;
            return;
        }

        MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();

        Bounds bounds = new Bounds();
        bool foundMesh = false;

        foreach (MeshFilter mesh in meshes)
        {
            if (mesh.sharedMesh == null)
                continue;

            Bounds meshBounds = mesh.sharedMesh.bounds;

            // Convert each mesh's bounds into this object's local space.
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = meshBounds.center + Vector3.Scale(
                    meshBounds.extents,
                    new Vector3(
                        (i & 1) == 0 ? -1 : 1,
                        (i & 2) == 0 ? -1 : 1,
                        (i & 4) == 0 ? -1 : 1
                    )
                );

                Vector3 point = transform.InverseTransformPoint(
                    mesh.transform.TransformPoint(corner)
                );

                if (!foundMesh)
                {
                    bounds = new Bounds(point, Vector3.zero);
                    foundMesh = true;
                }
                else
                {
                    bounds.Encapsulate(point);
                }
            }
        }

        if (!foundMesh)
        {
            enabled = false;
            return;
        }

        localCentre = bounds.center;
        localSize = bounds.size;

        halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        halo.name = "Treasure Halo";
        halo.layer = gameObject.layer;
        halo.transform.SetParent(transform, false);

        Collider haloCollider = halo.GetComponent<Collider>();
        haloCollider.enabled = false;
        Destroy(haloCollider);

        MeshRenderer renderer = halo.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = haloMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        UpdateHalo();
    }

    private void LateUpdate()
    {
        UpdateHalo();
    }

    private void UpdateHalo()
    {
        if (halo == null)
            return;

        halo.transform.localPosition = localCentre + centreOffset;
        halo.transform.localRotation = Quaternion.identity;
        halo.transform.localScale =
            Vector3.Scale(localSize, sizeMultiplier);
    }

    private void OnEnable()
    {
        if (halo != null)
            halo.SetActive(true);
    }

    private void OnDisable()
    {
        if (halo != null)
            halo.SetActive(false);
    }

    private void OnDestroy()
    {
        if (halo != null)
            Destroy(halo);
    }
}