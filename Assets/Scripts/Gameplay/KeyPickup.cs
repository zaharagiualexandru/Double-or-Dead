using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class KeyPickup : MonoBehaviour
{
    private bool collected;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

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
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (collected)
            return;

        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null || !inventory.TryCollectKey())
            return;

        collected = true;

        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayKey();

        gameObject.SetActive(false);
    }
}