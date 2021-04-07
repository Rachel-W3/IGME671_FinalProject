using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf
{

/// <summary>
/// Object for interfacing with interactables
/// </summary>
public interface IPlayerInteractable
{
    GameObject ThisGO { get; }
    string Action { get; }
    void PerformAction();
}

}