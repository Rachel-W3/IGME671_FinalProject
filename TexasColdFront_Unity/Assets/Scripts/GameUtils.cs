using System;
using UnityEngine;

namespace tcf
{

public enum NPC
{
    HUSBAND = 0,
    CHILD1 = 1,
    CHILD2 = 2,
}

public enum FurnitureSpriteType
{
    SOFA = 0,
    KITCHEN_CHAIR = 1,
    KITCHEN_DESK = 2,
    PAINTING = 3,
    WELCOME_MAT = 4,
}

public delegate void StringDelegate(string str);
public delegate void InteractableDelegate(IPlayerInteractable ipi);
public delegate void MessageDelegate(NPC npc, string str);
public delegate void GenericDelegate();
public delegate void GameStateChangeDelegate(GameState prev, GameState next);
public delegate void MinigameStateChangeDelegate(Minigame prev, Minigame next);

/// <summary>
/// Object for defining coded LERP animations in Unity Inspector
/// </summary>
[Serializable]
public struct TransitionObject
{
    public float            animationLength;
    public AnimationCurve   animationCurve;
}

/// <summary>
/// Defines structure for a timestamp
/// </summary>
[Serializable]
public struct Timestamp
{
    public int  day;
    public int  hour;
    public int  minute;

    public Timestamp(int _day, int _hour, int _minute)
    {
        day = _day;
        hour = _hour;
        minute = _minute;
    }
}

/// <summary>
/// Defines structure for a message
/// </summary>
[Serializable]
public struct Message
{
    public string           message;
    public Timestamp        timestamp;

    /// <summary>
    /// Constructor for creating a message specifically when sending someone out
    /// </summary>
    public Message(int _day, int _hour, int _minute)
    {
        timestamp = new Timestamp(_day, _hour, _minute);
        message = "I'm headed out to get food";
    }

    /// <summary>
    /// Constructor for creating a message specifically when someone comes back (with _foodObtained amount of food)
    /// </summary>
    public Message(int _day, int _hour, int _minute, int _foodObtained)
    {
        timestamp = new Timestamp(_day, _hour, _minute);
        message = string.Format("I got {0:00} food while I was out", _foodObtained);
    }
}

public static class Constants
{
    // different furniture yields different amounts of fuel pieces, but each piece is 5 fuel
    public const float FUEL_PER_PIECE       = 5.0f;
    // each bucket of water provides 10 water
    public const float WATER_PER_BUCKET     = 10.0f;

    /// <summary>
    /// Converts NPC to a properly formed string
    /// </summary>
    /// <param name="other">The ID of the character</param>
    /// <returns>The name of the character as a string</returns>
    public static string CharacterIDToString(NPC other)
    {
        switch(other)
        {
            case NPC.CHILD2:    return "Daughter";
            case NPC.CHILD1:    return "Son";
            case NPC.HUSBAND:   return "Husband";
            default:            return "MISSINGNO";
        }
    }

    /// <summary>
    /// Converts action-type interaction to a string
    /// </summary>
    /// <param name="action">The action to insert to the string</param>
    /// <returns>The string which is used for the interaction overlay</returns>
    public static string ActionToText(string action)
    {
        return "Press " + GameInputSystem.Instance.GameInputs[GameInput.Interact] + " to " + action;
    }

    /// <summary>
    /// Converts the mouse position to a position within a canvas
    /// </summary>
    /// <param name="container">The canvas the point will be located within (assumes overlay canvas)</param>
    /// <returns>The converted mouse position</returns>
    public static Vector2 MousePointToCanvasPoint(RectTransform container)
    {
        Vector2 mousePos = Input.mousePosition;
        mousePos.y = mousePos.y - Screen.height;
        Vector2 screen = new Vector2(Screen.width, Screen.height);
        Vector2 conversionFactor = screen / container.rect.size;
        mousePos /= conversionFactor;
        return mousePos;
    }

    /// <summary>
    /// Converts Minigame to properly formed string
    /// </summary>
    /// <param name="mg">The minigame to convert</param>
    /// <returns>The string label of the minigame</returns>
    public static string MinigameToText(Minigame mg)
    {
        switch(mg)
        {
            case Minigame.SNOW_GATHERING:       return "Gathering Snow";
            case Minigame.SNOW_MELTING:         return "Melting Snow";
            case Minigame.FIRE_REFUELING:       return "Refueling Fire";
            case Minigame.FURNITURE_BREAKING:   return "Breaking Furniture";
            default:                            return "MISSINGNO";
        }
    }
}

}