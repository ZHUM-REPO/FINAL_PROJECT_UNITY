using UnityEngine;

/// <summary>
/// Standalone reaction: when a ShapeCollectible is picked up, tint a target
/// GameObject. Subscribes to the collectible's Collected event in OnEnable and
/// unsubscribes in OnDisable. Works on a Renderer (via MaterialPropertyBlock, so
/// no material instances are leaked) or a SpriteRenderer for 2D.
/// </summary>
[DisallowMultipleComponent]
public class CollectibleColorReaction : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("The collectible to watch. Fires when it gets collected.")]
    [SerializeField] private ShapeCollectible item;

    [Header("Target")]
    [Tooltip("The GameObject whose color changes on collection. " +
             "Needs a Renderer (3D) or SpriteRenderer (2D).")]
    [SerializeField] private GameObject target;

    [Header("Colors")]
    [SerializeField] private Color collectedColor = new Color(0.3f, 1f, 0.4f, 1f);

    // Resolved target state.
    private Renderer targetRenderer;
    private SpriteRenderer targetSprite;
    private MaterialPropertyBlock mpb;
    private int colorPropertyId;
    private bool resolved;

    private void Awake() => ResolveTarget();

    private void OnEnable()
    {
        if (item != null) item.Collected += HandleCollected;

        // Sync immediately so a re-enabled (or late-added) reaction shows the
        // right color if the item was already collected.
        if (item != null && item.IsCollected) SetColor(collectedColor);
    }

    private void OnDisable()
    {
        if (item != null) item.Collected -= HandleCollected;
    }

    private void HandleCollected(ShapeCollectible collected) => SetColor(collectedColor);

    private void ResolveTarget()
    {
        resolved = false;
        if (target == null) return;

        // 2D path.
        targetSprite = target.GetComponent<SpriteRenderer>();
        if (targetSprite != null)
        {
            resolved = true;
            return;
        }

        // 3D path — use a MaterialPropertyBlock to avoid instancing the material.
        targetRenderer = target.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            // URP/HDRP shaders use _BaseColor; the built-in pipeline uses _Color.
            Material mat = targetRenderer.sharedMaterial;
            bool baseColor = mat != null && mat.HasProperty("_BaseColor");
            colorPropertyId = Shader.PropertyToID(baseColor ? "_BaseColor" : "_Color");

            mpb = new MaterialPropertyBlock();
            resolved = true;
            return;
        }

        Debug.LogWarning($"{nameof(CollectibleColorReaction)}: target '{target.name}' has no Renderer or SpriteRenderer to color.", this);
    }

    private void SetColor(Color c)
    {
        if (!resolved) return;

        if (targetSprite != null)
        {
            targetSprite.color = c;
            return;
        }

        if (targetRenderer != null)
        {
            targetRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(colorPropertyId, c);
            targetRenderer.SetPropertyBlock(mpb);
        }
    }
}