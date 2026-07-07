using System;
using System.Collections;
using UnityEngine;

/// <summary>The card faces the cube can land on. Order matters:
/// it maps 1-to-1 onto CubePedestal's Face Rotations array.</summary>
public enum CubeFace
{
    Club = 0,
    Diamond = 1
}

public class CubePedestal : MonoBehaviour, IInteractable
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

    private void OnEnable()
    {
        // Announce the current face so listeners can sync up.
        FaceChanged?.Invoke(_currentFace);
    }

    private void OnDisable()
    {
        // Snap to the target facing and reset, so a mid-spin disable can't
        // leave _isRotating stuck true.
        if (_isRotating)
        {
            if (_cube != null) _cube.localRotation = _target;
            _isRotating = false;
        }
    }

    public void Interact()
    {
        // checks if the object is rotating, if they are then it wont run this code.
        if (_isRotating) return;
        // needs a rotation entry for every shape, otherwise the cube can't face them.
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
        // No accumulated steps, so the cube always lands on club or diamond.
        _target = Quaternion.Euler(_faceRotations[(int)_currentFace]);
        _angle = Mathf.Max(Quaternion.Angle(_start, _target), 0.001f);

        float t = 0f;
        // here it does the rotation
        while (t < 1f)
        {
            t += (_rotationSpeed * Time.deltaTime) / _angle;
            _cube.localRotation = Quaternion.Slerp(_start, _target, Mathf.Clamp01(t));
            yield return null;
        }

        _cube.localRotation = _target;   // snap to an exact facing so it never drifts
        _isRotating = false;

        // New facing is locked in, so the selection is final -> tell listeners.
        FaceChanged?.Invoke(_currentFace);
    }

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}