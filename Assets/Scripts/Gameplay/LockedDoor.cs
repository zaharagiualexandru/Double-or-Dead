using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class LockedDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunManager runManager;
    [SerializeField] private GameObject doorBlocker;
    [SerializeField] private GameObject keyRequiredPrompt;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        ShowPrompt(false);
    }

    private void Update()
    {
        if (runManager.HasEnded)
            ShowPrompt(false);
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
        if (IsOpen || !runManager.CanCollectTreasure)
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

        IsOpen = true;

        // Disables both the visible door and its blocking collider.
        doorBlocker.SetActive(false);

        ShowPrompt(false);
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