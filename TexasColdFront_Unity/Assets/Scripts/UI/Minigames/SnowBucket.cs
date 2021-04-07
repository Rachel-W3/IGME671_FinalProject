using UnityEngine;
using UnityEngine.UI;

namespace tcf.ui
{

/// <summary>
/// Used for handling the bucket from the snow gathering minigame
/// </summary>
public class SnowBucket : MonoBehaviour
{
    [SerializeField] private Image          bucketImage;
    [SerializeField] private Collider2D[]   colliders;
    [SerializeField] private SnowGathering  manager;
    /**************/ private int            levelOfSnow;
    /**************/ private const int      requiredSnow = 6;
    
    public bool BucketIsFull => levelOfSnow >= requiredSnow;
    public bool IsActive => bucketImage.color.a >= 0.9f;

    /// <summary>
    /// Sets the bucket as active/inactive and resets snow level
    /// </summary>
    /// <param name="state">The state of the bucket</param>
    public void SetActive(bool state)
    {
        Color c = bucketImage.color;
        c.a = state ? 1.0f : 0.1f;
        bucketImage.color = c;
        levelOfSnow = 0;
        foreach(Collider2D col in colliders)
            col.enabled = state;
        bucketImage.sprite = manager.GetBucketSprite(BucketState.EMPTY);
    }

    /// <summary>
    /// Adds snow to bucket when snow enters trigger
    /// </summary>
    /// <param name="collision">The snow draggable</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out DraggableSprite ds))
            if (ds.Type == DraggableType.SNOWBALL)
                AddSnow(ds);
    }

    /// <summary>
    /// Adds snow to the bucket's snow level and removes it from the manager's list
    /// </summary>
    /// <param name="snow">The snow to despawn</param>
    private void AddSnow(DraggableSprite snow)
    {
        levelOfSnow++;
        manager.RemoveSnowFromList(snow);

        if (BucketIsFull)
            bucketImage.sprite = manager.GetBucketSprite(BucketState.SNOWY);
    }
}

}