using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkTest : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
            MultiplayerManager.Instance.StartSolo();
    }
}