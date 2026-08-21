using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace B10.MainMenu
{
    [DisallowMultipleComponent]
    public sealed class MainMenuCameraController : MonoBehaviour
    {
        [Header("Cinemachine")]
        [SerializeField] private Camera controlledCamera;
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineVirtualCamera establishingCamera;
        [SerializeField] private CinemachineVirtualCamera eyeCloseupCamera;
        [SerializeField] private Transform eyeTarget;

        [Header("Interface")]
        [SerializeField] private CanvasGroup interfaceGroup;
        [SerializeField] private CanvasGroup cinematicFade;
        [SerializeField] private Image cinematicFadeImage;
        [SerializeField] private Color transitionFadeColor = new Color(0.005f, 0.04f, 0.06f, 1f);

        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float introDuration = 1.8f;
        [SerializeField, Min(0.2f)] private float transitionDuration = 2.4f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Robot Eye")]
        [SerializeField] private Light eyeLight;
        [SerializeField, Min(0f)] private float eyeRestingIntensity = 120f;
        [SerializeField, Min(0f)] private float eyeTransitionIntensity = 700f;

        private bool transitioning;
        private Coroutine activeRoutine;

        public bool IsTransitioning => transitioning;
        public float TransitionDuration => transitionDuration;

        private void Awake()
        {
            if (cinemachineBrain != null)
            {
                cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(
                    CinemachineBlendDefinition.Style.EaseInOut,
                    transitionDuration);
            }

            if (establishingCamera != null)
            {
                establishingCamera.Priority = 20;
            }

            if (eyeCloseupCamera != null)
            {
                eyeCloseupCamera.Priority = 10;
            }

            if (eyeLight != null)
            {
                eyeLight.intensity = eyeRestingIntensity;
            }

            if (cinematicFadeImage != null)
            {
                cinematicFadeImage.color = Color.black;
            }
        }

        public void PlayIntro()
        {
            if (activeRoutine != null || cinematicFade == null)
            {
                return;
            }

            activeRoutine = StartCoroutine(IntroRoutine());
        }

        public bool PlayTransition(Action onComplete)
        {
            if (transitioning)
            {
                return false;
            }

            activeRoutine = StartCoroutine(TransitionRoutine(onComplete));
            return true;
        }

        private IEnumerator IntroRoutine()
        {
            cinematicFade.alpha = 1f;
            cinematicFade.blocksRaycasts = true;
            if (interfaceGroup != null)
            {
                interfaceGroup.alpha = 0f;
                interfaceGroup.interactable = false;
                interfaceGroup.blocksRaycasts = false;
            }

            yield return new WaitForSecondsRealtime(0.15f);
            float elapsed = 0f;
            while (elapsed < introDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / introDuration);
                cinematicFade.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
                if (interfaceGroup != null)
                {
                    float uiT = Mathf.InverseLerp(0.28f, 1f, t);
                    interfaceGroup.alpha = Mathf.SmoothStep(0f, 1f, uiT);
                }

                yield return null;
            }

            cinematicFade.alpha = 0f;
            cinematicFade.blocksRaycasts = false;
            if (interfaceGroup != null)
            {
                interfaceGroup.alpha = 1f;
                interfaceGroup.interactable = true;
                interfaceGroup.blocksRaycasts = true;
            }

            activeRoutine = null;
        }

        private IEnumerator TransitionRoutine(Action onComplete)
        {
            transitioning = true;
            if (interfaceGroup != null)
            {
                interfaceGroup.interactable = false;
                interfaceGroup.blocksRaycasts = false;
            }

            if (cinematicFadeImage != null)
            {
                cinematicFadeImage.color = transitionFadeColor;
            }

            if (cinematicFade != null)
            {
                cinematicFade.blocksRaycasts = true;
            }

            if (eyeCloseupCamera != null)
            {
                eyeCloseupCamera.LookAt = eyeTarget;
                eyeCloseupCamera.Priority = 30;
            }

            if (establishingCamera != null)
            {
                establishingCamera.Priority = 20;
            }

            float startingEyeIntensity = eyeLight != null ? eyeLight.intensity : 0f;
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / transitionDuration);
                float eased = transitionCurve.Evaluate(normalized);

                if (interfaceGroup != null)
                {
                    interfaceGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.42f, normalized));
                }

                if (eyeLight != null)
                {
                    eyeLight.intensity = Mathf.Lerp(startingEyeIntensity, eyeTransitionIntensity, eased);
                }

                if (cinematicFade != null)
                {
                    cinematicFade.alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.68f, 1f, normalized));
                }

                yield return null;
            }

            if (cinematicFade != null)
            {
                cinematicFade.alpha = 1f;
            }

            activeRoutine = null;
            onComplete?.Invoke();
        }
    }
}
