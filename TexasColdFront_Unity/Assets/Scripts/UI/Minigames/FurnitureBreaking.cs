using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tcf.ui
{

/// <summary>
/// Minigame for breaking down furniture into fuel pieces
/// </summary>
/// <remarks>
/// <para>- Destroys furniture once the button has been clicked enough times</para>
/// <para>- Each spawned piece button adds one to the count of pieces in the manager</para>
/// </remarks>
public class FurnitureBreaking : MonoBehaviour
{
    [SerializeField] private Button                 btnFurnishing;
    [SerializeField] private GameObject             btnPieceTemplate;
    [SerializeField] private MinigameUI             manager;
    /**************/ private int                    btnFurnishingClickCount;
    /**************/ private List<Button>           btnPieceInstances;
    // TODO: depend on furniture type for required clicks and number of pieces created
    /**************/ private const int              btnPieceReqClicks = 5;
    /**************/ private bool                   furnitureDestroyed;

    /// <summary>
    /// add listener to button on start
    /// </summary>
    private void Start()
    {
        // the core gameplay of the minigame is with the listener of this button
        btnFurnishing.onClick.AddListener(() => {
            // increment the button click count; if the right number is hit, spawn pieces
            btnFurnishingClickCount++;
            if (btnFurnishingClickCount >= btnPieceReqClicks)
            {
                // make the button noninteractable and loop to spawn pieces
                btnFurnishing.interactable = false;
                furnitureDestroyed = true;
                for (int i = 0; i < btnPieceReqClicks; i++)
                {
                    // instantiate with random position and sprite
                    GameObject instance = Instantiate(btnPieceTemplate, btnPieceTemplate.transform.parent);
                    instance.SetActive(true);
                    Image instanceIMG = instance.GetComponent<Image>();
                    instanceIMG.sprite = manager.FurniturePieceSprite;

                    RectTransform instanceRT = instance.GetComponent<RectTransform>();
                    Vector2 pos = instanceRT.anchoredPosition;
                    pos.x += Random.Range(-1.5f * instanceRT.rect.width, 1.5f * instanceRT.rect.width);
                    pos.y += Random.Range(-1.5f * instanceRT.rect.height, 1.5f * instanceRT.rect.height);
                    instanceRT.anchoredPosition = pos;
                    instanceRT.rotation = Quaternion.Euler(0, 0, Random.Range(-90f, 90f));

                    // track the button of the instance and add listener to click
                    Button instanceBtn = instance.GetComponent<Button>();
                    btnPieceInstances.Add(instanceBtn);
                    instanceBtn.onClick.AddListener(() => {
                        // increment furniture piece count and destroy the button; once all the furniture is clicked,
                        manager.FurniturePieceCount++;
                        btnPieceInstances.Remove(instanceBtn);
                        Destroy(instanceBtn.gameObject);
                    });
                }
            }
        });
    }

    /// <summary>
    /// Resets the state of the minigame
    /// </summary>
    public void ResetMinigame()
    {
        if (btnPieceInstances != null)
            for (int i = btnPieceInstances.Count - 1; i > -1; i--)
                Destroy(btnPieceInstances[i].gameObject);
        btnPieceInstances = new List<Button>(6);
        btnFurnishing.interactable = true;
        btnFurnishingClickCount = 0;

        if (GameStateMachine.minigameData_furnitureToDestroy != null)
        {
            // destroy the furniture which had called the minigame if the minigame was active on reset
            if (furnitureDestroyed)
            {
                Destroy(GameStateMachine.minigameData_furnitureToDestroy);
                GameStateMachine.minigameData_furnitureToDestroy = null;
                furnitureDestroyed = false;
            }

            // set sprite for button
            else
                if (GameStateMachine.minigameData_furnitureToDestroy.TryGetComponent(out SpriteRenderer sr))
                    btnFurnishing.image.sprite = sr.sprite;
        }
    }
}

}