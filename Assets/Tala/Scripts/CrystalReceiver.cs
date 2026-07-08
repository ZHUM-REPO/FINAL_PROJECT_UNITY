using UnityEngine;

public class CrystalReceiver : MonoBehaviour
{
    [Header("References")]
    public DoorController door; // اسحبي الباب هنا من الـ Inspector

    [Tooltip("اسحبي مجسم الكريستالة الثانية الذي يحمل سكريبت SecondCrystalReward هنا")]
    public SecondCrystalReward secondCrystalReward;

    [Header("Timer Settings")]
    public float requiredTime = 4f; // الوقت المطلوب بالثواني
    private float currentTimer = 0f;
    private bool isHitThisFrame = false;
    private bool isDoorOpened = false;

    [Header("Color Settings (HDR)")]
    public Color normalColor = Color.gray;       // اللون الافتراضي (مطفي أو خافت)
    public Color fullyChargedColor = Color.green; // اللون عند اكتمال الشحن (مشع)

    private Renderer crystalRenderer;
    private Material crystalMaterial;

    void Start()
    {
        crystalRenderer = GetComponent<Renderer>();
        if (crystalRenderer != null)
        {
            crystalMaterial = crystalRenderer.material;
            SetCrystalColor(normalColor);
        }
    }

    public void ActivateCrystal()
    {
        isHitThisFrame = true;
    }

    void LateUpdate()
    {
        if (isHitThisFrame && !isDoorOpened)
        {
            currentTimer += Time.deltaTime;

            float progress = currentTimer / requiredTime;

            Color currentColor = Color.Lerp(normalColor, fullyChargedColor, progress);
            SetCrystalColor(currentColor);

            if (currentTimer >= requiredTime)
            {
                // 1. فتح الباب الشفاف
                if (door != null) door.OpenDoor();

                // 2. تفعيل أمان الكريستالة الثانية لتصبح قابلة للأخذ فوراً!
                if (secondCrystalReward != null)
                {
                    secondCrystalReward.OnSecondPuzzleSolved();
                    Debug.Log("[CrystalReceiver] تم فتح أمان الكريستالة الثانية بنجاح!");
                }

                isDoorOpened = true;
                SetCrystalColor(fullyChargedColor);
            }
        }
        else if (!isHitThisFrame && !isDoorOpened)
        {
            if (currentTimer > 0)
            {
                currentTimer = 0f;
                SetCrystalColor(normalColor);
            }
        }

        isHitThisFrame = false;
    }

    void SetCrystalColor(Color color)
    {
        if (crystalMaterial != null)
        {
            if (crystalMaterial.HasProperty("_BaseColor"))
                crystalMaterial.SetColor("_BaseColor", color);
            else if (crystalMaterial.HasProperty("_Color"))
                crystalMaterial.SetColor("_Color", color);

            crystalMaterial.SetColor("_EmissionColor", color);
            crystalMaterial.EnableKeyword("_EMISSION");
        }
    }

    void OnDestroy()
    {
        if (crystalMaterial != null)
            Destroy(crystalMaterial);
    }
}