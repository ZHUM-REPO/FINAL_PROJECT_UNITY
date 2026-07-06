using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossArenaTrigger : MonoBehaviour
{
    [Header("Objects to enable on entry")]
    [SerializeField] GameObject[] objectsToActivate; // e.g. the boss, the boss UI canvas

    [Header("Settings")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] bool disableOnStart = true; // turn the objects off when the scene loads
    [SerializeField] bool triggerOnce = true;     // only fire the first time a player enters
    [SerializeField] bool disableColliderAfter = true; // stop the trigger from firing again

    bool triggered;

    void Awake()
    {
        if (disableOnStart) SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        SetActive(true);

        if (disableColliderAfter)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    void SetActive(bool state)
    {
        if (objectsToActivate == null) return;
        foreach (GameObject go in objectsToActivate)
            if (go != null) go.SetActive(state);
    }
}