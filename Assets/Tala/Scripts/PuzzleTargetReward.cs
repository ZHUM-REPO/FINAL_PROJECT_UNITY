using UnityEngine;

public class PuzzleTargetReward : MonoBehaviour
{
    [Header("Crystal Identity")]
    [Tooltip("رقم الكريستالة (1 للغز الأول، 2 للغز الثاني)")]
    [Range(1, 2)] public int crystalID = 1;

    [Header("Crystal Setup")]
    public GameObject crystalObject;
    public float rotationSpeed = 50f;

    [Header("Effects Setup")]
    public ParticleSystem smokeParticle;
    public ParticleSystem fireParticle;

    [Header("Audio Setup")]
    public AudioSource crystalSpawnSound;

    private bool isSolved = false;
    private bool isCrystalCollected = false;
    private Vector3 startPosition; // لحفظ موقع الكريستالة الأصلي

    public bool CanCollectCrystal => isSolved && !isCrystalCollected;

    void Start()
    {
        if (crystalObject != null)
        {
            crystalObject.SetActive(false);
            // حفظ الموقع المحلي الأصلي للكريستالة عند البداية عشان تترجح حوله بثبات
            startPosition = crystalObject.transform.localPosition;
        }

        if (smokeParticle != null) smokeParticle.Stop();
        if (fireParticle != null) fireParticle.Stop();
    }

    void Update()
    {
        if (isSolved && !isCrystalCollected && crystalObject != null)
        {
            // 1. الدوران حول نفسها بالضبط (Space.Self) يحل مشكلة اللفة الكبيرة تماماً
            crystalObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);

            // 2. حركة النزول والطلوع بشويش (تترجح بدقة ميكروسكوبية فوق وتحت موقعها الأصلي)
            float doubleSpeed = 1.5f; // سرعة المرجحة
            float heightRange = 0.1f; // مدى الارتفاع والنزول (صغير عشان ما تطير بعيد)

            float newY = startPosition.y + (Mathf.Sin(Time.time * doubleSpeed) * heightRange);

            // نحدث الموقع بدون ما نغير الـ X والـ Z عشان ما تروح يمين ويسار
            crystalObject.transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    public void OnPuzzleSolved()
    {
        if (isSolved) return;
        isSolved = true;
        if (crystalObject != null) crystalObject.SetActive(true);
        if (smokeParticle != null) smokeParticle.Play();
        if (fireParticle != null) fireParticle.Play();
        if (crystalSpawnSound != null) crystalSpawnSound.Play();
    }

    public void CollectCrystal()
    {
        if (isCrystalCollected) return;
        isCrystalCollected = true;
        if (crystalObject != null) crystalObject.SetActive(false);

        if (GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.CollectCrystal(crystalID);
        }
    }
}