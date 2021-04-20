using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tcf.ui
{

/// <summary>
/// Minigame for gathering snow into bucket
/// </summary>
/// <remarks>
/// <para>- The manager determines whether or not BucketIsFull when its bucket button is clicked
///         in order to switch the state in the inventory</para>
/// </remarks>
public class SnowGathering : MonoBehaviour
{
    [SerializeField] private MinigameUI                 manager;
    [SerializeField] private SnowBucket                 snowBucket;
    [SerializeField] private Button                     snowBtn;
    [SerializeField] private DraggableSprite            snowDraggableTemplate;
    /**************/ private List<DraggableSprite>      spawnedDraggables;

    // Audio
    private FMOD.Studio.EventInstance snowballGeneration_sfx;
    private FMOD.Studio.EventInstance dropSnowInBucket_sfx;
    
    /// <summary>
    /// Gets whether the bucket is full or not
    /// </summary>
    public bool BucketIsFull => snowBucket.BucketIsFull;

    /// <summary>
    /// Gets the sprite of the bucket based on state
    /// </summary>
    /// <param name="state">state of the bucket</param>
    /// <returns>sprite for the bucket</returns>
    public Sprite GetBucketSprite(BucketState state) => manager.GetBucketSprite(state);

    /// <summary>
    /// Gets/sets the bucket state
    /// </summary>
    public bool BucketState 
    {
        get { return snowBucket.IsActive; }
        set { snowBucket.SetActive(value); }
    }

    /// <summary>
    /// Sets up event listener for snow on start
    /// </summary>
    private void Start()
    {
        // Init audio
        snowballGeneration_sfx = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/SnowballGeneration");
        dropSnowInBucket_sfx = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/DropSnowInBucket");

        snowBtn.onClick.AddListener(() => {
            snowballGeneration_sfx.start();
            if (spawnedDraggables.Count < 3)
            {
                RectTransform parentTransform = snowDraggableTemplate.transform.parent.GetComponent<RectTransform>();
                for (int i = 0; i < 3; i++)
                {
                    GameObject instance = Instantiate(snowDraggableTemplate.gameObject, snowDraggableTemplate.transform.parent);
                    instance.SetActive(true);
                    RectTransform instanceRT = instance.GetComponent<RectTransform>();
                    switch (i)
                    {
                        case 0:
                            instanceRT.anchoredPosition += parentTransform.rect.size / 2 * new Vector2(1, -1) - new Vector2(-300, -100);
                            break;
                        case 1:
                            instanceRT.anchoredPosition += parentTransform.rect.size / 2 * new Vector2(1, -1) - new Vector2(-150, -50);
                            break;
                        case 2:
                            instanceRT.anchoredPosition += parentTransform.rect.size / 2 * new Vector2(1, -1) - new Vector2(0, -100);
                            break;
                    }
                    spawnedDraggables.Add(instance.GetComponent<DraggableSprite>());
                }
            }
        });
        ResetMinigame();
    }

    /// <summary>
    /// Resets the state of the minigame
    /// </summary>
    public void ResetMinigame()
    {
        snowBucket.SetActive(false);
        if (spawnedDraggables != null)
            for (int i = spawnedDraggables.Count - 1; i > -1; i--)
                Destroy(spawnedDraggables[i].gameObject);
        spawnedDraggables = new List<DraggableSprite>(6);
    }

    /// <summary>
    /// Removes a snow draggable from the list
    /// </summary>
    /// <param name="toRemove">The draggable to remove</param>
    public void RemoveSnowFromList(DraggableSprite toRemove)
    {
        dropSnowInBucket_sfx.start();
        spawnedDraggables.Remove(toRemove);
        Destroy(toRemove.gameObject);
    }
}

}