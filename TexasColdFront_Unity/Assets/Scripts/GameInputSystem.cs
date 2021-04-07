using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf
{

/// <summary>
/// Enumeration of definitions of possible input key names
/// </summary>
public enum GameInput
{
    MenuBack = 0,
    MoveLeft = 1,
    MoveRight = 2,
    MoveIn = 3,
    Interact = 4,
    TogglePhone = 5,
}

/// <summary>
/// Framework for handling game input and passing it to event subscribers
/// </summary>
public class GameInputSystem : MonoBehaviour
{
    // set of key inputs
    private Dictionary<GameInput, KeyCode> gameInputs;

    /// <summary>
    /// GameStateMachine is a singleton accessible by this static Instance
    /// </summary>
    public static GameInputSystem Instance { get; private set; }

    public Dictionary<GameInput, KeyCode> GameInputs => gameInputs;

    #region Input Events
    public static event GenericDelegate KeyUp_Interact;
    public static event GenericDelegate KeyUp_MenuBack;
    public static event GenericDelegate KeyHeld_Interact;
    public static event GenericDelegate KeyHeld_MoveLeft;
    public static event GenericDelegate KeyHeld_MoveRight;
    public static event GenericDelegate KeyHeld_MoveIn;
    public static event GenericDelegate KeyUp_MoveLeft;
    public static event GenericDelegate KeyUp_MoveRight;
    public static event GenericDelegate KeyUp_MoveIn;
    public static event GenericDelegate KeyUp_TogglePhone;
    public static event MessageDelegate NewDialogue;
    #endregion

    /// <summary>
    /// set singleton instance on unity awake
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;

        gameInputs = new Dictionary<GameInput, KeyCode>()
        {
            { GameInput.MenuBack,           KeyCode.Escape },
            { GameInput.Interact,           KeyCode.E },
            { GameInput.MoveIn,             KeyCode.W },
            { GameInput.MoveLeft,           KeyCode.A },
            { GameInput.MoveRight,          KeyCode.D },
            { GameInput.TogglePhone,        KeyCode.Tab },
        };
    }

    /// <summary>
    /// call input events on update
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyUp(gameInputs[GameInput.MenuBack]))
            KeyUp_MenuBack?.Invoke();
        if (Input.GetKeyUp(gameInputs[GameInput.Interact]))
            KeyUp_Interact?.Invoke();
        if (Input.GetKey(gameInputs[GameInput.Interact]))
            KeyHeld_Interact?.Invoke();
        if (Input.GetKey(gameInputs[GameInput.MoveLeft]))
            KeyHeld_MoveLeft?.Invoke();
        if (Input.GetKeyUp(gameInputs[GameInput.MoveLeft]))
             KeyUp_MoveLeft?.Invoke();
        if (Input.GetKey(gameInputs[GameInput.MoveRight]))
            KeyHeld_MoveRight?.Invoke();
        if (Input.GetKeyUp(gameInputs[GameInput.MoveRight]))
             KeyUp_MoveRight?.Invoke();
        if (Input.GetKey(gameInputs[GameInput.MoveIn]))
            KeyHeld_MoveIn?.Invoke();
        if (Input.GetKeyUp(gameInputs[GameInput.MoveIn]))
            KeyUp_MoveIn?.Invoke();
        if (Input.GetKeyUp(gameInputs[GameInput.TogglePhone]))
            KeyUp_TogglePhone?.Invoke();
    }

    /// <summary>
    /// call dialogue event when interacting with npc
    /// </summary>
    /// <param name="id">the id of the character</param>
    /// <param name="msg">the message from the character</param>
    public void OnCharacterInteract(NPC id, string msg)
    {
        NewDialogue?.Invoke(id, msg);
    }
}

}