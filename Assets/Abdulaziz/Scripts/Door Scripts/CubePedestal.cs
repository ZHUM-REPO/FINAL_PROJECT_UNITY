using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>The card faces the cube can land on. Order matters:
/// it maps 1-to-1 onto CubePedestal's Face Rotations array.</summary>
public enum CubeFace
{
    Club = 0,
    Diamond = 1
}

/// <summary>
/// Rotating pedestal. Interaction runs through PlayerInputs.OnInteractInput (the
/// permanent input event) plus a proximity check — walk close and press Interact,
/// same pattern as LevelHolder. No dependency on a raycast interactor.
/// </summary>
public class CubePedestal : MonoBehaviour
{
    [Header("Cube")]
    [Tooltip("The cube object on the pedestal that visually rotates.")]
    [SerializeField] private Transform _cube;

    [Tooltip("How fast (deg/sec) the cube turns to its new facing.")]
    [SerializeField] private float _rotationSpeed = 360f;

    [Header("Faces")]
    [Tooltip("One local rotation per face, in CubeFace order: " +
             "element 0 = the cube's Inspector rotation when the CLUB faces the player, " +
             "element 1 = the rotation when the DIAMOND faces the player.")]
    [SerializeField] private Vector3[] _faceRotations;

    [Header("Current Shape (indicator)")]
    [Tooltip("Shows which shape currently faces the player, and sets the starting " +
             "shape. Watch it flip each time you interact. It must match the shape " +
             "the cube actually shows at start, or the doors will be off by one.")]
    [SerializeField] private CubeFace _currentFace = CubeFace.Club;

    [Header("Interaction")]
    [Tooltip("How close the local player must be to interact with the pedestal.")]
    [SerializeField] private float interactRange = 3f;

    [Tooltip("Optional 'Press E' prompt, hidden until the player is in range.")]
    [SerializeField] private GameObject interactPrompt;

    private Transform localPlayer;
    private bool inRange = false;

    private bool _isRotating = false;
    private Quaternion _start, _target;
    private float _angle;

    // How many shapes exist (Club, Diamond) — used to cycle, so the array length
    // can't accidentally pin the face on one shape.
    private static readonly int FaceCount = Enum.GetValues(typeof(CubeFace)).Length;

    // Fired once the cube has snapped to its final facing and the selection is locked in.
    public event Action<CubeFace> FaceChanged;

    /// <summary>The card face currently shown to the player. The doors read this.</summary>
    public CubeFace CurrentFace => _currentFace;

    private void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Make the cube actually show the indicated starting face.
        if (_cube != null && _faceRotations != null && (int)_currentFace < _faceRotations.Length)
            _cube.localRotation = Quaternion.Euler(_faceRotations[(int)_currentFace]);

        FaceChanged?.Invoke(_currentFace);
    }

    // Subscribe to the permanent interact event in OnEnable, unsubscribe in OnDisable.
    private void OnEnable()  { PlayerInputs.OnInteractInput += HandleInteract; }
    private void OnDisable() { PlayerInputs.OnInteractInput -= HandleInteract; }

    private void Update()
    {
        // Find the local player once it exists, then track proximity each frame.
        if (localPlayer == null) { localPlayer = FindLocalPlayer(); return; }

        float dist = Vector3.Distance(localPlayer.position, transform.position);
        bool nowInRange = dist <= interactRange;

        if (nowInRange != inRange)
        {
            inRange = nowInRange;
            if (interactPrompt != null) interactPrompt.SetActive(inRange);
        }
    }

    // Fired by PlayerInputs when the local player presses Interact.
    private void HandleInteract()
    {
        if (!inRange) return;
        Rotate();
    }

    /// <summary>Rotate to the next face, ignoring proximity. Used by the editor
    /// test button so it works without walking the player into range.</summary>
    public void ForceRotate() => Rotate();

    // Advance the face and start the turn. Shared by the interact event and the
    // editor button.
    private void Rotate()
    {
        if (_isRotating) return;

        if (_faceRotations == null || _faceRotations.Length < FaceCount)
        {
            Debug.LogWarning($"{name}: Face Rotations needs {FaceCount} entries (one per shape).", this);
            return;
        }

        // Advance to the next shape. Cycling on FaceCount (not array length) means
        // the face ALWAYS changes, so the doors always see the new selection.
        _currentFace = (CubeFace)(((int)_currentFace + 1) % FaceCount);
        StartCoroutine(RotateCube());
    }

    private IEnumerator RotateCube()
    {
        _isRotating = true;
        _start = _cube.localRotation;
        // Absolute target: the exact rotation for the selected face.
        _target = Quaternion.Euler(_faceRotations[(int)_currentFace]);
        _angle = Mathf.Max(Quaternion.Angle(_start, _target), 0.001f);

        float t = 0f;
        while (t < 1f)
        {
            t += (_rotationSpeed * Time.deltaTime) / _angle;
            _cube.localRotation = Quaternion.Slerp(_start, _target, Mathf.Clamp01(t));
            yield return null;
        }

        _cube.localRotation = _target;   // snap to an exact facing so it never drifts
        _isRotating = false;

        FaceChanged?.Invoke(_currentFace);
    }

    // The local player's object (owner). Same idea as LevelHolder's lookup.
    private Transform FindLocalPlayer()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null && nm.LocalClient != null && nm.LocalClient.PlayerObject != null)
            return nm.LocalClient.PlayerObject.transform;
        return null;
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Interact range: green while the local player is in range, yellow otherwise.
        Gizmos.color = inRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}