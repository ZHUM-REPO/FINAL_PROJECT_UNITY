using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;

    [Header("UI Reference")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;

    private Camera mainCamera;
    private InteractableMirror currentLockedMirror = null;

    void Start()
    {
        mainCamera = Camera.main;
        if (interactionUI != null) interactionUI.SetActive(false);
    }

    void Update()
    {
        if (mainCamera == null) return;

        if (currentLockedMirror != null)
        {
            if (interactionUI != null) interactionUI.SetActive(false);
            float mouseX = Input.GetAxis("Mouse X");
            currentLockedMirror.RotateWithMouse(mouseX);
            if (Input.GetMouseButtonUp(1)) currentLockedMirror = null;
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            InteractableMirror mirror = hit.collider.GetComponent<InteractableMirror>();
            Torch torch = hit.collider.GetComponent<Torch>();

            // الكريستالة الأولى
            PuzzleTargetReward reward1 = hit.collider.GetComponent<PuzzleTargetReward>();
            if (reward1 == null) reward1 = hit.collider.GetComponentInParent<PuzzleTargetReward>();

            // الكريستالة الثانية الجديدة
            SecondCrystalReward reward2 = hit.collider.GetComponent<SecondCrystalReward>();
            if (reward2 == null) reward2 = hit.collider.GetComponentInParent<SecondCrystalReward>();

            // 1. التفاعل مع المرايا
            if (mirror != null && mirror.isMovable)
            {
                if (interactionText != null) interactionText.text = "Hold RC to rotate";
                if (interactionUI != null) interactionUI.SetActive(true);
                if (Input.GetMouseButtonDown(1)) currentLockedMirror = mirror;
            }
            // 2. التفاعل مع المشاعل
            else if (torch != null && !torch.IsLit)
            {
                if (interactionText != null) interactionText.text = "Press E to Light Torch";
                if (interactionUI != null) interactionUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E)) torch.LightTorch();
            }
            // 3. التقاط الكريستالة الأولى (التي تظهر بعد الحل)
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
            // 4. التقاط الكريستالة الثانية الجديدة (الموجودة من البداية وتنتظر فتح اللغز)
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
            // 5. التفاعل مع التمثال المركزي
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

    void HideUI()
    {
        if (interactionUI != null && interactionUI.activeSelf)
        {
            if (currentLockedMirror == null) interactionUI.SetActive(false);
        }
    }
}