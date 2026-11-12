using UnityEngine;

public class PickupIdleAnimation : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] private float floatHeight = 0.12f;
    [SerializeField] private float floatSpeed = 0.8f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 45f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float elapsedTime;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        float verticalOffset =
            Mathf.Sin(elapsedTime * floatSpeed * Mathf.PI * 2f)
            * floatHeight;

        transform.localPosition =
            startPosition + Vector3.up * verticalOffset;

        // Rotate around the parent's upright axis while preserving
        // the model's original orientation.
        transform.localRotation =
            Quaternion.AngleAxis(
                elapsedTime * rotationSpeed,
                Vector3.up
            ) * startRotation;
    }
}