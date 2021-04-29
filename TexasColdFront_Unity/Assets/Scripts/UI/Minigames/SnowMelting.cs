using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace tcf.ui
{

/// <summary>
/// Minigame for melting snow into clean water
/// </summary>
/// <remarks>
/// <para>- Water that has reached BOILED will automatically call Resources.AddWater if taking the bucket in time</para>
/// <para>- The interface has its own button for the water;
///   the button in the inventory and this button have the same behaviour with toggling + calling AddWater on BOILED</para>
/// </remarks>
public class SnowMelting : MonoBehaviour
{
    [SerializeField] private Button         bucketBtn;
    [SerializeField] private Image          bucketImg;
    [SerializeField] private TMP_Text       bucketTxt;
    [SerializeField] private MinigameUI     manager;
    /**************/ private float          timer;
    /**************/ private const float    timeUntilStateChange = 3.0f;

    // Audio
    //private FMODUnity.StudioEventEmitter waterBoiling_emitter;
    private FMOD.Studio.EventInstance waterBoiling_sfx;
    private FMOD.Studio.EventInstance waterSplashing_sfx;

    /// <summary>
    /// Determines if the bucket is placed in the minigame
    /// </summary>
    public bool BucketPlaced => bucketBtn.interactable;

    /// <summary>
    /// sets up button listener on start
    /// </summary>
    private void Start()
    {
        waterBoiling_sfx = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/WaterBoiling");
        waterSplashing_sfx = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/WaterSplashingInBucket");
        bucketBtn.onClick.AddListener(() => {
            ToggleBucket(!BucketPlaced);
        });
    }

    /// <summary>
    /// handles the main minigame loop (bucket state switching) and timer on update
    /// </summary>
    private void Update()
    {
        if (BucketPlaced)
        {
            timer += Time.deltaTime;
            if (timer >= timeUntilStateChange)
            {
                switch(manager.BucketFillState)
                {
                    case BucketState.SNOWY:
                        waterBoiling_sfx.setParameterByName("BucketState", 1);
                        manager.BucketFillState = BucketState.WATER;
                        bucketImg.sprite = manager.GetBucketSprite(BucketState.WATER);
                        break;
                    case BucketState.WATER:
                        waterBoiling_sfx.setParameterByName("BucketState", 2);
                        manager.BucketFillState = BucketState.BOILED;
                        bucketImg.sprite = manager.GetBucketSprite(BucketState.BOILED);
                        break;
                    case BucketState.BOILED:
                        waterBoiling_sfx.setParameterByName("BucketState", 3);
                        manager.BucketFillState = BucketState.EMPTY;
                        bucketImg.sprite = manager.GetBucketSprite(BucketState.EMPTY);
                        break;
                }
                timer = 0;
            }
            // animate sprite to warn that it will boil over and lose water
            else if (manager.BucketFillState == BucketState.BOILED)
                bucketImg.color = timer % 1.0f >= 0.5f ? Color.Lerp(Color.red, Color.white, timer % 0.5f) : Color.Lerp(Color.white, Color.red, timer % 0.5f);

            // doubly checking the fill state is redundant but this and the text will be removed when sprite swapping is implemented
            BucketState nextState;
            switch(manager.BucketFillState)
            {
                case BucketState.SNOWY:
                    nextState = BucketState.WATER;
                    break;
                case BucketState.WATER:
                    nextState = BucketState.BOILED;
                    break;
                case BucketState.BOILED:
                default:
                    nextState = BucketState.EMPTY;
                    break;
            }
            if (manager.BucketFillState == BucketState.EMPTY)
                bucketTxt.text = "Water boiled over!";
            else
                bucketTxt.text = string.Format("Time until {0}: {1:0.00}", nextState.ToString(), timeUntilStateChange - timer);
        }
    }

    /// <summary>
    /// Resets the state of the minigame
    /// </summary>
    public void ResetMinigame()
    {
        waterBoiling_sfx.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);  
        waterBoiling_sfx.setParameterByName("BucketState", 0);
        bucketBtn.interactable = false;
        timer = 0;
        bucketImg.color = Color.white;
        bucketTxt.text = "Place bucket from inventory to continue";
    }

    /// <summary>
    /// Toggles the bucket's UI element
    /// </summary>
    /// <param name="state">The new state of the bucket's UI element</param>
    public void ToggleBucket(bool state)
    {
        if (state)
            waterBoiling_sfx.start();
        else
            waterBoiling_sfx.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        bucketBtn.interactable = state;
        bucketImg.sprite = manager.GetBucketSprite(manager.BucketFillState);
        if (!state && manager.BucketFillState == BucketState.BOILED)
        {
            waterSplashing_sfx.start();
            Resources.Instance.AddWater(Constants.WATER_PER_BUCKET);
            manager.BucketFillState = BucketState.EMPTY;
        }
    }
}

}