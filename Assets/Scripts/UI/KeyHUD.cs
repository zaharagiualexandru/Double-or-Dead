using UnityEngine;

public class KeyHUD : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private RunManager runManager;

    [SerializeField] private GameObject keyIndicator;
    [SerializeField] private GameObject keyMessage;
    [SerializeField] private float messageDuration = 1.2f;

    private bool previouslyHadKey;
    private float messageTimeRemaining;

    private void Start()
    {
        keyIndicator.SetActive(false);
        keyMessage.SetActive(false);
    }

    private void LateUpdate()
    {
        bool hasKey = inventory.HasKey;

        if (hasKey && !previouslyHadKey)
            messageTimeRemaining = messageDuration;
        else
            messageTimeRemaining = Mathf.Max(
                0f,
                messageTimeRemaining - Time.deltaTime
            );

        bool showHUD = !runManager.HasEnded;

        keyIndicator.SetActive(showHUD && hasKey);
        keyMessage.SetActive(showHUD && messageTimeRemaining > 0f);

        previouslyHadKey = hasKey;
    }
}