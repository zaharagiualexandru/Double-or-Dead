using System.Globalization;
using TMPro;
using UnityEngine;

public class LeaderboardRowUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;

    [Header("Rank Colours")]
    [SerializeField]
    private Color firstPlace =
        new Color32(255, 208, 90, 255);

    [SerializeField]
    private Color secondPlace =
        new Color32(210, 215, 225, 255);

    [SerializeField]
    private Color thirdPlace =
        new Color32(205, 140, 85, 255);

    [SerializeField] private Color otherPlaces = Color.white;

    [Header("First Place Rainbow")]
    [SerializeField] private bool rainbowEnabled = true;
    [SerializeField, Range(0.05f, 1f)] private float rainbowSpeed = 0.2f;
    [SerializeField, Range(0.2f, 3f)] private float rainbowSpread = 1f;
    [SerializeField, Range(0f, 1f)] private float saturation = 0.7f;

    private bool isFirstPlace;
    private RectTransform rowRect;

    private void Awake()
    {
        rowRect = (RectTransform)transform;
    }

    public void SetEntry(
        int rank,
        string playerName,
        long score,
        int escapeTimeMilliseconds,
        bool isDeveloper = false)
    {
        isFirstPlace = rank == 1;

        rankText.text = $"#{rank}";

        playerNameText.richText = false;
        playerNameText.text = playerName ?? "";

        if (isDeveloper)
            playerNameText.text += " (dev)";

        scoreText.text = score.ToString(
            "N0", CultureInfo.InvariantCulture);

        timeText.text = (escapeTimeMilliseconds / 1000d)
            .ToString("F3", CultureInfo.InvariantCulture);

        Color colour = rank switch
        {
            1 => rainbowEnabled ? Color.white : firstPlace,
            2 => secondPlace,
            3 => thirdPlace,
            _ => otherPlaces
        };

        SetTextColour(rankText, colour);
        SetTextColour(playerNameText, colour);
        SetTextColour(scoreText, colour);
        SetTextColour(timeText, colour);
    }

    private void SetTextColour(TMP_Text text, Color colour)
    {
        text.enableVertexGradient = false;
        text.color = colour;

        // Clear any previous rainbow colours if the rank changes.
        text.SetVerticesDirty();
    }

    private void LateUpdate()
    {
        if (!isFirstPlace || !rainbowEnabled)
            return;

        ApplyRainbow(rankText);
        ApplyRainbow(playerNameText);
        ApplyRainbow(scoreText);
        ApplyRainbow(timeText);
    }

    private void ApplyRainbow(TMP_Text text)
    {
        if (text == null || !text.isActiveAndEnabled)
            return;

        text.ForceMeshUpdate();

        TMP_TextInfo info = text.textInfo;
        float width = Mathf.Max(1f, rowRect.rect.width);
        float phase = Time.unscaledTime * rainbowSpeed;

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];

            if (!character.isVisible)
                continue;

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;

            Vector3[] vertices = info.meshInfo[materialIndex].vertices;
            Color32[] colours = info.meshInfo[materialIndex].colors32;

            for (int corner = 0; corner < 4; corner++)
            {
                int index = vertexIndex + corner;

                // Measure position across the whole row so all columns
                // share one continuous moving rainbow.
                Vector3 worldPosition =
                    text.transform.TransformPoint(vertices[index]);

                float rowX =
                    rowRect.InverseTransformPoint(worldPosition).x;

                float position =
                    (rowX - rowRect.rect.xMin) / width;

                float hue = Mathf.Repeat(
                    position * rainbowSpread - phase, 1f);

                Color32 colour =
                    Color.HSVToRGB(hue, saturation, 1f);

                colour.a = colours[index].a;
                colours[index] = colour;
            }
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    [ContextMenu("Preview First Place")]
    private void PreviewFirstPlace()
    {
        SetEntry(1, "ziou", 1440500, 18574, true);
    }

    [ContextMenu("Preview Second Place")]
    private void PreviewSecondPlace()
    {
        SetEntry(2, "Player02", 925000, 17250);
    }

    [ContextMenu("Preview Third Place")]
    private void PreviewThirdPlace()
    {
        SetEntry(3, "Player03", 750000, 19120);
    }
}