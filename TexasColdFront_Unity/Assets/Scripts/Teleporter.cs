using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf
{

/// <summary>
/// Used for transferring the player between floors of the house
/// </summary>
public class Teleporter : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private GameObject destination;
    [SerializeField] private GameObject player;

    public GameObject ThisGO => gameObject;
    public string Action => "use stairs";

    /// <summary>
    /// Method which moves the player to the destination position
    /// </summary>
    public void PerformAction()
    {
        player.transform.position = destination.transform.position;
    }
}

}