using UnityEngine;

public class ReadableScroll : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("اسحبي لوحة الـ UI (Canvas أو Panel) المخصصة للقراءة هنا")]
    public GameObject scrollCanvasUI;

    [Header("Auto Close Settings")]
    [Tooltip("المسافة التي إذا ابتعد عنها اللاعب تقفل الورقة تلقائيًا")]
    public float autoCloseDistance = 3.5f;

    private bool isReading = false;
    private Transform playerTransform;

    void Start()
    {
        if (scrollCanvasUI != null)
            scrollCanvasUI.SetActive(false);

        // البحث عن اللاعب تلقائيًا في الماب عن طريق الـ Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        // إذا كان اللاعب جالس يقرأ حاليًا واللاعب موجود في الماب
        if (isReading && playerTransform != null)
        {
            // حساب المسافة الحالية بين مجسم الورقة واللاعب
            float currentDistance = Vector3.Distance(transform.position, playerTransform.position);

            // إذا تعدت المسافة الحد المسموح، نقفل الورقة فورًا
            if (currentDistance > autoCloseDistance)
            {
                CloseScroll();
            }
        }
    }

    public void ToggleScroll()
    {
        if (scrollCanvasUI == null) return;

        isReading = !isReading;
        scrollCanvasUI.SetActive(isReading);

        if (isReading)
            Debug.Log("اللاعب يقرأ اللغز الآن...");
    }

    // دالة خاصة للإغلاق التلقائي عند الابتعاد
    private void CloseScroll()
    {
        isReading = false;
        if (scrollCanvasUI != null)
            scrollCanvasUI.SetActive(false);

        Debug.Log("ابتعد اللاعب.. تم إغلاق الورقة تلقائيًا!");
    }
}