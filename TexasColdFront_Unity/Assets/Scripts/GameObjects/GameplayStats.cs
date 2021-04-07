using System;
using System.Collections;
using System.Collections.Generic;
using tcf.obj;
using UnityEngine;

namespace tcf
{
public class GameplayStats : MonoBehaviour
{
    //Notes
    /* 
     * In a larger scale game it would probably be
     * better to track these stats using delegates/events
     * but since this is the final submission
     * I'm just gonna do it with a singleton
     * and a crap ton of public methods
     *  - Ethan B)
     */

    private static GameplayStats instance;

    public static GameplayStats Instance { get => instance; }
    


    #region Fields

    int daysSurvived = 0;

    bool husbandSurvived = true;
    bool childOneSurvived = true;
    bool childTwoSurvived = true;

    float foodConsumed = 0;
    float foodRetrieved = 0;

    float waterConsumed = 0;
    float waterCreated = 0;

    float fuelCreated = 0;
    float fuelBurned = 0;

    bool playerWon = true;

    float averageTemp = 50.0f;
    List<float> tempRecords = new List<float>();

    int timesSlept = 0;

	#endregion


	#region Properties
	public int DaysSurvived { get => daysSurvived;}
    public bool HusbandSurvived { get => husbandSurvived; }
    public bool ChildOneSurvived { get => childOneSurvived;}
    public bool ChildTwoSurvived { get => childTwoSurvived;}
    public float FoodConsumedP { get => foodConsumed;}
    public float FoodRetrievedP{ get => foodRetrieved;}
    public float WaterConsumedP { get => waterConsumed;}
    public float WaterCreatedP { get => waterCreated;}
    public float FuelCreatedP { get => fuelCreated;}
    public float FuelBurnedP { get => fuelBurned;}
    public float AverageTemp { get => averageTemp;}

	#endregion

	// Start is called before the first frame update
	void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void GameOver(bool playerWin)
    {
        daysSurvived = (int)TimeTracker.Instance.DaysElapsed;

        //Who survived
        husbandSurvived = !NPCStatus.Instance.Characters[0].Unconscious;
        childOneSurvived = !NPCStatus.Instance.Characters[1].Unconscious;
        childTwoSurvived = !NPCStatus.Instance.Characters[2].Unconscious;

        //Did the player win?
        playerWon = playerWin;

        //Change gamestate
        if (playerWon)
            GameStateMachine.Instance.CurrentState = GameState.GOODWIN;
        else
            GameStateMachine.Instance.CurrentState = GameState.BADWIN;

        //Calculate average temp over course of gameplay
        if (tempRecords.Count != 0)
        {
           
            for (int i = 0; i < tempRecords.Count; i++)
            {
                averageTemp += tempRecords[i];
            }

            averageTemp /= (float)tempRecords.Count;
        }
    }

    public void RecordTemp(float currentTempIn)
    {
        tempRecords.Add(currentTempIn);
    }

    //Fuel
    public void FuelCreated(float amountCreatedIN)
    {
        fuelCreated += amountCreatedIN;
    }

    public void FuelBurned(float amountBurnedIN)
    {
        fuelBurned += amountBurnedIN;
    }

    //Water
    public void WaterCreated(float amountCreatedIN)
    {
        waterCreated += amountCreatedIN;
    }
    public void WaterConsumed(float amountConsumedIN)
    {
        waterConsumed += amountConsumedIN;
    }

    //Food
    public void FoodRetrieved(float amountConsumedIN)
    {
        foodRetrieved += amountConsumedIN;
    }

    public void FoodConsumed(float amountConsumedIN)
    {
        foodConsumed += amountConsumedIN;
    }

    //Other
    public void PlayerSlept()
    {
        timesSlept++;
    }
}
}