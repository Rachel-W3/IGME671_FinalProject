using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf.ui
{
    public class FireToggler : MonoBehaviour
    {
        [SerializeField] private GameObject                       fireSprite;
        /**************/ private FMODUnity.StudioEventEmitter     fireCrackling_emitter;
        /**************/ private FMOD.Studio.EventInstance        fireCrackling_Ambience;

        private void Awake()
        {
            //fireCrackling_Ambience = FMODUnity.RuntimeManager.CreateInstance("event:/Ambience/FireCrackling");
        }

        // Start is called before the first frame update
        void Start()
        {
            fireCrackling_emitter = fireSprite.GetComponent<FMODUnity.StudioEventEmitter>();
            fireSprite.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (  GameStateMachine.Instance.CurrentState == GameState.MENU
               || GameStateMachine.Instance.CurrentState == GameState.BADWIN
               || GameStateMachine.Instance.CurrentState == GameState.GOODWIN)
                fireSprite.SetActive(false);
            
            if (Resources.Instance.FuelAmount > 0)
                fireSprite.SetActive(true);
            else
                fireSprite.SetActive(false);
            //fireCrackling_Ambience.setParameterByName("fuelLevel", Resources.Instance.FuelAmount);
            fireCrackling_emitter.SetParameter("fuelLevel", Resources.Instance.FuelAmount);
        }
    }
}
