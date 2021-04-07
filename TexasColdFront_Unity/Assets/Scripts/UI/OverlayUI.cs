using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace tcf.ui
{

/// <summary>
/// Manages the overlay UI in the game, which deals with dialogue and interaction overlays
/// </summary>
public class OverlayUI : MonoBehaviour
{
    [SerializeField] private Camera             cameraWhichRenders;
    [SerializeField] private DialogueContainer  dialogueTemplate;
    [SerializeField] private RectTransform      interactionContainer;
    [SerializeField] private TMP_Text           interactionText;
    /**************/ private Vector2            currentInteractableTransform;

    /// <summary>
    /// disable the template and interaction container on start
    /// </summary>
    private void Start()
    {
        interactionContainer.gameObject.SetActive(false);
        dialogueTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// position interaction container every frame if it is active
    /// </summary>
    private void Update()
    {
        if (interactionContainer.gameObject.activeInHierarchy)
        {
            Vector2 pos = RectTransformUtility.WorldToScreenPoint(cameraWhichRenders, currentInteractableTransform)
                + new Vector2(-interactionContainer.rect.width / 4, interactionContainer.rect.height * 1.5f);
            interactionContainer.anchoredPosition = pos;
        }
    }

    /// <summary>
    /// event subscribe on unity enable
    /// </summary>
    private void OnEnable()
    {
        GameStateMachine.InteractableChanged += OnInteractableChanged;
        GameInputSystem.NewDialogue += OnNewDialogue;
    }

    /// <summary>
    /// event unsubscribe on unity disable
    /// </summary>
    private void OnDisable()
    {
        GameStateMachine.InteractableChanged -= OnInteractableChanged;
    }

    /// <summary>
    /// Handles InteractableChanged event, updating the overlay depending on if null or not
    /// </summary>
    /// <param name="interactable">The new interactable (enables and updates overlay) or null (disables)</param>
    private void OnInteractableChanged(IPlayerInteractable interactable)
    {
        if (interactable == null)
            interactionContainer.gameObject.SetActive(false);
        else
        {
            interactionText.text = Constants.ActionToText(interactable.Action);
            interactionContainer.gameObject.SetActive(true);
            currentInteractableTransform = interactable.ThisGO.transform.position;
        }
    }

    /// <summary>
    /// Handles NewDialogue event, which creates a new
    /// </summary>
    /// <param name="npc">The NPC spoken to</param>
    /// <param name="str">The dialogue they spoke</param>
    private void OnNewDialogue(NPC npc, string str)
    {
        // instantiate instance and get components
        GameObject instance = Instantiate(dialogueTemplate.gameObject, dialogueTemplate.transform.parent);
        DialogueContainer instancedDC = instance.GetComponent<DialogueContainer>();
        /*
        RectTransform instancedRT = instance.GetComponent<RectTransform>();
        */

        // set active and position
        instance.SetActive(true);
        /*
        Vector2 pos = RectTransformUtility.WorldToScreenPoint(cameraWhichRenders, currentInteractableTransform)
            + new Vector2(-instancedRT.rect.width / 5, instancedRT.rect.height * 1.5f);
        instancedRT.anchoredPosition = pos;
        */

        // initialize
        instancedDC.Initialize(str, Constants.CharacterIDToString(npc));
    }
}

}