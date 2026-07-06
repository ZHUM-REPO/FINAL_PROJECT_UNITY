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

    private int _currentIndex = 0;
    private bool _isRotating = false;
    private Quaternion _start, _target;
    private float _angle;

    // Fired once the cube has snapped to its final facing and the selection is locked in.
    public event Action<CubeFace> FaceChanged;

    /// <summary>The card face currently shown to the player.</summary>
    public CubeFace CurrentFace => (CubeFace)_currentIndex;

    private void OnEnable()
    {
        // Announce the current face so listeners can sync up.
        FaceChanged?.Invoke(CurrentFace);
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
        // needs at least one face rotation to have a valid target.
        if (_faceRotations == null || _faceRotations.Length == 0) return;

        _currentIndex = (_currentIndex + 1) % _faceRotations.Length;
        StartCoroutine(RotateCube());
    }

    private IEnumerator RotateCube()
    {
        _isRotating = true;
        _start = _cube.localRotation;
        // Absolute target: the exact rotation for the selected face.
        // No accumulated steps, so the cube always lands on club or diamond.
        _target = Quaternion.Euler(_faceRotations[_currentIndex]);
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
        FaceChanged?.Invoke(CurrentFace);
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