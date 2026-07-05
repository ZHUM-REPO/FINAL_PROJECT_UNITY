using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class TextChatManager : MonoBehaviour
{
    public static TextChatManager Instance;

    [Header("UI References")]
    public GameObject chatPanel;
    public Transform messageContainer;
    public GameObject messagePrefab;
    public TMP_InputField chatInput;
    public ScrollRect scrollRect;

    [Header("Settings")]
    public int maxMessages = 50;

    private bool isChatOpen = false;
    private List<GameObject> messages = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        chatPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            ToggleChat();

        if (isChatOpen && Keyboard.current.enterKey.wasPressedThisFrame)
            TrySendMessage();
    }

    // ─── Toggle ───────────────────────────────────────────

    private void ToggleChat()
    {
        isChatOpen = !isChatOpen;
        chatPanel.SetActive(isChatOpen);

        if (isChatOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            chatInput.ActivateInputField();
            ScrollToBottom();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            chatInput.DeactivateInputField();
            chatInput.text = "";
        }
    }

    // ─── Send ─────────────────────────────────────────────

    private void TrySendMessage()
    {
        string text = chatInput.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        string playerName = $"Player {NetworkManager.Singleton.LocalClientId + 1}";
        string fullMessage = $"[{playerName}]: {text}";

        // use NetworkManager's custom messaging or just broadcast locally
        // and send via a networked object that already exists
        ChatNetworkHelper.Instance?.SendChatMessage(fullMessage);

        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    // ─── Display (called by ChatNetworkHelper) ────────────

    public void DisplayMessage(string message)
    {
        if (messages.Count >= maxMessages)
        {
            Destroy(messages[0]);
            messages.RemoveAt(0);
        }

        GameObject msg = Instantiate(messagePrefab, messageContainer);
        TextMeshProUGUI text = msg.GetComponent<TextMeshProUGUI>();
        if (text != null) text.text = message;

        messages.Add(msg);
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}