using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace B10.MainMenu
{
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Scene Connection")]
        [SerializeField, Tooltip("Exact gameplay scene included in Build Settings.")]
        private string gameSceneName = "DemoLevel";
        [SerializeField, Tooltip("Enable only after a compatible save system is connected.")]
        private bool continueAvailable;

        [Header("Scene References")]
        [SerializeField] private MainMenuCameraController cameraController;
        [SerializeField] private MainMenuOptionsPanel optionsPanel;
        [SerializeField] private CanvasGroup mainContentGroup;
        [SerializeField] private MainMenuButton continueButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField, Min(0.1f)] private float contentFadeDuration = 0.22f;

        private bool actionLocked;
        private Coroutine contentRoutine;
        private Coroutine statusRoutine;

        public string GameSceneName
        {
            get => gameSceneName;
            set => gameSceneName = value;
        }

        private void Start()
        {
            continueButton?.SetInteractable(continueAvailable);
            SetStatus(string.Empty);
            cameraController?.PlayIntro();
        }

        private void Update()
        {
            if (!actionLocked && optionsPanel != null && optionsPanel.IsVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                optionsPanel.Hide();
                CloseOptions();
            }
        }

        public void HandleAction(MainMenuAction action)
        {
            if (actionLocked || (cameraController != null && cameraController.IsTransitioning))
            {
                return;
            }

            switch (action)
            {
                case MainMenuAction.Continue:
                    if (!continueAvailable)
                    {
                        ShowStatus("SIN PARTIDA GUARDADA");
                        return;
                    }

                    TryBeginSceneTransition();
                    break;
                case MainMenuAction.NewGame:
                    TryBeginSceneTransition();
                    break;
                case MainMenuAction.Options:
                    OpenOptions();
                    break;
                case MainMenuAction.Exit:
                    BeginTransition(QuitApplication);
                    break;
            }
        }

        public void CloseOptions()
        {
            if (contentRoutine != null)
            {
                StopCoroutine(contentRoutine);
            }

            contentRoutine = StartCoroutine(FadeContent(true));
        }

        private void OpenOptions()
        {
            if (contentRoutine != null)
            {
                StopCoroutine(contentRoutine);
            }

            contentRoutine = StartCoroutine(FadeContent(false));
            optionsPanel?.Show();
        }

        private void TryBeginSceneTransition()
        {
            if (string.IsNullOrWhiteSpace(gameSceneName))
            {
                ShowStatus("CONFIGURA GAME SCENE NAME EN EL INSPECTOR");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
            {
                ShowStatus($"ESCENA NO DISPONIBLE: {gameSceneName}");
                return;
            }

            BeginTransition(() => SceneManager.LoadScene(gameSceneName));
        }

        private void BeginTransition(System.Action onComplete)
        {
            actionLocked = true;
            if (cameraController == null || !cameraController.PlayTransition(onComplete))
            {
                actionLocked = false;
            }
        }

        private IEnumerator FadeContent(bool show)
        {
            if (mainContentGroup == null)
            {
                yield break;
            }

            float start = mainContentGroup.alpha;
            float target = show ? 1f : 0f;
            mainContentGroup.interactable = show;
            mainContentGroup.blocksRaycasts = show;
            float elapsed = 0f;
            while (elapsed < contentFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                mainContentGroup.alpha = Mathf.Lerp(start, target,
                    Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / contentFadeDuration)));
                yield return null;
            }

            mainContentGroup.alpha = target;
            contentRoutine = null;
        }

        private void ShowStatus(string message)
        {
            if (statusRoutine != null)
            {
                StopCoroutine(statusRoutine);
            }

            statusRoutine = StartCoroutine(StatusRoutine(message));
        }

        private IEnumerator StatusRoutine(string message)
        {
            SetStatus(message);
            yield return new WaitForSecondsRealtime(3f);
            SetStatus(string.Empty);
            statusRoutine = null;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
