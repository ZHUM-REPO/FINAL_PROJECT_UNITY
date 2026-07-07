#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a real clickable "Collect Item (Interact)" button in the ShapeCollectible
/// Inspector, below the normal fields. Editor-only, so it won't affect builds.
/// Enter Play mode to use it — the same as the CubePedestal button.
/// </summary>
[CustomEditor(typeof(ShapeCollectible))]
public class ShapeCollectibleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the usual serialized fields first.
        DrawDefaultInspector();

        ShapeCollectible collectible = (ShapeCollectible)target;

        EditorGUILayout.Space();

        // Only useful in Play mode: interacting fires events and hides the object.
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Collect Item (Interact)"))
                collectible.Interact();
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play mode to use this button.", MessageType.Info);
    }
}
#endif