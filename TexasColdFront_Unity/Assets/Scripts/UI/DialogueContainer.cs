using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace tcf.ui
{

/// <summary>
/// Used for creating dialogue popups that disappear over time
/// </summary>
public class DialogueContainer : MonoBehaviour
{
    [SerializeField] private float      durationFactor;
    [SerializeField] private TMP_Text   textDialogue;
    [SerializeField] private TMP_Text   textName;
    /**************/ private bool       initialized;

    /// <summary>
    /// Initializes the container instance and starts destroy timer
    /// </summary>
    /// <param name="text">The dialogue text</param>
    /// <param name="name">The nameplate text</param>
    public void Initialize(string text, string name)
    {
        if (!initialized) {
            initialized = true;
            textName.text = name;
            textDialogue.text = text;
            int amountOfText = text.Split(' ').Length;
            StartCoroutine(DestroyAfterTimer(durationFactor * amountOfText));
        }
    }

    /// <summary>
    /// Destroys the container after WaitForSeconds
    /// </summary>
    /// <param name="timer">The time to wait until destruction, used with WaitForSeconds</param>
    private IEnumerator DestroyAfterTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        Destroy(gameObject);
    }
}

}