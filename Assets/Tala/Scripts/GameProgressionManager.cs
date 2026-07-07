using UnityEngine;

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance;

    [Header("Progression Status")]
    public bool hasFirstCrystal = false;
    public bool hasSecondCrystal = false;

    [Header("Central Statue Audio")]
    [Tooltip("صوت التمثال عند تبقي كريستالة واحدة")]
    public AudioSource remainingOneCrystalSound;
    [Tooltip("صوت التمثال عند جمع كل الكريستالات وفتح اللغز النهائي")]
    public AudioSource allCrystalsCollectedSound;

    private bool playedRemainingVoice = false;
    private bool playedCompletedVoice = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // دالة ذكية تستقبل رقم الكريستالة وتسجلها في النظام
    public void CollectCrystal(int crystalID)
    {
        if (crystalID == 1) hasFirstCrystal = true;
        if (crystalID == 2) hasSecondCrystal = true;
        Debug.Log($"تم تسجيل الكريستالة رقم {crystalID} بنجاح!");
    }

    public void InteractWithCentralStatue()
    {
        // الحالة 1: اللاعب جلب الكريستالة الأولى فقط (متبقي واحدة)
        if (hasFirstCrystal && !hasSecondCrystal && !playedRemainingVoice)
        {
            if (remainingOneCrystalSound != null)
            {
                remainingOneCrystalSound.Play();
                playedRemainingVoice = true;
                Debug.Log("التمثال يتحدث: متبقي لك جوهرة واحدة... ابحث في الغرفة الأخرى!");
            }
        }
        // الحالة 2: اللاعب نجح بجمع الجوهرتين معاً (الفوز النهائي!)
        else if (hasFirstCrystal && hasSecondCrystal && !playedCompletedVoice)
        {
            if (allCrystalsCollectedSound != null)
            {
                allCrystalsCollectedSound.Play();
                playedCompletedVoice = true;
                Debug.Log("التمثال يتحدث: أهلاً بك يا بطل! لقد جمعت الجوهرتين، البوابة مفتوحة الآن!");
            }
        }
    }
}