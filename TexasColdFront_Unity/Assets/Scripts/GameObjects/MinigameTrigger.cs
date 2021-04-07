using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf
{

/// <summary>
/// Trigger for starting minigames
/// </summary>
public class MinigameTrigger : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private Minigame minigame;

    public GameObject ThisGO => gameObject;
    public string Action => "start " + Constants.MinigameToText(minigame);

    /// <summary>
    /// starts minigame if in the game state
    /// </summary>
    public void PerformAction()
    {
        if (GameStateMachine.Instance.CurrentState == GameState.GAME)
        {
            if (minigame == Minigame.FURNITURE_BREAKING)
                GameStateMachine.minigameData_furnitureToDestroy = ThisGO;
            GameStateMachine.Instance.CurrentMinigame = minigame;
            GameStateMachine.Instance.CurrentState = GameState.MINIGAME;
        }
    }
}

}