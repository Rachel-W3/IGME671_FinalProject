using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace tcf.ui
{

/// <summary>
/// Represents the fill state of the bucket
/// </summary>
public enum BucketState
{
    /// <summary>
    /// need to do the snow obtaining minigame
    /// </summary>
    EMPTY = 0,

    /// <summary>
    /// can melt into water
    /// </summary>
    SNOWY = 1,

    /// <summary>
    /// can boil
    /// </summary>
    WATER = 2,

    /// <summary>
    /// can distribute (probably automatic once you put it in inventory)
    /// </summary>
    BOILED = 3,
}

/// <summary>
/// Manages the various minigames in the game
/// </summary>
public class MinigameUI : MonoBehaviour
{
    /// <summary>
    /// Represents the bucket used between minigames
    /// </summary>
    [Serializable]
    protected class Bucket
    {
        public Button       button;
        public Image        icon;
        public BucketState  state;
        public TMP_Text     textState;
        public Sprite       spriteEmpty;
        public Sprite       spriteSnowy;
        public Sprite       spriteWater;
    }

    /// <summary>
    /// Represents the furniture pieces obtained, used from furniture minigame to fire minigame
    /// </summary>
    [Serializable]
    protected class FurniturePieces
    {
        public int          amountStored;
        public Button       button;
        public TMP_Text     textState;
    }

    /// <summary>
    /// Represents the lighter, mainly used for lighting the fire
    /// </summary>
    [Serializable]
    protected class Lighter
    {
        public Button       button;
    }

    [SerializeField] private Image              blurContainer;
    [SerializeField] private Bucket             bucket;
    [SerializeField] private FurniturePieces    furniturePieces;
    [SerializeField] private Lighter            lighter;
    [SerializeField] private Sprite[]           furniturePieceSprites;
    [SerializeField] private CanvasGroup        mainContainer;
    [SerializeField] private TMP_Text           mainContainerHeading;
    [SerializeField] private FireRefueling      minigameFireRefueling;
    [SerializeField] private FurnitureBreaking  minigameFurnitureBreaking;
    [SerializeField] private SnowGathering      minigameSnowGathering;
    [SerializeField] private SnowMelting        minigameSnowMelting;
    [SerializeField] private TransitionObject   transitionContainerState;
    /**************/ private Material           blurContainerMaterialInstance;
    /**************/ private IEnumerator        transitionContainerInstance;

    // Audio
    /**************/ private FMOD.Studio.EventInstance throwWoodToFire_SFX;

    /// <summary>
    /// MinigameUI is a singleton accessible by this static Instance
    /// </summary>
    public static MinigameUI Instance { get; private set; }

    /// <summary>
    /// Gets/sets the current fill state of the bucket and updates the display text
    /// </summary>
    public BucketState BucketFillState
    {
        get { return bucket.state; }
        set
        {
            bucket.state = value;
            bucket.textState.text = value.ToString();
            bucket.icon.sprite = GetBucketSprite(value);
        }
    }

    /// <summary>
    /// Gets/sets the current furniture piece count and updates the display text
    /// </summary>
    public int FurniturePieceCount
    {
        get { return furniturePieces.amountStored; }
        set
        {
            if (value >= 0)
            {
                furniturePieces.amountStored = value;
                furniturePieces.textState.text = value.ToString();
            }

            if (value == 0)
                furniturePieces.button.interactable = false;
            else if (value > 0)
                furniturePieces.button.interactable = true;
        }
    }

    /// <summary>
    /// Gets a random furniture piece sprite from the sprite collection stored in MinigameUI
    /// </summary>
    public Sprite FurniturePieceSprite => furniturePieceSprites[UnityEngine.Random.Range(0, furniturePieceSprites.Length)];

    /// <summary>
    /// Gets the sprite of the bucket based on state
    /// </summary>
    /// <param name="state">state of the bucket</param>
    /// <returns>sprite for the bucket</returns>
    public Sprite GetBucketSprite(BucketState state)
    {
        switch(state)
        {
            case BucketState.EMPTY: return bucket.spriteEmpty;
            case BucketState.SNOWY: return bucket.spriteSnowy;
            default:                return bucket.spriteWater;
        }
    }

    /// <summary>
    /// set singleton instance on unity awake
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;
    }

    /// <summary>
    /// setup ui on start
    /// </summary>
    private void Start()
    {
        bucket.button.onClick.AddListener(() => {
            switch(GameStateMachine.Instance.CurrentMinigame)
            {
                case Minigame.SNOW_GATHERING:
                    if (minigameSnowGathering.BucketState)
                    {
                        if (minigameSnowGathering.BucketIsFull)
                            BucketFillState = BucketState.SNOWY;
                        minigameSnowGathering.BucketState = false;
                    }
                    else if (bucket.state == BucketState.EMPTY)
                        minigameSnowGathering.BucketState = true;
                    break;

                case Minigame.SNOW_MELTING:
                    if (BucketFillState != BucketState.EMPTY)
                    {
                        minigameSnowMelting.ToggleBucket(!minigameSnowMelting.BucketPlaced);
                        if (BucketFillState == BucketState.BOILED)
                        {
                            Resources.Instance.AddWater(Constants.WATER_PER_BUCKET);
                            BucketFillState = BucketState.EMPTY;
                        }
                    }
                    break;
            }
        });

        furniturePieces.button.onClick.AddListener(() => {
            if (GameStateMachine.Instance.CurrentMinigame == Minigame.FIRE_REFUELING)
            {
                if (minigameFireRefueling.AddToFire())
                {
                    // play wood throwing sound effects
                    throwWoodToFire_SFX = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/ThrowWoodToFire");
                    throwWoodToFire_SFX.start();
                    FurniturePieceCount--;
                }
            }
        });

        BucketFillState = BucketState.EMPTY;
        FurniturePieceCount = 0;

        blurContainerMaterialInstance = new Material(blurContainer.material);
        blurContainer.material = blurContainerMaterialInstance;

        DisableAllMinigames();
        StartCoroutine(DelayedStart());
    }

    /// <summary>
    /// Coroutine for delaying the disabling of the phone menu
    /// </summary>
    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        transitionContainerInstance = TransitionContainer(false);
        StartCoroutine(transitionContainerInstance);
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
            case GameState.MINIGAME:
                switch(GameStateMachine.Instance.CurrentMinigame)
                {
                    case Minigame.SNOW_GATHERING:
                        mainContainerHeading.text = "Gathering Snow";
                        minigameSnowGathering.transform.gameObject.SetActive(true);
                        minigameSnowGathering.ResetMinigame();
                        break;
                    case Minigame.SNOW_MELTING:
                        mainContainerHeading.text = "Melting Snow";
                        minigameSnowMelting.transform.gameObject.SetActive(true);
                        minigameSnowMelting.ResetMinigame();
                        break;
                    case Minigame.FURNITURE_BREAKING:
                        mainContainerHeading.text = "Breaking Furniture";
                        minigameFurnitureBreaking.transform.gameObject.SetActive(true);
                        minigameFurnitureBreaking.ResetMinigame();
                        break;
                    case Minigame.FIRE_REFUELING:
                        mainContainerHeading.text = "Refueling Fire";
                        minigameFireRefueling.transform.gameObject.SetActive(true);
                        minigameFireRefueling.ResetMinigame();
                        break;
                }
                transitionContainerInstance = TransitionContainer(true);
                StartCoroutine(transitionContainerInstance);
                break;
            default:
            if (prev == GameState.MINIGAME)
            {
                DisableAllMinigames();
                transitionContainerInstance = TransitionContainer(false);
                StartCoroutine(transitionContainerInstance);
            }
            else if (mainContainer.gameObject.activeInHierarchy)
                mainContainer.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Disables all the minigame gameobjects in the hierarchy
    /// </summary>
    private void DisableAllMinigames()
    {
        minigameFireRefueling.transform.gameObject.SetActive(false);
        minigameFurnitureBreaking.transform.gameObject.SetActive(false);
        minigameFurnitureBreaking.ResetMinigame(); // this should be reset on disable to destroy furniture in the world
        minigameSnowGathering.transform.gameObject.SetActive(false);
        minigameSnowMelting.transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// Coroutine for transitioning the pause menu container alpha
    /// </summary>
    /// <param name="state">the state the container should end on</param>
    private IEnumerator TransitionContainer(bool state)
    {
        // initialize container for animation
        mainContainer.gameObject.SetActive(true);
        mainContainer.alpha = state ? 0 : 1;
        blurContainerMaterialInstance.SetFloat("_Size", state ? 0 : 8);
        float t = 0;

        // loop for animation
        while (t <= transitionContainerState.animationLength)
        {
            yield return new WaitForEndOfFrame();
            t += Time.unscaledDeltaTime;
            mainContainer.alpha = state ?
                Mathf.Lerp(0, 1, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength)) :
                Mathf.Lerp(1, 0, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength));
            blurContainerMaterialInstance.SetFloat("_Size", state ? 
                Mathf.Lerp(0, 8, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength)) :
                Mathf.Lerp(8, 0, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength)));
        }

        // ensure alpha is fully set (b/c of dt animation may not perfectly set to 1) if active, or deactivate in hierarchy if inactive
        if (state)
            mainContainer.alpha = 1;
        else
            mainContainer.gameObject.SetActive(false);
    }
}

}