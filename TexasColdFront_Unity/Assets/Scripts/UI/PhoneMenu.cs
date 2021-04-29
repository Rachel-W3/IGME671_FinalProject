using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using tcf.obj;

namespace tcf.ui
{
/// <summary>
/// Defines structure for the UI state of the game's phone menu
/// </summary>
public enum PhoneState
{
    HOME = 0,
    CONTACTS = 1,
    MESSAGES = 2,
    TASKLIST = 3,
    NEWS = 4,
}

/// <summary>
/// Object which handles functions related to the game's phone menu
/// </summary>
public class PhoneMenu : MonoBehaviour
{
    /// <summary>
    /// Defines structure for a message contact on the contact menu
    /// </summary>
    [Serializable] protected struct PhoneContact 
    {
        public Button           button;
        public NPC              id;
        public TMP_Text         status;
    }

    /// <summary>
    /// Defines identifier for tasks
    /// </summary>
    public enum TaskType
    {
        WATER_BUCKET_SNOW,
        WATER_BUCKET_BOIL,
        WATER_STOCKPILE_STATUS,

        FIRE_FUEL_GATHER,
        FIRE_FUEL_STATUS,
    }

    /// <summary>
    /// Defines structure for referencing tasks
    /// </summary>
    [Serializable] protected struct Task
    {
        public Toggle   taskCheck;
        public TMP_Text taskProgressText;
        public Slider   taskSlider;
        public TaskType taskType;
    }

    /// <summary>
    /// Defines structure for news items
    /// </summary>
    [Serializable] protected struct NewsItem
    {
        [TextArea(6,8)]
        public string title;

        [TextArea(16,64)]
        public string body;
    }

#pragma warning disable 0649
    [SerializeField] private RectTransform                      containerContacts;
    [SerializeField] private RectTransform                      containerHome;
    [SerializeField] private RectTransform                      containerMessages;
    [SerializeField] private RectTransform                      containerNews;
    [SerializeField] private RectTransform                      containerPrimary;
    [SerializeField] private RectTransform                      containerTasklist;
    [SerializeField] private Button                             homeButton;
    [SerializeField] private Button                             homeNavContacts;
    [SerializeField] private Button                             homeNavMessages;
    [SerializeField] private Button                             homeNavNews;
    [SerializeField] private Button                             homeNavTasklist;
    [SerializeField] private Button                             messagesBack;
    [SerializeField] private TMP_Text                           messagesHeading;
    [SerializeField] private Image                              messagesIcon;
    [SerializeField] private TMP_Text                           newsBody;
    [SerializeField] private TMP_Text                           newsTitle;
    [SerializeField] private PhoneContact[]                     contacts;
    [SerializeField] private NewsItem[]                         newsItems;
    [SerializeField] private Task[]                             tasklist;
    [SerializeField] private MessageTemplate                    templateMessageIn;
    [SerializeField] private TMP_Text                           timeIndicator;
    [SerializeField] private TransitionObject                   transitionContainerState;
#pragma warning restore 0649
    /**************/ private PhoneState                         currentState;
    /**************/ private NPC                                currentCharacterForMessages;
    /**************/ private IEnumerator                        transitionContainerInstance;
    /**************/ private const float                        messagePadding = 20.0f;
    /**************/ private const string                       statusCold      = "<color=#99f>Cold</color>\n";
    /**************/ private const string                       statusExhausted = "<color=#f99>Exhausted</color>\n";
    /**************/ private const string                       statusFine      = "<color=#ff9>Fine</color>";
    /**************/ private const string                       statusHead      = "Status: ";
    /**************/ private const string                       statusSick      = "<color=#9f9>Sick</color>\n";
    /**************/ private FMOD.Studio.EventInstance          phoneTapped_sfx;
    /**************/ private FMOD.Studio.EventInstance          togglePhone_sfx;

