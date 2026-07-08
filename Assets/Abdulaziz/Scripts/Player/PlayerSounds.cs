using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioSource footstepSource;
    [SerializeField] AudioClip[] walkFootsteps;
    [SerializeField] AudioClip[] runFootsteps;

    [Header("Step Timing (seconds between steps)")]
    [SerializeField] float walkStepInterval = 0.5f;
    [SerializeField] float runStepInterval = 0.3f;

    [Header("Pitch")]
    [SerializeField] float basePitch = 1f;       // the anchor pitch
    [SerializeField] float pitchVariance = 0.1f; // random +/- offset around basePitch

    float stepTimer;

    void Awake()
    {
        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (!moving)
        {
            stepTimer = 0f; // step immediately when movement resumes
            return;
        }

        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float interval = running ? runStepInterval : walkStepInterval;

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep(running ? runFootsteps : walkFootsteps);
            stepTimer = interval;
        }
    }

    void PlayFootstep(AudioClip[] clips)
    {
        if (footstepSource == null || clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        footstepSource.pitch = basePitch + Random.Range(-pitchVariance, pitchVariance);
        footstepSource.PlayOneShot(clip);
    }
}