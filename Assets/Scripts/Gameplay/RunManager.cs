using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    [SerializeField] private FirstPersonController player;
    [SerializeField] private float runDuration = 20f;

    private double endTime;
    private bool restarting;

    public float TimeRemaining { get; private set; }
    public bool HasStarted { get; private set; }
    public bool IsDead { get; private set; }
    public bool CanCollectTreasure =>
    HasStarted &&
    !IsDead &&
    !restarting &&
    Time.timeAsDouble < endTime;

    private void Awake()
    {
        TimeRemaining = runDuration;
    }

    private void Update()
    {
        // This stays active even after the player controller is disabled.
        if (!restarting &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            restarting = true;
            Time.timeScale = 1f;

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    private void LateUpdate()
    {
        if (restarting || IsDead || player == null)
            return;

        // The controller records movement and jumping in Update.
        if (!HasStarted)
        {
            if (!player.HasStartedMoving)
                return;

            HasStarted = true;
            endTime = Time.timeAsDouble + runDuration;
        }

        TimeRemaining = Mathf.Max(
            0f,
            (float)(endTime - Time.timeAsDouble)
        );

        if (TimeRemaining <= 0f)
            Die();
    }

    private void Die()
    {
        TimeRemaining = 0f;
        IsDead = true;

        // Stops movement, jumping and mouse look.
        // Its OnDisable method also releases the cursor.
        player.enabled = false;
    }
}