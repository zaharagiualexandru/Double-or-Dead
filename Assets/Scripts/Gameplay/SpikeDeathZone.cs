using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpikeDeathZone : MonoBehaviour
{
    private RunManager runManager;

    private void Awake()
    {
        runManager = FindFirstObjectByType<RunManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<FirstPersonController>() == null)
            return;

        if (runManager != null && !runManager.HasEnded)
            runManager.Die();
    }
}  