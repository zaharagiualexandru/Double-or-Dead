using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TreasurePickup : MonoBehaviour
{
    [Header("Treasure")]
    [SerializeField, Min(1)] private int value = 5;

    private bool collected;

    private void Awake()
    {
        BoxCollider pickupCollider = GetComponent<BoxCollider>();
        pickupCollider.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Allows collection if the player was already overlapping
        // when the run became active.
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (collected)
            return;

        PlayerScore playerScore =
            other.GetComponentInParent<PlayerScore>();

        if (playerScore == null)
            return;

        if (!playerScore.TryCollectTreasure(value))
            return;

        collected = true;
        gameObject.SetActive(false);
    }
}