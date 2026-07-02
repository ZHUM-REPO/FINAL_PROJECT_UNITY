using UnityEngine;

public class CrystalReceiver : MonoBehaviour
{
    [Header("References")]
    public DoorController door; // اسحب الباب هنا من الـ Inspector

    [Header("Timer Settings")]
    public float requiredTime = 4f; // الوقت المطلوب بالثواني
    private float currentTimer = 0f;
    private bool isHitThisFrame = false;
    private bool isDoorOpened = false;

    [Header("Color Settings (HDR)")]
    // يمكنك تحديد الألوان من الـ Inspector واجعلها HDR لتعطي توهجاً
    public Color normalColor = Color.gray;       // اللون الافتراضي (مطفي أو خافت)
    public Color fullyChargedColor = Color.green; // اللون عند اكتمال الشحن (مشع)

    private Renderer crystalRenderer;
    private Material crystalMaterial;

    void Start()
    {
        // الحصول على الـ Renderer والـ Material الخاصة بالبلورة
        crystalRenderer = GetComponent<Renderer>();
        if (crystalRenderer != null)
        {
            // نأخذ نسخة من الـ Material لكي لا نعدل على الأصل في المشروع بالكامل
            crystalMaterial = crystalRenderer.material;
            // تعيين اللون المبدئي
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

            // حساب النسبة المئوية للشحن (تتراوح بين 0 و 1)
            float progress = currentTimer / requiredTime;

            // تغيير اللون تدريجياً بناءً على النسبة
            Color currentColor = Color.Lerp(normalColor, fullyChargedColor, progress);
            SetCrystalColor(currentColor);

            if (currentTimer >= requiredTime)
            {
                door.OpenDoor();
                isDoorOpened = true;
                SetCrystalColor(fullyChargedColor); // التأكيد على اللون النهائي
            }
        }
        else if (!isHitThisFrame && !isDoorOpened)
        {
            // إذا انقطع الليزر، يعود العداد للصفر، ويعود اللون للوضع الخافت تدريجياً أو فوراً
            if (currentTimer > 0)
            {
                currentTimer = 0f;
                SetCrystalColor(normalColor);
            }
        }

        isHitThisFrame = false;
    }

    // دالة مساعدة لتغيير لون الـ Material والـ Emission الخاص بها
    void SetCrystalColor(Color color)
    {
        if (crystalMaterial != null)
        {
            // تغيير اللون الأساسي للبلورة
            if (crystalMaterial.HasProperty("_BaseColor")) // لنظام URP
                crystalMaterial.SetColor("_BaseColor", color);
            else if (crystalMaterial.HasProperty("_Color")) // للنظام العادي
                crystalMaterial.SetColor("_Color", color);

            // تغيير لون التوهج (Emission) لتبدو مشعة
            crystalMaterial.SetColor("_EmissionColor", color);
            // تفعيل الـ Emission في الـ Material برمجياً للتأكد من عمله
            crystalMaterial.EnableKeyword("_EMISSION");
        }
    }

    void OnDestroy()
    {
        // تنظيف الـ Material من الذاكرة عند تدمير المجسم
        if (crystalMaterial != null)
            Destroy(crystalMaterial);
    }
}