using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf.controller
{
    public class Footsteps : MonoBehaviour
    {
        private FMOD.Studio.EventInstance footstep;
        private CharacterController characterController;
        private bool isPlayerMoving;
        private float movingSpeed;

        float timer = 0.0f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            movingSpeed = 0.38f;
        }

        private void Start()
        {
            footstep = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Footsteps");
            InvokeRepeating("PlayFootsteps", 0.0f, movingSpeed);
        }

        // Update is called once per frame
        void Update()
        {
            if(!characterController.LeftUp || !characterController.RightUp)
            {
                isPlayerMoving = true;
            }
            else
            {
                isPlayerMoving = false;
            }
        }

        /// <summary>
        /// Play FMOD event "Footsteps" if player is moving
        /// </summary>
        private void PlayFootsteps()
        {
            if (isPlayerMoving)
            {
                footstep.start();
            }
        }
    }
}
