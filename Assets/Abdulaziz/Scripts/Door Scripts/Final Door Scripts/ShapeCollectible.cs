using System;
using UnityEngine;

/// <summary>
/// A poker-shape pickup (one per level). The player collects it with the
/// existing interact system; it then hides itself and remembers it was
/// collected so the FinalDoor can check it.
/// </summary>
public class ShapeCollectible : MonoBehaviour, IInteractable
{
    [Tooltip("Which shape this pickup represents (Club or Diamond).")]
    [SerializeField] private CubeFace shape;

    /// <summary>True once the player has picked this up.</summary>
    public bool IsCollected { get; private set; }

    public CubeFace Shape => shape;

    // Fired the moment the player picks this up.
    public event Action<ShapeCollectible> Collected;

    public void Interact()
    {
        if (IsCollected) return;

        IsCollected = true;
        Collected?.Invoke(this);

        // Hide the object. The component (and IsCollected) still exists,
        // so the FinalDoor can read it on a disabled GameObject.
        gameObject.SetActive(false);
    }
}