using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light torchLight;

    [Header("Flicker Settings")]
    public float minIntensity = 1.0f;  // أقل قوة للنور
    public float maxIntensity = 2.5f;  // أعلى قوة للنور
    public float flickerSpeed = 5f;    // سرعة اهتزاز النار

    private float randomOffset;

    void Start()
    {
        torchLight = GetComponent<Light>();

        // نعطي كل مشعل رقم عشوائي في البداية
        // عشان لو حطيتي الكود على أكثر من مشعل بالممر، ما يهتزون كلهم مع بعض بنفس اللحظة ويصير شكلهم آلي!
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (torchLight == null) return;

        // الـ PerlinNoise يحسب التموج الطبيعي بناءً على الوقت
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);

        // تطبيق التموج على قوة الإضاءة
        torchLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}