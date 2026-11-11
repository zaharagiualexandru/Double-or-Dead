using UnityEngine;

public class CrownHUD : MonoBehaviour
{
    [SerializeField] private PlayerScore playerScore;
    [SerializeField] private RunManager runManager;
    [SerializeField] private GameObject crownMessage;
    [SerializeField] private float messageDuration = 1.5f;

    private bool previouslyCollected;
    private float messageTimeRemaining;

    private void Start()
    {
        crownMessage.SetActive(false);
    }

    private void LateUpdate()
    {
        bool hasCrown = playerScore.HasCollectedCrown;

        if (hasCrown && !previouslyCollected)
        {
            messageTimeRemaining = messageDuration;
        }
        else
        {
            messageTimeRemaining = Mathf.Max(
                0f,
                messageTimeRemaining - Time.deltaTime
            );
        }

        crownMessage.SetActive(
            !runManager.HasEnded && messageTimeRemaining > 0f
        );

        previouslyCollected = hasCrown;
    }
}