using UnityEngine;

public class IceWall : MonoBehaviour
{
    public float duration = 10f;
    public bool isWider = false;
    public bool freezeOnTouch = false;
    public float freezeDuration = 2f;
    public ParticleSystem breakParticles;

    private void Start()
    {
        if (isWider)
            transform.localScale = new Vector3(
                transform.localScale.x * 2f,
                transform.localScale.y,
                transform.localScale.z);

        Invoke(nameof(BreakWall), duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!freezeOnTouch) return;
        if (other.CompareTag("Player")) return;

        IFreezable freezable = other.GetComponent<IFreezable>();
        if (freezable != null)
            freezable.Freeze(freezeDuration, false);
    }

    private void BreakWall()
    {
        if (breakParticles != null)
            Instantiate(breakParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}