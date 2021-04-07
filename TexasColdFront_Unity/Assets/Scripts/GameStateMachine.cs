using UnityEngine;

namespace tcf
{

/// <summary>
/// Defines states for the game
/// </summary>
public enum GameState
{
    /// <summary>
    /// in the menus
    /// </summary>
    MENU = 0,

    /// <summary>
    /// in the game but paused
    /// </summary>
    PAUSED = 1,

    /// <summary>
    /// in the game unpaused
    /// </summary>
    GAME = 2,
    
    /// <summary>
    /// in the game using the phone ui
    /// </summary>
    PHONE = 3,

    /// <summary>
    /// in the game using a minigame interface
    /// </summary>
    MINIGAME = 4,

    /// <summary>
    /// in the win screen with family present
    /// </summary>
    GOODWIN = 5,

    /// <summary>
    /// in the win screen without family present
    /// </summary>
    BADWIN = 6,
}

/// <summary>
/// Defines states for the minigame UI/system
/// </summary>
public enum Minigame
{
    /// <summary>
    /// minigame where you click on the snow to create snowballs and drag snowballs into bucket
    /// </summary>
    SNOW_GATHERING = 0,

    /// <summary>
    /// minigame where you heat up a bucket of snow or water to make it reach boiling
    /// </summary>
    SNOW_MELTING = 1,

    /// <summary>
    /// minigame where you break down furniture into fragments
    /// </summary>
    FURNITURE_BREAKING = 2,

    /// <summary>
    /// minigame where you ensure the fire is lit and has enough fuel
    /// </summary>
    FIRE_REFUELING = 3,
}

/// <summary>
/// Manages the internal state of the game
/// </summary>
public class GameStateMachine : MonoBehaviour
{
    // TODO: change this into a struct/class which tracks furniture type in order to achieve proper sprite swapping in FurnitureBreaking
    public  static GameObject       minigameData_furnitureToDestroy;
    private Minigame                currentMinigame;
    private GameState               currentState;
    private IPlayerInteractable     currentInteractableInRange;

    /// <summary>
    /// GameStateMachine is a singleton accessible by this static Instance
    /// </summary>
    public static GameStateMachine Instance { get; private set; }

    /// <summary>
    /// Event for handling on GameStateChanged, which passes previous and current states
    /// </summary>
    public static event GameStateChangeDelegate     GameStateChanged;

    /// <summary>
    /// Event for handling on InteractableChanged, which passes the string Action of the new interactable
    /// </summary>
    public static event InteractableDelegate        InteractableChanged;

    /// <summary>
    /// Event for handling on MinigameChanged, which passes the previous and current minigames
    /// </summary>
    public static event MinigameStateChangeDelegate MinigameChanged;

    /// <summary>
    /// the current state of the game, useful for things such as UI and time scaling
    /// </summary>
    public GameState CurrentState
    {
        get { return currentState; }

        set
        {
            if (value != currentState)
            {
                GameStateChanged?.Invoke(currentState, value);
                currentState = value;
                Time.timeScale = currentState == GameState.GAME || currentState == GameState.MINIGAME ? 1 : 0;
            }
        }
    }

    /// <summary>
    /// the current minigame
    /// </summary>
    public Minigame CurrentMinigame
    {
        get { return currentMinigame; }

        set
        {
            if (value != currentMinigame)
            {
                MinigameChanged?.Invoke(currentMinigame, value);
                currentMinigame = value;
            }
        }
    }

    /// <summary>
    /// the current interactable in range of the character
    /// </summary>
    public IPlayerInteractable CurrentInteractableInRange
    {
        get { return currentInteractableInRange; }

        set
        {
            if (currentInteractableInRange != value)
            {
                currentInteractableInRange = value;
                InteractableChanged?.Invoke(currentInteractableInRange);
            }
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
    /// Set initial state on unity start
    /// </summary>
    private void Start()
    {
        CurrentState = GameState.MENU;
    }

    /// <summary>
    /// event subscribe on unity enable
    /// </summary>
    private void OnEnable()
    {
        GameInputSystem.KeyUp_MenuBack += OnKeyUp_MenuBack;
        GameInputSystem.KeyUp_TogglePhone += OnKeyUp_TogglePhone;
        GameInputSystem.KeyUp_Interact += OnKeyUp_Interact;
    }

    /// <summary>
    /// event unsubscribe on unity disable
    /// </summary>
    private void OnDisable()
    {
        GameInputSystem.KeyUp_MenuBack -= OnKeyUp_MenuBack;
        GameInputSystem.KeyUp_TogglePhone -= OnKeyUp_TogglePhone;
        GameInputSystem.KeyUp_Interact -= OnKeyUp_Interact;
    }

    /// <summary>
    /// Handles input KeyUp_MenuBack
    /// </summary>
    private void OnKeyUp_MenuBack()
    {
        switch (currentState)
        {
            case GameState.GAME:
                CurrentState = GameState.PAUSED;
                break;
            case GameState.PAUSED:
            case GameState.PHONE:
            case GameState.MINIGAME:
                CurrentState = GameState.GAME;
                break;
            case GameState.MENU:
                // menu does nothing for now
                // there may be other ui/game states (such as the phone which escape can close,
                // or an expansion on the menu state which can be used for traversing such as settings > main)
                break;
            default:
                // shouldn't theoretically reach here
                break;
        }
    }

    /// <summary>
    /// Handles input KeyUp_TogglePhone
    /// </summary>
    private void OnKeyUp_TogglePhone()
    {
        switch (currentState)
        {
            case GameState.GAME:
                CurrentState = GameState.PHONE;
                break;
            case GameState.PHONE:
                CurrentState = GameState.GAME;
                break;
            default:
                // do nothing, can only toggle phone from phone or game
                break;
        }
    }

    /// <summary>
    /// Handles input KeyUp_Interact
    /// </summary>
    private void OnKeyUp_Interact()
    {
        if (currentInteractableInRange != null)
            currentInteractableInRange.PerformAction();
    }
}

}