using UnityEngine;

[DefaultExecutionOrder(200)]
public class LootGlowBillboard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetVisual;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Camera targetCamera;

    [Header("Position")]
    [SerializeField, Min(0f)] private float behindOffset = 0.15f;

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null || targetVisual == null)
            return;

        // Use the mesh centre, which may differ from its pivot.
        Vector3 centre = targetRenderer != null
            ? targetRenderer.bounds.center
            : targetVisual.position;

        Vector3 cameraToCoin =
            centre - targetCamera.transform.position;

        if (cameraToCoin.sqrMagnitude < 0.0001f)
            return;

        Vector3 direction = cameraToCoin.normalized;

        // Moving along this line keeps the aura centred
        // on the coin from the player's perspective.
        transform.position = centre + direction * behindOffset;

        transform.rotation = Quaternion.LookRotation(
            direction,
            targetCamera.transform.up
        );
    }
}