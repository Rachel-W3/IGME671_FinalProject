using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeTest : MonoBehaviour
{
    public FMODUnity.StudioEventEmitter emitter;
    private FMOD.Studio.EventInstance eventInstance;
    [Range(0.0f, 10.0f)] public float volume;

    private void Start()
    {
       eventInstance = emitter.EventInstance;
       eventInstance.getVolume(out volume);
    }

    private void Update()
    {
        eventInstance.setVolume(volume);
    }
}
