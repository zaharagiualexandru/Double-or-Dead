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
        // Allows collection if already overlapping when the run starts.
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (collected)
            return;

        PlayerScore playerScore =
            other.GetComponentInParent<PlayerScore>();

        if (playerScore == null ||
            !playerScore.TryCollectTreasure(value))
        {
            return;
        }

        collected = true;

        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayTreasure();

        gameObject.SetActive(false);
    }
}