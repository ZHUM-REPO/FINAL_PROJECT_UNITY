using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("المسافة القصوى التي يستطيع اللاعب التفاعل من خلالها مع الأشياء")]
    public float interactDistance = 3f;

    [Header("UI Reference")]
    [Tooltip("اسحبي كائن الـ UI الرئيسي للتفاعل هنا (مثل لوحة الـ Canvas أو الـ Panel الخاصة بالـ Crosshair)")]
    public GameObject interactionUI;
    [Tooltip("اسحبي كائن النص TextMeshPro الخاص برسائل التفاعل هنا")]
    public TextMeshProUGUI interactionText;

    private Camera mainCamera;
    private InteractableMirror currentLockedMirror = null;

    void Start()
    {
        // الحصول على الكاميرا الرئيسية للاعب
        mainCamera = Camera.main;

        // التأكد من إخفاء واجهة التفاعل عند بداية اللعبة
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 1. نظام التحكم في تدوير المرايا (إذا كان اللاعب متمسكاً بمرآة حالياً)
        if (currentLockedMirror != null)
        {
            if (interactionUI != null) interactionUI.SetActive(false);
            float mouseX = Input.GetAxis("Mouse X");
            currentLockedMirror.RotateWithMouse(mouseX);

            // ترك المرآة عند رفع الإصبع عن الزر الأيمن للفأرة
            if (Input.GetMouseButtonUp(1))
                currentLockedMirror = null;

            return;
        }

        // إطلاق شعاع من منتصف الشاشة تماماً (محل تطلع عين اللاعب)
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // فحص المكونات الموجودة على الكائن الذي صدمه الشعاع
            InteractableMirror mirror = hit.collider.GetComponent<InteractableMirror>();
            Torch torch = hit.collider.GetComponent<Torch>();

            // الكريستالة الأولى (التي تظهر بعد حل لغز المشاعل)
            PuzzleTargetReward reward1 = hit.collider.GetComponent<PuzzleTargetReward>();
            if (reward1 == null) reward1 = hit.collider.GetComponentInParent<PuzzleTargetReward>();

            // الكريستالة الثانية (الموجودة من البداية وتنتظر حل لغز الليزر)
            SecondCrystalReward reward2 = hit.collider.GetComponent<SecondCrystalReward>();
            if (reward2 == null) reward2 = hit.collider.GetComponentInParent<SecondCrystalReward>();

            // الورقة أو اللغز المكتوب القابل للقراءة
            ReadableScroll scroll = hit.collider.GetComponent<ReadableScroll>();
            if (scroll == null) scroll = hit.collider.GetComponentInParent<ReadableScroll>();


            // --- مصفوفة التفاعلات الذكية ---

            // أ) التفاعل مع المرايا القابلة للتحريك
            if (mirror != null && mirror.isMovable)
            {
                if (interactionText != null) interactionText.text = "Hold RC to rotate";
                if (interactionUI != null) interactionUI.SetActive(true);
                if (Input.GetMouseButtonDown(1)) currentLockedMirror = mirror;
            }

            // ب) التفاعل مع المشاعل (إشعال المشعل)
            else if (torch != null && !torch.IsLit)
            {
                if (interactionText != null) interactionText.text = "Press E to Light Torch";
                if (interactionUI != null) interactionUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E)) torch.LightTorch();
            }

            // ج) التقاط الكريستالة الأولى (بعد حل لغز المشاعل)
            else if (reward1 != null && reward1.CanCollectCrystal)
            {
                if (interactionText != null) interactionText.text = "Press E to Take Crystal";
                if (interactionUI != null) interactionUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    reward1.CollectCrystal();
                    HideUI();
                }
            }

            // د) التقاط الكريستالة الثانية (المحجوزة خلف الباب الشفاف)
            else if (reward2 != null && reward2.CanCollectCrystal)
            {
                if (interactionText != null) interactionText.text = "Press E to Take Crystal";
                if (interactionUI != null) interactionUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    reward2.CollectCrystal();
                    HideUI();
                }
            }

            // هـ) قراءة الورقة أو المخطوطة المكتوبة (اللغز المكتوب)
            else if (scroll != null)
            {
                if (interactionText != null) interactionText.text = "Press E to Read Note";
                if (interactionUI != null) interactionUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    scroll.ToggleScroll();
                }
            }

            // و) التفاعل مع التمثال المركزي لمعرفة تقدم اللعبة والأصوات
            else if (hit.collider.CompareTag("Statue"))
            {
                if (GameProgressionManager.Instance != null)
                {
                    if (interactionText != null) interactionText.text = "Press E to Examine Statue";
                    if (interactionUI != null) interactionUI.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        GameProgressionManager.Instance.InteractWithCentralStatue();
                    }
                }
            }
            else
            {
                HideUI();
            }
        }
        else
        {
            HideUI();
        }
    }

    // دالة مساعدة لإخفاء واجهة النص عندما لا ينظر اللاعب لشيء قابل للتفاعل
    void HideUI()
    {
        if (interactionUI != null && interactionUI.activeSelf)
        {
            if (currentLockedMirror == null)
                interactionUI.SetActive(false);
        }
    }
}