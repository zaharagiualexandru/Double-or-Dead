using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;

public class PlayerNameSetup : MonoBehaviour
{
    private const string NameKey = "LeaderboardPlayerName";
    private const string RegisteredNameKey = "LeaderboardRegisteredName";
    private const string RegisteredPlayerKey = "LeaderboardRegisteredPlayer";

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Button confirmButton;

    private bool initialized;

    public static bool IsSaving { get; private set; }

    public static string SavedName =>
        PlayerPrefs.GetString(NameKey, "");

    // Old names saved only locally must be confirmed online once.
    public static bool HasValidName
    {
        get
        {
            if (IsSaving || !IsValidName(SavedName))
                return false;

            if (PlayerPrefs.GetString(RegisteredNameKey, "") != SavedName)
                return false;

            try
            {
                return AuthenticationService.Instance.IsSignedIn &&
                    PlayerPrefs.GetString(RegisteredPlayerKey, "") ==
                    AuthenticationService.Instance.PlayerId;
            }
            catch
            {
                return false;
            }
        }
    }

    [Serializable]
    public class RegisterNameResponse
    {
        public bool success;
        public string code;
        public string message;
        public string playerName;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsSaving = false;
    }

    private void Awake()
    {
        InitializeFields();
    }

    private void InitializeFields()
    {
        if (initialized)
            return;

        if (nameInput != null)
        {
            nameInput.contentType =
                TMP_InputField.ContentType.Alphanumeric;
            nameInput.characterLimit = 8;
            nameInput.lineType = TMP_InputField.LineType.SingleLine;
            nameInput.SetTextWithoutNotify(SavedName);
        }

        if (errorText != null)
            errorText.text = "";

        initialized = true;
    }

    public void ShowSavedName()
    {
        InitializeFields();

        if (IsSaving)
            return;

        if (nameInput != null)
            nameInput.SetTextWithoutNotify(SavedName);

        if (errorText != null)
            errorText.text = "";
    }

    public void ShowNameRequired()
    {
        InitializeFields();

        if (IsSaving)
        {
            SetMessage(
                "Please wait while your name is saved.",
                new Color32(255, 208, 90, 255));
            return;
        }

        if (nameInput != null)
            nameInput.SetTextWithoutNotify(SavedName);

        SetMessage(
            "Confirm your name online before playing.",
            new Color32(255, 208, 90, 255));
    }

    public async void ConfirmName()
    {
        InitializeFields();

        if (IsSaving || nameInput == null)
            return;

        string chosenName = nameInput.text.Trim();

        if (!IsValidName(chosenName))
        {
            SetMessage(
                "Use 1–8 letters or numbers.",
                new Color32(255, 100, 100, 255));
            return;
        }

        IsSaving = true;
        SetControlsEnabled(false);

        SetMessage(
            "Checking name...",
            new Color32(255, 208, 90, 255));

        try
        {
            OnlineLeaderboard manager = OnlineLeaderboard.Instance;

            if (manager == null)
                throw new InvalidOperationException(
                    "OnlineLeaderboard is missing from the scene.");

            await manager.EnsureReadyAsync();

            string playerId = AuthenticationService.Instance.PlayerId;

            var arguments = new Dictionary<string, object>
            {
                { "requestedName", chosenName }
            };

            RegisterNameResponse response =
                await CloudCodeService.Instance
                    .CallEndpointAsync<RegisterNameResponse>(
                        "RegisterPlayerName",
                        arguments);

            // Never save the result against a different signed-in account.
            if (!AuthenticationService.Instance.IsSignedIn ||
                AuthenticationService.Instance.PlayerId != playerId)
            {
                throw new InvalidOperationException(
                    "The signed-in player changed. Please try again.");
            }

            if (response == null)
                throw new InvalidOperationException(
                    "Name registration returned an empty response.");

            if (!response.success)
            {
                if (this != null)
                {
                    SetMessage(
                        string.IsNullOrWhiteSpace(response.message)
                            ? "Couldn't save that name. Please try again."
                            : response.message,
                        new Color32(255, 100, 100, 255));
                }

                return;
            }

            if (!IsValidName(response.playerName) ||
                response.playerName != chosenName)
            {
                throw new InvalidOperationException(
                    "Name registration returned an unexpected name.");
            }

            // Save only after the server accepts the registration.
            PlayerPrefs.SetString(NameKey, response.playerName);
            PlayerPrefs.SetString(RegisteredNameKey, response.playerName);
            PlayerPrefs.SetString(RegisteredPlayerKey, playerId);
            PlayerPrefs.Save();

            if (this != null)
            {
                if (nameInput != null)
                    nameInput.SetTextWithoutNotify(response.playerName);

                SetMessage(
                    "Name saved! Go Back to play.",
                    new Color32(130, 230, 140, 255));
            }
        }
        catch (Exception exception)
        {
            if (this != null)
            {
                SetMessage(
                    "Couldn't save name online. Please try again.",
                    new Color32(255, 100, 100, 255));
            }

            Debug.LogWarning(
                $"Name registration failed: {exception.Message}");
        }
        finally
        {
            IsSaving = false;

            if (this != null)
                SetControlsEnabled(true);
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (nameInput != null)
            nameInput.interactable = enabled;

        if (confirmButton != null)
            confirmButton.interactable = enabled;
    }

    private void SetMessage(string message, Color colour)
    {
        if (errorText == null)
            return;

        errorText.color = colour;
        errorText.text = message;
    }

    public static bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 8)
            return false;

        foreach (char character in name)
        {
            bool isLetter =
                (character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z');

            bool isNumber =
                character >= '0' && character <= '9';

            if (!isLetter && !isNumber)
                return false;
        }

        return true;
    }

#if UNITY_EDITOR
    [ContextMenu("Clear Local Name For Testing")]
    private void ClearLocalNameForTesting()
    {
        if (IsSaving)
            return;

        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.DeleteKey(RegisteredNameKey);
        PlayerPrefs.DeleteKey(RegisteredPlayerKey);
        PlayerPrefs.Save();

        if (nameInput != null)
            nameInput.SetTextWithoutNotify("");

        if (errorText != null)
            errorText.text = "";

        Debug.Log(
            "Local name cleared. The online name reservation, " +
            "scores and sign-in are unchanged.");
    }
#endif
}