using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace tcf.ui
{
    public class GameOverMenu : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Button buttonQuit;
        [SerializeField] private CanvasGroup containerMenu;
        [SerializeField] private GameObject goodWinText;
        [SerializeField] private GameObject badWinText;
        [SerializeField] private TransitionObject transitionContainerState;
#pragma warning restore 0649
        /**************/
        private IEnumerator transitionContainerInstance;

        /// <summary>
        /// setup ui on unity start
        /// </summary>
        private void Start()
        {
            // Quit button should exit the game
            buttonQuit.onClick.AddListener(() =>
            {
                Application.Quit();
            });

            badWinText.gameObject.SetActive(false);
            goodWinText.gameObject.SetActive(false);
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
            // You can only exit from the game over screen, so prev is not used here
            if (transitionContainerInstance != null)
                StopCoroutine(transitionContainerInstance);
            
            switch (next)
            {
                case GameState.BADWIN:
                    badWinText.gameObject.SetActive(true);
                    transitionContainerInstance = TransitionContainer(true);
                    StartCoroutine(transitionContainerInstance);
                    break;
                case GameState.GOODWIN:
                    goodWinText.gameObject.SetActive(true);
                    transitionContainerInstance = TransitionContainer(true);
                    StartCoroutine(transitionContainerInstance);
                    break;
                default:
                    if (containerMenu.gameObject.activeInHierarchy)
                    {
                        containerMenu.alpha = 0;
                        containerMenu.gameObject.SetActive(false);
                    }
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
