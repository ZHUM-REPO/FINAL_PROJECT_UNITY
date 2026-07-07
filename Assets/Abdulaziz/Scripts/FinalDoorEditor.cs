#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a "Collect All Items" button in the FinalDoor Inspector, below the normal
/// fields — same style as the CubePedestal button. Clicking it collects every
/// required ShapeCollectible (as if the player picked each up), so the door's
/// requirement is met. Use it in Play mode.
/// </summary>
[CustomEditor(typeof(FinalDoor))]
public class FinalDoorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the usual serialized fields first.
        DrawDefaultInspector();

        FinalDoor door = (FinalDoor)target;

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Collect All Items"))
                CollectAll(door);
        }

        // Live status readout.
        if (Application.isPlaying)
        {
            if (door.AllCollected())
                EditorGUILayout.HelpBox("All required items are collected — the door will open.", MessageType.Info);
            else
                EditorGUILayout.HelpBox("Not all items collected yet — the door will send the player to spawn.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play mode to collect items.", MessageType.Info);
        }
    }

    // Collect every required item, exactly as the player would by interacting.
    private void CollectAll(FinalDoor door)
    {
        ShapeCollectible[] items = door.RequiredItems;

        if (items == null || items.Length == 0)
        {
            Debug.LogWarning($"{door.name}: no required items assigned — nothing to collect.", door);
            return;
        }

        int collected = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                Debug.LogWarning($"{door.name}: required item element {i} is empty.", door);
                continue;
            }

            if (!items[i].IsCollected)
            {
                items[i].Interact();   // same as the player picking it up
                collected++;
            }
        }

        if (door.AllCollected())
            Debug.Log($"{door.name}: collected {collected} item(s) — all required items are now collected, door will open.", door);
        else
            Debug.LogWarning($"{door.name}: collected {collected} item(s), but some slots are empty — assign every required item.", door);
    }
}
#endif