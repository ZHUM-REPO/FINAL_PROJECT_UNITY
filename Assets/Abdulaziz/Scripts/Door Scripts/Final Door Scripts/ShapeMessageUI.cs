using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows a message for a few seconds whenever the FinalDoor denies the player.
/// Lives on the UI canvas; subscribes to FinalDoor.Denied.
/// </summary>
public class ShapeMessageUI : MonoBehaviour
{
    [Tooltip("The FinalDoor to listen to.")]
    [SerializeField] private FinalDoor door;

    [Tooltip("The text element that displays the message.")]
    [SerializeField] private TMP_Text messageText;

    [Tooltip("What to say when the player is missing shapes.")]
    [SerializeField] private string message = "You did not collect all the poker shapes!";

    [Tooltip("How long (seconds) the message stays on screen.")]
    [SerializeField] private float displayTime = 3f;

    private Coroutine _hideRoutine;

    private void OnEnable()
    {
        door.Denied += ShowMessage;
        messageText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        door.Denied -= ShowMessage;
    }

    private void ShowMessage()
    {
        // Restart the timer if the player bumps the door again mid-message.
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);

        messageText.text = message;
        messageText.gameObject.SetActive(true);
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        messageText.gameObject.SetActive(false);
        _hideRoutine = null;
    }
}