using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;

    [Header("UI Reference")]
    public GameObject interactionUI;

    private Camera mainCamera;
    private InteractableMirror currentLockedMirror = null;

    void Start()
    {
        mainCamera = Camera.main;
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 1. إدارة تدوير المرايا (تظل كما هي تماماً)
        if (currentLockedMirror != null)
        {
            if (interactionUI != null) interactionUI.SetActive(false);

            float mouseX = Input.GetAxis("Mouse X");
            currentLockedMirror.RotateWithMouse(mouseX);

            if (Input.GetMouseButtonUp(1))
            {
                currentLockedMirror = null;
            }

            return;
        }

        // 2. الفحص العام للاصطدام بالأشياء (مرايا أو مشاعل)
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // فحص إذا كان الكائن مرآة
            InteractableMirror mirror = hit.collider.GetComponent<InteractableMirror>();
            // فحص إذا كان الكائن مشعل (وعاء رماد)
            Torch torch = hit.collider.GetComponent<Torch>();

            if (mirror != null && mirror.isMovable)
            {
                if (interactionUI != null) interactionUI.SetActive(true);

                if (Input.GetMouseButtonDown(1))
                {
                    currentLockedMirror = mirror;
                }
            }
            else if (torch != null && !torch.IsLit) // إذا نظر للمشعل ولم يكن مشتعلاً بعد
            {
                if (interactionUI != null) interactionUI.SetActive(true);

                // التفاعل مع المشعل بضغطة زر الفأرة الأيمن (نفس زر المرآة لتوحيد التحكم)
                // يمكنكِ تغييره لـ GetMouseButtonDown(0) للأيسر إذا أردتِ
                if (Input.GetMouseButtonDown(1))
                {
                    torch.LightTorch();
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