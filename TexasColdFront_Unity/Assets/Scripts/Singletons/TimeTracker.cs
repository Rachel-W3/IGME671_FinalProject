using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf 
{
public class TimeTracker : MonoBehaviour
{

	#region Fields

    [Tooltip("How long each day should last in (realtime) minutes")]
	[SerializeField] float targetDayLength = 1.0f;

    //What % of the day has elapsed
    float timeOfDay = 0;
    float daysElapsed = 0;

    //Calculated at start, how fast to increment time to meet
    // desired day length
    float timeScale;

    //Current Hour (24 hour clock, 0 through 23) 
    //hour 0 = 12am, hour 23 = 11pm
    float hour;
    //Current minute
    float minute;

    float lastHour;

    [SerializeField]
    AnimationCurve darknessCurve;

    [SerializeField] SpriteRenderer darknessMain;
    [SerializeField] SpriteRenderer darknessSecondary;
    //Singleton
    private static TimeTracker instance;
    public static TimeTracker Instance { get => instance; }

    //Properties
    public float Hour { get => hour; }
    public float Minute { get => minute;}
    public float DaysElapsed { get => daysElapsed; }

    #endregion

    //Delegates/events
    public delegate void newDay();
    public newDay dNewDay;

    public delegate void timeChange(int hour);
    public timeChange hourChanged;

    public delegate void timeSkip(float timeSkipped);
    public timeSkip timeSkipped;

    private void Awake()
    {
        //Set singleton
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        //How fast should time elapse to meet target (realtime minutes)
        timeScale = 24.0f / (targetDayLength / 60.0f);
        lastHour = hour;

        SetTime(6, false);
        SetTime(12,true);
    }

    // Update is called once per frame
    void Update()
    {
    if (GameStateMachine.Instance.CurrentState == GameState.GAME || GameStateMachine.Instance.CurrentState == GameState.MINIGAME)
    { 
        //Increment time of day using calculated timescale
        timeOfDay += Time.deltaTime * timeScale / 86400.0f;

        //New day
        if(timeOfDay >=1.0f)
        {
            timeOfDay = 0.0f;
            NewDay();
        }

        //Update minute and second
        hour = Mathf.Floor(timeOfDay * 24.0f);
        minute = Mathf.Floor(((timeOfDay * 24.0f) - hour) * 60.0f);

        //"Show" Darkness
        darknessMain.color = new Color(darknessMain.color.r, darknessMain.color.g, darknessMain.color.b,darknessCurve.Evaluate(timeOfDay));
        darknessSecondary.color = new Color(darknessMain.color.r, darknessMain.color.g, darknessMain.color.b, darknessCurve.Evaluate(timeOfDay) * 0.5f);

        //Check if we are in a new hour
        if (lastHour != hour)
        {
            hourChanged?.Invoke((int)hour);
            lastHour = hour;
        }
       
        //Check if day is over
        if (timeOfDay >= 1.0f)
        {
            //New Day
            NewDay();
        }
    }
    }


    /// <summary>
    /// Player sleeps
    /// </summary>
    public void Sleep()
    {
        //Sleep overnight
        if(hour > 6)
        {
            SetTime(6, true);
        }

        //Sleep until morning
        else
        {
            SetTime(6, false);
        }

        GameplayStats.Instance.PlayerSlept();
    }

    //Trigger a new day
    void NewDay()
    {
        daysElapsed++; //Increment days
        
        //Call delegate if it has valid subscribers (use of '?')
        dNewDay?.Invoke();

        if(daysElapsed >= 15)
        {
            GameplayStats.Instance.GameOver(true);
        }
    }

    /// <summary>
    /// Set the time of day
    /// </summary>
    /// <param name="hour">Hour to set the time to</param>
    /// <param name="minute">Minute to set the time to</param>
    /// <param name="isNewDay">Will a new day begin</param>
    public void SetTime(int hour, int minute, bool isNewDay)
    {
        float secondsSkipped;

        float oldTimeOfDay = timeOfDay;

        float newTimeOfDay = ((1.0f / 24.0f) * hour) + ((1.0f / 24.0f) / 60.0f * minute);

        secondsSkipped = (newTimeOfDay - oldTimeOfDay) * timeScale / 60.0f / 24.0f;

        if (isNewDay)
        {
            //Take the new day into consideration
            secondsSkipped = (1.0f - oldTimeOfDay) * timeScale / 60.0f / 24.0f;//Seconds skipped to end of first day
            secondsSkipped += (1.0f - newTimeOfDay) * timeScale / 60.0f / 24.0f; //Seconds skipped from start of today to now;
            NewDay();
        }

        timeOfDay = newTimeOfDay; //update time of day
        print(secondsSkipped.ToString());

        //Trigger delegate
        timeSkipped?.Invoke(secondsSkipped);
    }

    //Override using only hour
    /// <summary>
    /// Set the time of day
    /// </summary>
    /// <param name="hour">Hour to set the time to</param>
    /// <param name="isNewDay">Will a new day begin</param>
    public void SetTime(int hour, bool isNewDay)
    {
        float secondsSkipped;

        float oldTimeOfDay = timeOfDay;

        float newTimeOfDay = (1.0f / 24.0f) * hour;

        secondsSkipped = (newTimeOfDay - oldTimeOfDay) * timeScale / 60.0f / 24.0f;

        //Increment day?
        if (isNewDay)
        {
            //Take the new day into consideration
            secondsSkipped = (1.0f - oldTimeOfDay) * timeScale / 60.0f / 24.0f; //Seconds skipped to end of first day
            secondsSkipped += (1.0f - newTimeOfDay) * timeScale / 60.0f / 24.0f;//Seconds skipped from start of today to now
            NewDay();
        }

        timeOfDay = newTimeOfDay;

        //Trigger delegate
        timeSkipped?.Invoke(secondsSkipped);
    }
    
    /// <summary>
    /// Gets the current time as a string
    /// </summary>
    /// <returns>A string with the hour and minute in the form HOUR:MINUTE</returns>
    public string GetTimeAsString()
    {
        return hour.ToString("00") + ":" + minute.ToString("00");
    }
}
}