    private void Awake()
    {
        phoneTapped_sfx = FMODUnity.RuntimeManager.CreateInstance("event:/Interface/InGame_ButtonPressed");
        togglePhone_sfx   = FMODUnity.RuntimeManager.CreateInstance("event:/Interface/PhoneToggling");
    }

    /// <summary>
    /// disable container and setup ui on start
    /// </summary>
    private void Start()
    {
        StartCoroutine(DelayedStart());

        #region setup button listeners
        homeButton.onClick.AddListener(() => {
            UpdatePhoneState(PhoneState.HOME);
        });

        homeNavContacts.onClick.AddListener(() => {
            UpdatePhoneState(PhoneState.CONTACTS);
        });

        homeNavMessages.onClick.AddListener(() => {
            UpdatePhoneState(PhoneState.MESSAGES);
        });

        homeNavNews.onClick.AddListener(() => {
            UpdatePhoneState(PhoneState.NEWS);
        });

        homeNavTasklist.onClick.AddListener(() => {
            UpdatePhoneState(PhoneState.TASKLIST);
        });

        messagesBack.onClick.AddListener(() => {
            UpdatePhoneState(PhoneState.CONTACTS);
        });

        /*
        for(int i = 0; i < contacts.Length; i++)
        {
            NPC id = contacts[i].id;
            contacts[i].button.onClick.AddListener(() => {
                currentCharacterForMessages = id;
                UpdateMessages();
                UpdatePhoneState(PhoneState.MESSAGES);
            });
        }
        */

        contacts.First(c => c.id == NPC.HUSBAND).button.onClick.AddListener(() => {
            currentCharacterForMessages = NPC.HUSBAND;
            UpdateMessages();
            UpdatePhoneState(PhoneState.MESSAGES);
        });
        
        #endregion
    }

    /// <summary>
    /// event subscribe on unity enable
    /// </summary>
    private void OnEnable()
    {
        GameStateMachine.GameStateChanged += OnGameStateChanged;
    }

    /// <summary>
    /// event unsubscribe on unity disable
    /// </summary>
    private void OnDisable()
    {
        GameStateMachine.GameStateChanged -= OnGameStateChanged;
    }
    
