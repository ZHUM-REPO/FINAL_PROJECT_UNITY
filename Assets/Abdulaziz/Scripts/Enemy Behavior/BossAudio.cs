using UnityEngine;

[RequireComponent(typeof(BossAI))]
public class BossAudio : MonoBehaviour
{
    [SerializeField] BossAI boss;

    [Header("Audio Sources")]
    [SerializeField] AudioSource sfxSource;   // one-shot attack sounds
    [SerializeField] AudioSource loopSource;  // looping clapping, then running
    [SerializeField] AudioSource musicSource; // boss fight music

    [Header("Clips")]
    [SerializeField] AudioClip normalAttackClip;
    [SerializeField] AudioClip heavyAttackClip;
    [SerializeField] AudioClip clappingClip;
    [SerializeField] AudioClip runningClip;
    [SerializeField] AudioClip musicClip;

    [Header("Settings")]
    [SerializeField] float runSoundThreshold = 0.1f;
    [SerializeField] bool stopMusicOnDeath = false;

    bool runningSoundPlaying;

    void Awake()
    {
        if (boss == null) boss = GetComponent<BossAI>();
    }

    void OnEnable()
    {
        boss.StoodUp += OnStoodUp;
        boss.SpeedChanged += OnSpeedChanged;
        boss.NormalAttacked += OnNormalAttacked;
        boss.HeavyAttacked += OnHeavyAttacked;
        boss.Died += OnDied;

        PlayLoop(clappingClip); // clapping starts with the intro animation
    }

    void OnDisable()
    {
        boss.StoodUp -= OnStoodUp;
        boss.SpeedChanged -= OnSpeedChanged;
        boss.NormalAttacked -= OnNormalAttacked;
        boss.HeavyAttacked -= OnHeavyAttacked;
        boss.Died -= OnDied;
    }

    void OnStoodUp()
    {
        // clapping is over — kill the loop and kick off the boss music
        StopLoop();

        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void OnSpeedChanged(float speed01)
    {
        bool shouldRun = speed01 > runSoundThreshold;
        if (shouldRun && !runningSoundPlaying) PlayLoop(runningClip);
        else if (!shouldRun && runningSoundPlaying) StopLoop();
    }

    void OnNormalAttacked() => PlayOneShot(normalAttackClip);
    void OnHeavyAttacked() => PlayOneShot(heavyAttackClip);

    void OnDied()
    {
        StopLoop();
        if (stopMusicOnDeath && musicSource != null) musicSource.Stop();
    }

    void PlayLoop(AudioClip clip)
    {
        if (loopSource == null || clip == null) return;
        loopSource.clip = clip;
        loopSource.loop = true;
        loopSource.Play();
        runningSoundPlaying = clip == runningClip;
    }

    void StopLoop()
    {
        if (loopSource != null) loopSource.Stop();
        runningSoundPlaying = false;
    }

    void PlayOneShot(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }
}