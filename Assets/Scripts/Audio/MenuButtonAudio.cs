using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonAudio : MonoBehaviour, IPointerEnterHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlayClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.IsActive() || !button.IsInteractable())
            return;

        if (MenuAudio.Instance != null)
            MenuAudio.Instance.PlayHover();
    }

    private void PlayClick()
    {
        if (MenuAudio.Instance != null)
            MenuAudio.Instance.PlayClick();
    }
}