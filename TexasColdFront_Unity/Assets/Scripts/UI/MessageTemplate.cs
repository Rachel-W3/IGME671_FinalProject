using UnityEngine;
using TMPro;

namespace tcf.ui
{

/// <summary>
/// Represents a message template for the PhoneMenu to use for the messages screen
/// </summary>
public class MessageTemplate : MonoBehaviour
{
    [SerializeField] private TMP_Text       textTimestamp;
    [SerializeField] private TMP_Text       textMessage;

    /// <summary>
    /// Sets the UI elements of the message
    /// </summary>
    /// <param name="timestamp">The timestamp of the message</param>
    /// <param name="message">The content of the message</param>
    public void SetupMessage(Timestamp timestamp, string message)
    {
        textMessage.text = message;
        textTimestamp.text = string.Format("Day {0:00} : {1:00}:{2:00}", timestamp.day, timestamp.hour, timestamp.minute);
    }
}

}