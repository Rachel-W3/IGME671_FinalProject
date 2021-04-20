using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf.obj
{

/// <summary>
/// Object which represents an instance of an NPC
/// </summary>
public class NonPlayerCharacter : MonoBehaviour, IPlayerInteractable
{
    // Audio
    private FMOD.Studio.EventInstance buttonPressed_SFX;    

    [SerializeField] private NPC            id;
    [SerializeField] private string[]       possibleDialogue;
    [SerializeField] private List<Message>  messages;


#region NPCStatFields

    //Not hungry = 0, Starving = 1
    [Range(0.0f,1.0f)]
    float hunger = 0;

    [Range(0.0f, 1.0f)]
    float thirst = 0;


    [SerializeField]
    [Range(0.0f,1f)]
    float hungerIncreasePerDay = 0.2f;

    [SerializeField]
    [Range(0f, 1f)]
    float thirstIncreasePerDay = 0.3f;

    [SerializeField]
    [Tooltip("The range of how much food this person can retrieve")]
    Vector2 amountOfRetrievedFood = new Vector2(2,5);

    bool cold = false;
    bool sick = false;
    bool exhausted = false;
    bool unconscious = false;

    int daysCold = 0;
    int daysSick = 0;
    int daysExhausted = 0;
    bool outForFood = false;


#endregion



#region Properties

    public string Action => "talk";
    public GameObject ThisGO => gameObject;
    public NPC ID => id;
    public List<Message> Messages => messages;

    public bool Cold { get => cold;}
    public bool Sick { get => sick;}
    public bool Exhausted { get => exhausted; }
    public bool Unconscious { get => unconscious; }
    public int DaysCold { get => daysCold; }
    public int DaysSick { get => daysSick; }
    public int DaysExhausted { get => daysExhausted; }
    public bool OutForFood { get => outForFood; }

#endregion

#region Methods


     private void Start()
     {
        //Subscribe to delegates
        TimeTracker.Instance.dNewDay += NewDay;
        TimeTracker.Instance.hourChanged += HourChange;
     }

     private void OnEnable()
     {
        //When first started, TimeTracker sometimes isnt initialized
        if(TimeTracker.Instance != null)
        {
            //Subscribe to delegates
            TimeTracker.Instance.dNewDay += NewDay;
            TimeTracker.Instance.hourChanged += HourChange;
        }
     }

     private void OnDisable()
     {
        //Unsubscribe from delegates
        TimeTracker.Instance.dNewDay -= NewDay;
        TimeTracker.Instance.hourChanged -= HourChange;
     }

    /// <summary>
    /// pulls random possible dialogue and sends it to the dialogue ui on interact
    /// </summary>
    public void PerformAction()
    {
        buttonPressed_SFX = FMODUnity.RuntimeManager.CreateInstance("event:/Interface/InGame_ButtonPressed");
        buttonPressed_SFX.start();
        GameInputSystem.Instance.OnCharacterInteract(id, possibleDialogue[Random.Range(0, possibleDialogue.Length)]);
    }
       
    /// <summary>
    /// Eat food, reduce hunger 
    /// </summary>
    public void EatFood()
    {
        //Did we successfully eat food?
        if(Resources.Instance.ConsumeFood(1))
        {
           hunger = 0;
        }
    }

    /// <summary>
    /// Drink water, curing exhaustion
    /// </summary>
    public void DrinkWater()
    {
        // //Did we successfully drink water?
        if (Resources.Instance.RemoveWater(1))
        {
            exhausted = false;
        }
    }

    /// <summary>
    /// Give this NPC medicine, curing ailments
    /// </summary>
    public void GiveMedicine()
    {
        sick = false;
    }

    /// <summary>
    /// Called via delegate for the new day
    /// </summary>
    public void NewDay()
    {
        //NPC is unconscious, dont increment data
        if (unconscious)
        {
           return;
        }

        hunger += hungerIncreasePerDay;
        thirst += thirstIncreasePerDay;

        //Sickness
        if (sick)
        {
            daysSick++;

            if(daysSick > 4)
            {
                unconscious = true;
            }
        }
        else
        {
            daysSick = 0;
        }

        //Warmth
        if (cold)
        {
            daysCold++;

            //Sick if cold for 3 days or more
            if(daysCold > 3)
            {
                sick = true;
            }
        }
        else
        {
            daysCold = 0;
        }


        //NPC sleeps every day
        exhausted = false;

        //NOTE: May need to change up exhaustion and/or hunger
        //      since 

        if(hunger >= 0.7f || thirst >= 0.7f)
        {
           exhausted = true;
        }

         /*
        //Exhaustion
        if (exhausted)
        {
            daysExhausted++;
            if (daysExhausted > 4)
            {
                unconscious = true;
            }

        }
        else
        {
            daysExhausted = 0;
        }
        */

        if(unconscious)
        {
            //Add to amount of unconscious characters
            NPCStatus.Instance.NPCUnconscious();
        }
    }

    /// <summary>
    /// Send this NPC out for food
    /// </summary>
    /// <returns>True if NPC is sent for food, false if they cant go (sick/exhausted)</returns>
    public bool SendForFood()
    {
       //Is this NPC able to get food
       //if(!sick && !exhausted && !outForFood)
       if(!outForFood)
       {
           outForFood = true;

           //Make "invisible"
           SpriteRenderer rend = GetComponent<SpriteRenderer>();
           if (rend != null)
           {
               rend.enabled = false;
           }

           //Disable collision
           Collider2D col = GetComponent<Collider2D>();
           if (col != null)
           {
               col.enabled = false;
           }
           
            messages.Add(new Message((int)TimeTracker.Instance.DaysElapsed, (int)TimeTracker.Instance.Hour, (int)TimeTracker.Instance.Minute));
           return true;
       }

       //Can't retrieve food
       return false;
       
    }

    public void ReturnFromGettingFood()
    {
        outForFood = false;

        //Make visible
        SpriteRenderer rend = this.GetComponent<SpriteRenderer>();
        if (rend != null)
        {
            rend.enabled = true;
        }

        //Enable collision
        Collider2D col = this.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        //Determine how much food was retrieved
        int amountOfFoodRetrieved = Random.Range((int)amountOfRetrievedFood.x, (int)amountOfRetrievedFood.y);
        
        //"Get" food
        Resources.Instance.AddFood(amountOfFoodRetrieved);

        messages.Add(new Message((int)TimeTracker.Instance.DaysElapsed, (int)TimeTracker.Instance.Hour, (int)TimeTracker.Instance.Minute, amountOfFoodRetrieved));
    }

    /// <summary>
    /// Triggered by time tracker delegate, allows hourly events
    /// </summary>
    /// <param name="hour">The current hour</param>
    public void HourChange(int hour)
    {
         //If Npc is out for food, return at 4pm
         if(outForFood && hour == 15)
         {
             ReturnFromGettingFood();
         }
    }

#endregion
	}

}