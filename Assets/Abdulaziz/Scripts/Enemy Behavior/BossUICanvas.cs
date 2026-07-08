using UnityEngine;

public class BossUICanvas : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] BossAI boss; // drag the boss in — shows on FightStarted, hides on Died

    [Header("Canvas Objects (enabled / disabled together)")]
    [SerializeField] GameObject[] canvasObjects;

    [Header("HP Bar Images (hidden / shown together)")]
    [SerializeField] GameObject backgroundImage;
    [SerializeField] GameObject fillImage;

    [Header("Startup")]
    [SerializeField] bool hiddenOnStart = true;
    [SerializeField] float hideDelayAfterDeath = 1f;

    void Awake()
    {
        if (hiddenOnStart) Hide();
    }

    void OnEnable()
    {
        if (boss != null)
        {
            boss.FightStarted += Show;
            boss.Died += OnBossDied;
        }
    }

    void OnDisable()
    {
        if (boss != null)
        {
            boss.FightStarted -= Show;
            boss.Died -= OnBossDied;
        }
    }

    void OnBossDied() => Invoke(nameof(Hide), hideDelayAfterDeath);

    public void Show() => SetVisible(true);
    public void Hide() => SetVisible(false);
    public void Toggle() => SetVisible(!IsVisible());

    public void SetVisible(bool visible)
    {
        // enable/disable the canvas GameObjects
        if (canvasObjects != null)
        {
            foreach (GameObject go in canvasObjects)
                if (go != null) go.SetActive(visible);
        }

        // hide/show the two HP bar images
        if (backgroundImage != null) backgroundImage.SetActive(visible);
        if (fillImage != null) fillImage.SetActive(visible);
    }

    bool IsVisible()
    {
        if (fillImage != null) return fillImage.activeSelf;

        if (canvasObjects != null)
        {
            foreach (GameObject go in canvasObjects)
                if (go != null) return go.activeSelf;
        }
        return false;
    }
}