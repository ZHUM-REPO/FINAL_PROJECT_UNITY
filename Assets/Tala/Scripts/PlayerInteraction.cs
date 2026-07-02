using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;

    [Header("UI Reference")]
    public GameObject interactionUI;

    private Camera mainCamera;
    private InteractableMirror currentLockedMirror = null; // لحفظ المرآة التي يتم تدويرها حالياً

    void Start()
    {
        mainCamera = Camera.main;
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 1. إذا كان اللاعب جالس يدوّر مرآة حالياً (مستمر بالضغط على زر الفأرة الأيمن)
        if (currentLockedMirror != null)
        {
            // إخفاء النص أثناء التدوير لكي لا يشوش على الرؤية
            if (interactionUI != null) interactionUI.SetActive(false);

            // قراءة حركة الفأرة الأفقية (يمين ويسار)
            float mouseX = Input.GetAxis("Mouse X");

            // تدوير المرآة بحرية بناءً على حركة يد اللاعب
            currentLockedMirror.RotateWithMouse(mouseX);

            // إذا رفع اللاعب إصبعه عن زر الفأرة، يفك القفل عن المرآة
            if (Input.GetMouseButtonUp(1)) // 1 تعني زر الفأرة الأيمن (يمكنكِ تغييرها لـ 0 للأيسر)
            {
                currentLockedMirror = null;
            }

            return; // تخطي باقي الكود طالما التدوير مستمر
        }

        // 2. الفحص العادي للاصطدام بالمرآة عند الاقتراب
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            InteractableMirror mirror = hit.collider.GetComponent<InteractableMirror>();

            if (mirror != null && mirror.isMovable)
            {
                if (interactionUI != null)
                    interactionUI.SetActive(true);

                // إذا ضغط اللاعب على زر الفأرة الأيمن للبدء في التدوير الحر
                if (Input.GetMouseButtonDown(1))
                {
                    currentLockedMirror = mirror; // قفل التحكم على هذه المرآة
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

    void HideUI()
    {
        if (interactionUI != null && interactionUI.activeSelf)
        {
            interactionUI.SetActive(false);
        }
    }
}