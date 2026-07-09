#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a real clickable "Rotate Cube (Interact)" button in the CubePedestal
/// Inspector, below the normal fields. Editor-only, so it won't affect builds.
/// Calls ForceRotate(), which bypasses the proximity check so you can test the
/// turn without walking the player into range. Rotation is a coroutine, so the
/// button only animates in Play mode.
/// </summary>
[CustomEditor(typeof(CubePedestal))]
public class CubePedestalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the usual serialized fields first.
        DrawDefaultInspector();

        CubePedestal pedestal = (CubePedestal)target;

        EditorGUILayout.Space();

        // The button is only useful in Play mode (coroutines don't run in edit mode).
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Rotate Cube (Interact)"))
                pedestal.ForceRotate();
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play mode to use this button.", MessageType.Info);
    }
}
#endif