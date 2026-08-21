using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace B10.MainMenu
{
    [DisallowMultipleComponent]
    public sealed class MainMenuOptionsPanel : MonoBehaviour
    {
        private const string MasterKey = "B10.MainMenu.MasterVolume";
        private const string MusicKey = "B10.MainMenu.MusicVolume";
        private const string SfxKey = "B10.MainMenu.SfxVolume";

        [Header("Presentation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panel;
        [SerializeField, Min(0.05f)] private float transitionDuration = 0.22f;
        [SerializeField] private float slideDistance = 18f;

        [Header("Controls")]
        [SerializeField] private Slider masterVolume;
        [SerializeField] private Slider musicVolume;
        [SerializeField] private Slider sfxVolume;
        [SerializeField] private TMP_Text masterValue;
        [SerializeField] private TMP_Text musicValue;
        [SerializeField] private TMP_Text sfxValue;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;

        [Header("Scene Audio")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private MainMenuController controller;

        private readonly List<Resolution> availableResolutions = new List<Resolution>();
        private Vector2 visiblePosition;
        private Coroutine transitionRoutine;

        public bool IsVisible => canvasGroup != null && canvasGroup.interactable;

        private void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            panel ??= transform as RectTransform;
            visiblePosition = panel != null ? panel.anchoredPosition : Vector2.zero;
            PopulateResolutions();

            masterVolume.SetValueWithoutNotify(PlayerPrefs.GetFloat(MasterKey, 0.85f));
            musicVolume.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicKey, 0.35f));
            sfxVolume.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxKey, 0.65f));
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);

            masterVolume.onValueChanged.AddListener(SetMasterVolume);
            musicVolume.onValueChanged.AddListener(SetMusicVolume);
            sfxVolume.onValueChanged.AddListener(SetSfxVolume);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            backButton.onClick.AddListener(Back);

            ApplyAudioValues();
            HideImmediate();
        }

        private void OnDestroy()
        {
            masterVolume.onValueChanged.RemoveListener(SetMasterVolume);
            musicVolume.onValueChanged.RemoveListener(SetMusicVolume);
            sfxVolume.onValueChanged.RemoveListener(SetSfxVolume);
            resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
            backButton.onClick.RemoveListener(Back);
            PlayerPrefs.Save();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            StartTransition(true);
            if (masterVolume != null)
            {
                masterVolume.Select();
            }
        }

        public void Hide()
        {
            StartTransition(false);
        }

        public void HideImmediate()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (panel != null)
            {
                panel.anchoredPosition = visiblePosition + Vector2.left * slideDistance;
            }
        }

        private void Back()
        {
            Hide();
            controller?.CloseOptions();
        }

        private void StartTransition(bool show)
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(Transition(show));
        }

        private IEnumerator Transition(bool show)
        {
            float startAlpha = canvasGroup.alpha;
            float targetAlpha = show ? 1f : 0f;
            Vector2 startPosition = panel.anchoredPosition;
            Vector2 targetPosition = show ? visiblePosition : visiblePosition + Vector2.left * slideDistance;
            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                panel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            panel.anchoredPosition = targetPosition;
            transitionRoutine = null;
        }

        private void PopulateResolutions()
        {
            availableResolutions.Clear();
            availableResolutions.AddRange(Screen.resolutions
                .GroupBy(resolution => new { resolution.width, resolution.height })
                .Select(group => group.Last())
                .OrderBy(resolution => resolution.width)
                .ThenBy(resolution => resolution.height));

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(availableResolutions
                .Select(resolution => $"{resolution.width} × {resolution.height}")
                .ToList());

            int currentIndex = availableResolutions.FindIndex(resolution =>
                resolution.width == Screen.width && resolution.height == Screen.height);
            resolutionDropdown.SetValueWithoutNotify(Mathf.Max(0, currentIndex));
            resolutionDropdown.RefreshShownValue();
        }

        private void ApplyAudioValues()
        {
            SetMasterVolume(masterVolume.value);
            SetMusicVolume(musicVolume.value);
            SetSfxVolume(sfxVolume.value);
        }

        private void SetMasterVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(MasterKey, value);
            SetPercent(masterValue, value);
        }

        private void SetMusicVolume(float value)
        {
            if (musicSource != null)
            {
                musicSource.volume = value;
            }

            PlayerPrefs.SetFloat(MusicKey, value);
            SetPercent(musicValue, value);
        }

        private void SetSfxVolume(float value)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = value;
            }

            PlayerPrefs.SetFloat(SfxKey, value);
            SetPercent(sfxValue, value);
        }

        private void SetResolution(int index)
        {
            if (index < 0 || index >= availableResolutions.Count)
            {
                return;
            }

            Resolution resolution = availableResolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
        }

        private static void SetFullscreen(bool value)
        {
            Screen.fullScreen = value;
        }

        private static void SetPercent(TMP_Text label, float value)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(value * 100f):00}";
            }
        }
    }
}
