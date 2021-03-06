using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace tcf.ui
{

/// <summary>
/// Object which handles functions related to the main menu
/// </summary>
public class MainMenu : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Button buttonStart;
        [SerializeField] private Button buttonQuit;
        [SerializeField] private CanvasGroup containerMenu;
        [SerializeField] private GameObject containerText;
        [SerializeField] private TransitionObject transitionContainerState;
#pragma warning restore 0649
        private IEnumerator transitionContainerInstance;

        // Audio
        private FMOD.Studio.EventInstance buttonPressed_SFX;

        /// <summary>
        /// setup ui on unity start
        /// </summary>
        private void Start()
        {
            buttonPressed_SFX = FMODUnity.RuntimeManager.CreateInstance("event:/Interface/Menu_ButtonPressed");

            // Play button should set game state to GAME, but we're starting with paused
            // so the instructions appear first thing
            buttonStart.onClick.AddListener(() =>
            {
                buttonPressed_SFX.start();
                GameStateMachine.Instance.CurrentState = GameState.PAUSED;
            });

            // Quit button should exit the game
            buttonQuit.onClick.AddListener(() =>
            {
                buttonPressed_SFX.start();
                Application.Quit();
            });
        }

        /// <summary>
        /// event subscribe on unity enable
        /// </summary>
        private void OnEnable()
        {
            GameStateMachine.GameStateChanged += OnGameStateChanged;
        }

        /// <summary>
        /// event unsubscribe on unity disable
        /// </summary>
        private void OnDisable()
        {
            GameStateMachine.GameStateChanged -= OnGameStateChanged;
        }

        /// <summary>
        /// Handler for GameStateMachine.GameStateChanged, toggles the menu
        /// </summary>
        /// <param name="prev">the previous GameState</param>
        /// <param name="next">the current/new GameState</param>
        private void OnGameStateChanged(GameState prev, GameState next)
        {
            if (transitionContainerInstance != null)
                StopCoroutine(transitionContainerInstance);

            switch (next)
            {
                case GameState.MENU:
                    transitionContainerInstance = TransitionContainer(true);
                    StartCoroutine(transitionContainerInstance);
                    break;
                default:
                    if (prev == GameState.MENU)
                    {
                        transitionContainerInstance = TransitionContainer(false);
                        StartCoroutine(transitionContainerInstance);
                    }
                    else if (containerMenu.gameObject.activeInHierarchy)
                        containerMenu.gameObject.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// Coroutine for transitioning the pause menu container alpha
        /// </summary>
        /// <param name="state">the state the container should end on</param>
        private IEnumerator TransitionContainer(bool state)
        {
            // initialize container for animation
            containerMenu.gameObject.SetActive(true);
            containerMenu.alpha = state ? 0 : 1;
            float offsetX = state ? 400.0f : -400.0f;
            Vector3 start = containerText.transform.position;
            Vector3 end = containerText.transform.position + new Vector3(offsetX, 0, 0);
            float t = 0;

            // loop for animation
            while (t <= transitionContainerState.animationLength)
            {
                yield return new WaitForEndOfFrame();
                t += Time.unscaledDeltaTime;
                // Fade effect
                containerMenu.alpha = state ?
                    Mathf.Lerp(0, 1, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength)) :
                    Mathf.Lerp(1, 0, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength));
                // Slide effect
                containerText.transform.position =
                    Vector3.Lerp(start, end, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength));
            }

            // ensure alpha is fully set (b/c of dt animation may not perfectly set to 1) if active, or deactivate in hierarchy if inactive
            if (state)
            {
                containerMenu.alpha = 1;
            }
            else
                containerMenu.gameObject.SetActive(false);
        }
    }

}