    /// <summary>
    /// Handler for GameStateMachine.GameStateChanged, toggles the menu
    /// </summary>
    /// <param name="prev">the previous GameState</param>
    /// <param name="next">the current/new GameState</param>
    private void OnGameStateChanged(GameState prev, GameState next)
    {
        if (transitionContainerInstance != null)
            StopCoroutine(transitionContainerInstance);

        switch (next)
        {
            case GameState.PHONE:
                togglePhone_sfx.setParameterByName("TogglePhone", 0); // Play unlock phone sfx
                UpdatePhoneState(PhoneState.HOME);
                timeIndicator.text = "Day " + TimeTracker.Instance.DaysElapsed + "   " + 
                                     string.Format("{0:00}:{1:00}", TimeTracker.Instance.Hour, TimeTracker.Instance.Minute);
                transitionContainerInstance = TransitionContainer(true);
                StartCoroutine(transitionContainerInstance);
                togglePhone_sfx.start();
                break;
            default:
                if (prev == GameState.PHONE) 
                {
                    togglePhone_sfx.setParameterByName("TogglePhone", 1); // Play lock phone sfx
                    transitionContainerInstance = TransitionContainer(false);
                    StartCoroutine(transitionContainerInstance);
                    togglePhone_sfx.start();
                }
                else if (containerPrimary.gameObject.activeInHierarchy)
                    containerPrimary.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Updates the current phone UI state
    /// </summary>
    /// <param name="next">The state to update to</param>
    private void UpdatePhoneState(PhoneState next)
    {
        phoneTapped_sfx.start();
        switch (next)
        {
            case PhoneState.CONTACTS:
                // get current status information
                UpdateContacts();

                containerHome.gameObject.SetActive(false);
                containerContacts.gameObject.SetActive(true);
                containerMessages.gameObject.SetActive(false);
                containerNews.gameObject.SetActive(false);
                containerTasklist.gameObject.SetActive(false);
                break;
            case PhoneState.MESSAGES:
                // get current messages information based on current character
                // the current character isn't updated from the messages icon, but from the contacts buttons
                UpdateMessages();

                containerHome.gameObject.SetActive(false);
                containerContacts.gameObject.SetActive(false);
                containerMessages.gameObject.SetActive(true);
                containerNews.gameObject.SetActive(false);
                containerTasklist.gameObject.SetActive(false);
                break;
            case PhoneState.NEWS:
                // get random news
                UpdateNews();

                containerHome.gameObject.SetActive(false);
                containerContacts.gameObject.SetActive(false);
                containerMessages.gameObject.SetActive(false);
                containerNews.gameObject.SetActive(true);
                containerTasklist.gameObject.SetActive(false);
                break;
            case PhoneState.TASKLIST:
                // get current task information
                UpdateTaskProgress();

                containerHome.gameObject.SetActive(false);
                containerContacts.gameObject.SetActive(false);
                containerMessages.gameObject.SetActive(false);
                containerNews.gameObject.SetActive(false);
                containerTasklist.gameObject.SetActive(true);
                break;
            case PhoneState.HOME:
            default:
                containerHome.gameObject.SetActive(true);
                containerContacts.gameObject.SetActive(false);
                containerMessages.gameObject.SetActive(false);
                containerNews.gameObject.SetActive(false);
                containerTasklist.gameObject.SetActive(false);
                break;
        }

        currentState = next;
    }

    /// <summary>
    /// Updates tasklist UI
    /// </summary>
    private void UpdateTaskProgress()
    {
        for(int i = 0; i < tasklist.Length; i++)
        {
            // set ui based on the task and its associated rules for progression (found within the SetIsOn/SetValue)
            // use WithoutNotify to avoid firing UI events; there shouldn't be subscribers to these elements, but just in case, do this properly
            switch(tasklist[i].taskType)
            {
                case TaskType.WATER_BUCKET_SNOW:
                    tasklist[i].taskCheck.SetIsOnWithoutNotify(MinigameUI.Instance.BucketFillState != BucketState.EMPTY);
                    break;
                case TaskType.WATER_BUCKET_BOIL:
                    tasklist[i].taskCheck.SetIsOnWithoutNotify(Resources.Instance.WaterAmount >= Constants.WATER_PER_BUCKET);
                    break;
                case TaskType.WATER_STOCKPILE_STATUS:
                    tasklist[i].taskCheck.SetIsOnWithoutNotify(Resources.Instance.WaterAmount >= Constants.WATER_PER_BUCKET);
                    tasklist[i].taskSlider.SetValueWithoutNotify(Resources.Instance.WaterAmount / Constants.WATER_PER_BUCKET);
                    tasklist[i].taskProgressText.text = string.Format("{0:00.00} / {1:00.00}", Resources.Instance.WaterAmount, Constants.WATER_PER_BUCKET);
                    break;
                case TaskType.FIRE_FUEL_GATHER:
                    tasklist[i].taskCheck.SetIsOnWithoutNotify(MinigameUI.Instance.FurniturePieceCount > 0);
                    break;
                case TaskType.FIRE_FUEL_STATUS:
                    tasklist[i].taskCheck.SetIsOnWithoutNotify(Resources.Instance.FuelAmount > FireRefueling.fuelPerPiece);
                    tasklist[i].taskSlider.SetValueWithoutNotify((Resources.Instance.Temperature - Resources.MinTemp) / (Resources.MaxTemp - Resources.MinTemp));
                    tasklist[i].taskProgressText.text = string.Format("{0:00.0}\u00B0", Resources.Instance.Temperature);
                    break;
            }
        }
    }

    /// <summary>
    /// Updates contacts UI
    /// </summary>
    private void UpdateContacts()
    {
        foreach(PhoneContact contact in contacts)
        {
            // reset status text
            contact.status.text = statusHead;

            // get statuses
            bool cold = NPCStatus.Instance.GetCold(contact.id);
            bool exhausted = NPCStatus.Instance.GetExhausted(contact.id);
            bool sick = NPCStatus.Instance.GetSick(contact.id);

            // append status text
            if (cold) contact.status.text += statusCold;
            if (exhausted) contact.status.text += statusExhausted;
            if (sick) contact.status.text += statusSick;
            if (!cold && !exhausted && !sick) contact.status.text += statusFine;
        }
    }

    /// <summary>
    /// Updates messages UI
    /// </summary>
    private void UpdateMessages()
    {
        // set heading of messages screen and get messages from current character
        messagesHeading.text = Constants.CharacterIDToString(currentCharacterForMessages);
        Message[] messages = NPCStatus.Instance.GetMessages(currentCharacterForMessages);

        // get parent of template and clear it
        RectTransform parent = templateMessageIn.transform.parent.GetComponent<RectTransform>();
        for (int i = parent.childCount - 1; i > -1; i--)
            if (parent.GetChild(i).CompareTag("message"))
                Destroy(parent.GetChild(i).gameObject);

        // create new message boxes from messages array
        for (int i = 0; i < messages.Length; i++)
        {
            // instantiate and set active/tag
            GameObject instance = Instantiate(templateMessageIn.gameObject, parent);
            instance.SetActive(true);
            instance.tag = "message";

            // get transform and set position
            RectTransform instanceRT = instance.GetComponent<RectTransform>();
            instanceRT.anchoredPosition = new Vector2(0, -(((instanceRT.sizeDelta.y + messagePadding) * i) + (instanceRT.sizeDelta.y / 2)));

            // set message contents
            MessageTemplate instanceTP = instance.GetComponent<MessageTemplate>();
            instanceTP.SetupMessage(messages[i].timestamp, messages[i].message);
        }

        // resize the container so that scrolling works properly
        parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (templateMessageIn.GetComponent<RectTransform>().sizeDelta.y + 20) * (messages.Length + 1));
    }

    /// <summary>
    /// Updates news UI
    /// </summary>
    private void UpdateNews()
    {
        int rng = UnityEngine.Random.Range(0, newsItems.Length);
        newsBody.text = newsItems[rng].body;
        newsTitle.text = newsItems[rng].title;
    }

    /// <summary>
    /// Coroutine for delaying the disabling of the phone menu
    /// </summary>
    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        transitionContainerInstance = TransitionContainer(false);
        StartCoroutine(transitionContainerInstance);
    }

    /// <summary>
    /// Coroutine for transitioning the phone menu container position
    /// </summary>
    /// <param name="state">the state the container should end on</param>
    private IEnumerator TransitionContainer(bool state)
    {
        // define position states
        Vector2 posDown = new Vector2(0, -1000);
        Vector2 posUp = new Vector2(0, 0);

        // initialize container for animation
        containerPrimary.gameObject.SetActive(true);
        containerPrimary.anchoredPosition = state ? posDown : posUp;
        float t = 0;

        // loop for animation
        while (t <= transitionContainerState.animationLength)
        {
            yield return new WaitForEndOfFrame();
            t += Time.unscaledDeltaTime;
            containerPrimary.anchoredPosition = state ?
                Vector2.Lerp(posDown, posUp, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength)) :
                Vector2.Lerp(posUp, posDown, transitionContainerState.animationCurve.Evaluate(t / transitionContainerState.animationLength));
        }

        // ensure alpha is fully set (b/c of dt animation may not perfectly set to 1) if active, or deactivate in hierarchy if inactive
        if (state)
        {
            containerPrimary.anchoredPosition = posUp;
        }
        else
        {
            containerPrimary.gameObject.SetActive(false);
        }
    }
}

}