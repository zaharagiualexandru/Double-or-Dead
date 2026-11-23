using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class LockedDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunManager runManager;
    [SerializeField] private Transform gateVisual;
    [SerializeField] private GameObject doorBlocker;
    [SerializeField] private GameObject keyRequiredPrompt;

    [Header("Gate Movement")]
    [SerializeField, Min(0f)] private float liftHeight = 2.7f;
    [SerializeField, Min(0.01f)] private float openingDuration = 0.3f;

    public bool IsOpen { get; private set; }

    private Vector3 closedPosition;
    private float openingElapsed;
    private bool isOpening;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        if (gateVisual != null)
            closedPosition = gateVisual.localPosition;

        ShowPrompt(false);

        if (runManager == null ||
            gateVisual == null ||
            doorBlocker == null)
        {
            Debug.LogError(
                "Assign Run Manager, Gate Visual and Door Blocker " +
                "on LockedDoor.",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        if (runManager.HasEnded)
            ShowPrompt(false);

        if (!isOpening)
            return;

        openingElapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(
            openingElapsed / Mathf.Max(0.01f, openingDuration));

        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

        gateVisual.localPosition = Vector3.Lerp(
            closedPosition,
            closedPosition + Vector3.up * liftHeight,
            easedProgress);

        if (progress >= 1f)
        {
            isOpening = false;
            IsOpen = true;

            // Keep the gate object active; remove only the blocker.
            doorBlocker.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryOpen(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryOpen(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerInventory>() != null)
            ShowPrompt(false);
    }

    private void TryOpen(Collider other)
    {
        if (!isActiveAndEnabled ||
            runManager == null ||
            IsOpen ||
            isOpening ||
            !runManager.CanCollectTreasure)
        {
            return;
        }

        if (gateVisual == null || doorBlocker == null)
            return;

        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null)
            return;

        if (!inventory.HasKey)
        {
            ShowPrompt(true);
            return;
        }

        if (!inventory.TryUseKey())
            return;

        isOpening = true;
        openingElapsed = 0f;

        ShowPrompt(false);

        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayDoor();
    }

    private void ShowPrompt(bool visible)
    {
        if (keyRequiredPrompt != null)
            keyRequiredPrompt.SetActive(visible);
    }

    private void OnDisable()
    {
        ShowPrompt(false);
    }
}