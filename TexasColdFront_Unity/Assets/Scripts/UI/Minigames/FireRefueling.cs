using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace tcf.ui
{

/// <summary>
/// Minigame for managing the fire
/// </summary>
/// <remarks>
/// <para>- Reads the fuel and indicates up to 5 fuel (can't add more when the level is over 5)</para>
/// <para>- Gives a small boost to the temperature when stoked</para>
/// </remarks>
public class FireRefueling : MonoBehaviour
{
    [SerializeField] private GameObject[]   fuelLevelIndicator;
    [SerializeField] private GameObject     fireLitIndicator;
    [SerializeField] private GameObject     fireSpriteMinigameObject;
    [SerializeField] private Button         fireStokeButton;
    [SerializeField] private TMP_Text       fireStokeIndicatorText;
    [SerializeField] private MinigameUI     manager;
    /**************/ public  const float    fuelPerPiece = 1.0f;
    /**************/ private const float    fireStokeIncrease = 5.0f;
    /**************/ private const float    fireStokeButtonWaitTime = 30.0f;

    /// <summary>
    /// set up listeners on start
    /// </summary>
    private void Start()
    {
        fireSpriteMinigameObject.SetActive(false);
        // fire stoke button gives small boost to temperature on click and can only be clicked every fireStokeButtonWaitTime
        fireStokeButton.onClick.AddListener(() => {
            if (Resources.Instance.FuelAmount > 0) { 
                Resources.Instance.Temperature += fireStokeIncrease;
                fireStokeButton.interactable = false;
                fireStokeIndicatorText.text = "Cannot stoke right now";
                // we're calling startcoroutine from manager because the GO FireRefueling is attached to
                // can be SetActive(false), which breaks coroutines
                manager.StartCoroutine(WaitToReactivateButton());
            }
        });
    }

    /// <summary>
    /// Resets the minigame
    /// </summary>
    public void ResetMinigame()
    {
        ResetFuelIndicator();
    }

    /// <summary>
    /// Checks if oen can add fuel to the fire and adds it to Resources if so
    /// </summary>
    /// <returns>Whether fuel was added</returns>
    public bool AddToFire()
    {
        if (Mathf.Round(Resources.Instance.FuelAmount) < fuelLevelIndicator.Length)
        {
            Resources.Instance.AddFuel(fuelPerPiece);
            ResetFuelIndicator();
            return true;
        }
        else
        { 
            ResetFuelIndicator();
            return false;
        }
    }

    /// <summary>
    /// Resets the fire's fuel indicator and fire lit indicator
    /// </summary>
    private void ResetFuelIndicator()
    {
        if (Resources.Instance.FuelAmount > 0)
        { 
            for (int i = 0; i < fuelLevelIndicator.Length; i++)
                if (Mathf.Round(Resources.Instance.FuelAmount) >= i + 1)
                    fuelLevelIndicator[i].SetActive(true);
                else
                    fuelLevelIndicator[i].SetActive(false);
            fireLitIndicator.SetActive(true);

            // Show active fire in the game's world space as well as the UI
            fireSpriteMinigameObject.SetActive(true);
        }
        else
        {
            foreach(GameObject go in fuelLevelIndicator)
                go.SetActive(false);
            fireLitIndicator.SetActive(false);
            // Disable fire in the game's world space as well as the UI
            fireSpriteMinigameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Waits to reactivate the stoke fire button
    /// </summary>
    private IEnumerator WaitToReactivateButton()
    {
        yield return new WaitForSeconds(fireStokeButtonWaitTime);
        fireStokeButton.interactable = true;
        fireStokeIndicatorText.text = "Stoke Fire";
    }
}

}