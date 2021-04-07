using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf
{
public class Resources : MonoBehaviour
{
    //Singleton
    private static Resources instance;
    public static Resources Instance { get => instance; }

    //Fields
    [SerializeField] float fuelAmount;
    [SerializeField] float waterAmount;
    [SerializeField] int foodAmount = 10;

    public const float MaxTemp = 70f;
    public const float MinTemp = 25f;

    [Range(MinTemp,MaxTemp)]
    [SerializeField] float temperature = 60.0f;
    //NOTE: Will need to write function to calculate applicated heat
    //          based on distance from central heat source

    const float tempChangeRate = 0.4f;

    bool hasHeat;

    //How much of the fuel is consumed per second
    const float fuelConsumptionRate = 0.02f;

    //References(?)


    //Properties
    public float FuelAmount { get => fuelAmount;}
    public float WaterAmount { get => waterAmount; }
    public bool HasHeat { get => hasHeat;}
    public float Temperature { get => temperature; set => temperature = Mathf.Clamp(value, MinTemp, MaxTemp); }
    public float FoodAmount { get => foodAmount; }

    WaitForSeconds waitForSecond = new WaitForSeconds(1.0f);

    private void Awake()
    {
        //Initialize Singleton
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ConsumeFuel());
        StartCoroutine(UpdateTemp());

        //Delegate sub
        TimeTracker.Instance.timeSkipped += TimeSkip;
    }

    private void OnDestroy()
    {
        //Delegate unsub
        TimeTracker.Instance.timeSkipped -= TimeSkip;
    }

    /// <summary>
    /// Add fuel to the total amount currently prepared
    /// </summary>
    /// <param name="amountToAdd">How much fuel we should add</param>
    public void AddFuel(float amountToAdd)
    {
        fuelAmount += amountToAdd;

        //We have added fuel but we currently have no heat, start consuming fuel
        if(fuelAmount >= 0 && !hasHeat)
        {
            StartCoroutine(ConsumeFuel());
        }

        GameplayStats.Instance.FuelCreated(amountToAdd);
    }

    /// <summary>
    /// Add water to the total amount available
    /// </summary>
    /// <param name="waterToAdd">How much water should be added</param>
    public void AddWater(float waterToAdd)
    {
        waterAmount += waterToAdd;
        GameplayStats.Instance.WaterCreated(waterToAdd);
    }

    /// <summary>
    /// Remove water from the total amount available
    /// </summary>
    /// <param name="waterToRemove">Amount of water to remove from total available</param>
    public bool RemoveWater(float waterToRemove)
    {
        if(waterAmount >= waterToRemove)
        {
            waterAmount -= waterToRemove;
            if (waterAmount < 0)
            {
                waterAmount = 0;
            }

            GameplayStats.Instance.WaterConsumed(waterToRemove);
            return true;
        }

        return false; //No water to drink
        
    }

    /// <summary>
    /// Consume an amount of food from the stocks
    /// </summary>
    /// <param name="amountToConsume">How much food to consume</param>
    /// <returns>True if amount is available for consumption, false if there isn't enough</returns>
    public bool ConsumeFood(int amountToConsume)
    { 
        if(foodAmount >= amountToConsume && foodAmount > 0)
        {
            foodAmount -= amountToConsume;
            GameplayStats.Instance.FoodConsumed(amountToConsume);
            return true;
        }

        //Not enough food
        return false;
    }

    /// <summary>
    /// Add to the stored food
    /// </summary>
    /// <param name="amountToAdd">Amount of food to add to the stored food</param>
    public void AddFood(int amountToAdd)
    {
        foodAmount += amountToAdd;
        GameplayStats.Instance.FoodRetrieved(amountToAdd);
    }

    private void TimeSkip (float secondsSkipped)
    {
        //How much fuel was consumed in that time period
        float fuelConsumed = secondsSkipped * fuelConsumptionRate;

        //Had enough to keep fire going 
        if(fuelConsumed >= fuelAmount)
        {
            fuelAmount -= fuelConsumed;
            GameplayStats.Instance.FuelBurned(fuelConsumed);
            return;
        }
        else
        {
            GameplayStats.Instance.FuelBurned(fuelAmount);
            fuelAmount = 0; //Ran out of fuel
            
            float leftoverTime = (fuelConsumed % fuelAmount) / fuelConsumptionRate;

            //How much the temperature dropped
            temperature -= (tempChangeRate * leftoverTime);
            temperature = Mathf.Clamp(temperature, MinTemp, MaxTemp);
        }

    }

    /// <summary>
    /// Consumes fuel to produce heat if available
    /// </summary>
    /// <returns>Nothing</returns>
    IEnumerator ConsumeFuel()
    {
        hasHeat = true;
        while (fuelAmount > 0)
        {
            yield return waitForSecond;
            if (GameStateMachine.Instance.CurrentState == GameState.GAME || GameStateMachine.Instance.CurrentState == GameState.MINIGAME)
            {
                fuelAmount -= fuelConsumptionRate;
                GameplayStats.Instance.FuelBurned(fuelConsumptionRate);
            }
        }

        //No more fuel, no more heat :(
        hasHeat = false;
    }
    IEnumerator UpdateTemp()
    {
        while (true)
        {
            if (GameStateMachine.Instance.CurrentState == GameState.GAME || GameStateMachine.Instance.CurrentState == GameState.MINIGAME)
            {
                if(hasHeat)
                {
                    temperature += tempChangeRate * 2.0f;
                }
                else
                {
                    temperature -= tempChangeRate;
                }

                //Limit temperature
                temperature = Mathf.Clamp(temperature, MinTemp, MaxTemp);

                GameplayStats.Instance.RecordTemp(temperature); //Stat logging
            }
            yield return waitForSecond;
        }
    }
}
}