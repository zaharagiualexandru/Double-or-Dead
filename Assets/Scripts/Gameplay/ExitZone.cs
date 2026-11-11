using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ExitZone : MonoBehaviour
{
    [SerializeField] private RunManager runManager;

    private void Awake()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        zone.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryExit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryExit(other);
    }

    private void TryExit(Collider other)
    {
        if (runManager == null)
            return;

        PlayerScore playerScore =
            other.GetComponentInParent<PlayerScore>();

        if (playerScore != null)
            runManager.TryEscape(playerScore);
    }
}