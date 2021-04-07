using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace tcf.obj
{

/// <summary>
/// Class which tracks the statuses of NPCs
/// </summary>
public class NPCStatus : MonoBehaviour
{
    [SerializeField] private NonPlayerCharacter[] characters;

    /// <summary>
    /// NPCStatus is a singleton accessible by this static Instance
    /// </summary>
    public static NPCStatus Instance { get; private set; }
    public NonPlayerCharacter[] Characters { get => characters;}


    int charactersUnconscious = 0;

    /// <summary>
        /// set singleton instance on unity awake
        /// </summary>
    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;

        //Delegate Connections
        
    }

    private void Start()
    {
        TimeTracker.Instance.hourChanged += NewHour;
    }
    private void OnDisable()
    {
       TimeTracker.Instance.hourChanged -= NewHour;
    }

    /// <summary>
    /// Gets the Exhausted status of NPC "npc"
    /// </summary>
    /// <param name="npc">The NPC whose status is being obtained</param>
    /// <returns>Whether the npc is exhausted or not</returns>
    public bool GetExhausted(NPC npc)
    {
        return characters.FirstOrDefault(c => c.ID == npc).Exhausted;
    }

    /// <summary>
    /// Gets the Sick status of NPC "npc"
    /// </summary>
    /// <param name="npc">The NPC whose status is being obtained</param>
    /// <returns>Whether the npc is sick or not</returns>
    public bool GetSick(NPC npc)
    {
        return characters.FirstOrDefault(c => c.ID == npc).Sick;
    }

    /// <summary>
    /// Gets the Cold status of NPC "npc"
    /// </summary>
    /// <param name="npc">The NPC whose status is being obtained</param>
    /// <returns>Whether the npc is cold or not</returns>
    public bool GetCold(NPC npc)
    {
        return characters.FirstOrDefault(c => c.ID == npc).Cold;
    }

    /// <summary>
    /// Gets the Messages of NPC "npc"
    /// </summary>
    /// <param name="npc">The NPC to obtain messages from</param>
    /// <returns>The NPC's current list of messages</returns>
    public Message[] GetMessages(NPC npc)
    {
        return characters.FirstOrDefault(c => c.ID == npc).Messages.ToArray();
    }


    public void NPCUnconscious()
    {
        charactersUnconscious++;

        if(charactersUnconscious >= 3)
        {
            GameplayStats.Instance.GameOver(false);
        }
    }

    public void NewHour(int hour)
    {
        if(Resources.Instance.FoodAmount <= 4)
        {
            //6am
            if(hour == 5)
            {
                //Send husband for food
                characters[0].SendForFood(); //NOTE: Maybe change characters to dictionary?
            }
          
        }
        else
        {
            //6am
            if (hour == 5)
            {
               //Let everyone eat
               for (int i = 0; i < characters.Length; i++)
               {
                    characters[i].EatFood();
                    characters[i].DrinkWater();
               }
            }
        }
    }
}

}