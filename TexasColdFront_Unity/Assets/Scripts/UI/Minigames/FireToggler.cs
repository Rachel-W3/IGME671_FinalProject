using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf.ui
{
    public class FireToggler : MonoBehaviour
    {
        [SerializeField] private GameObject                   fireSprite;
        /**************/ private FMODUnity.StudioEventEmitter emitter;

        // Start is called before the first frame update
        void Start()
        {
            emitter = fireSprite.GetComponent<FMODUnity.StudioEventEmitter>();
            fireSprite.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Resources.Instance.FuelAmount > 0)
                fireSprite.SetActive(true);
            else
                fireSprite.SetActive(false);
            emitter.SetParameter("fuelLevel", Resources.Instance.FuelAmount);
        }
    }
}
