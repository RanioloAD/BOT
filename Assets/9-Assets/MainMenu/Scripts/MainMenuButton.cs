using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace B10.MainMenu
{
    public enum MainMenuAction
    {
        Continue,
        NewGame,
        Options,
        Exit
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Action")]
        [SerializeField] private MainMenuAction action;
        [SerializeField] private MainMenuController controller;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;

        [Header("Hover")]
        [SerializeField, Min(0.01f)] private float hoverDuration = 0.22f;
        [SerializeField] private float hoverOffset = 14f;
        [SerializeField, Min(1f)] private float hoverScale = 1.025f;
        [SerializeField] private Color normalColor = new Color(0.72f, 0.77f, 0.8f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.14f, 0.9f, 1f, 1f);
        [SerializeField] private string hoverPrefix = "> ";

        [Header("Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;
        [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float clickVolume = 0.65f;

        private RectTransform rectTransform;
        private Vector2 normalPosition;
        private Vector3 normalScale;
        private string normalLabel;
        private bool pointerInside;
        private bool selected;
        private bool visuallyHovered;
        private Coroutine hoverRoutine;

        public MainMenuAction Action => action;
        public bool IsInteractable => button != null && button.interactable;

        private void Awake()
        {
            button ??= GetComponent<Button>();
            label ??= GetComponentInChildren<TMP_Text>(true);
            rectTransform = transform as RectTransform;
            normalPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
            normalScale = transform.localScale;
            normalLabel = label != null ? label.text.TrimStart('>', ' ') : string.Empty;
        }

        private void OnEnable()
        {
            button ??= GetComponent<Button>();
            button.onClick.AddListener(Activate);
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(Activate);
            }
        }

        public void SetInteractable(bool value, float disabledAlpha = 0.3f)
        {
            button ??= GetComponent<Button>();
            button.interactable = value;

            if (!value)
            {
                pointerInside = false;
                selected = false;
                SetHover(false, false);
                if (label != null)
                {
                    Color disabledColor = normalColor;
                    disabledColor.a = disabledAlpha;
                    label.color = disabledColor;
                }
            }
            else if (!visuallyHovered && label != null)
            {
                label.color = normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            SetHover(true, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            SetHover(selected, false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            SetHover(true, !pointerInside);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            SetHover(pointerInside, false);
        }

        private void Activate()
        {
            if (!IsInteractable || controller == null)
            {
                return;
            }

            PlayClip(clickClip, clickVolume);
            controller.HandleAction(action);
        }

        private void SetHover(bool hover, bool playSound)
        {
            if (!IsInteractable)
            {
                hover = false;
            }

            if (visuallyHovered == hover)
            {
                return;
            }

            visuallyHovered = hover;
            if (playSound && hover)
            {
                PlayClip(hoverClip, hoverVolume);
            }

            if (hoverRoutine != null)
            {
                StopCoroutine(hoverRoutine);
            }

            hoverRoutine = StartCoroutine(AnimateHover(hover));
        }

        private IEnumerator AnimateHover(bool hover)
        {
            Vector2 startPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
            Vector2 targetPosition = normalPosition + (hover ? Vector2.right * hoverOffset : Vector2.zero);
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = normalScale * (hover ? hoverScale : 1f);
            Color startColor = label != null ? label.color : Color.white;
            Color targetColor = hover ? hoverColor : normalColor;
            float elapsed = 0f;

            if (label != null)
            {
                label.text = hover ? hoverPrefix + normalLabel : normalLabel;
            }

            while (elapsed < hoverDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / hoverDuration));
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
                }

                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
                if (label != null)
                {
                    label.color = Color.LerpUnclamped(startColor, targetColor, t);
                }

                yield return null;
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = targetPosition;
            }

            transform.localScale = targetScale;
            if (label != null)
            {
                label.color = targetColor;
            }

            hoverRoutine = null;
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (uiAudioSource != null && clip != null)
            {
                uiAudioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
