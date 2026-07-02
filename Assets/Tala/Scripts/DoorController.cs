using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("المسافة التي سينزلها الباب لأسفل. يجب أن تكون بالسالب مثل -3")]
    public Vector3 openOffset = new Vector3(0,3.5f, 0);
    public float speed = 2f; // سرعة نزول الباب

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool shouldOpen = false;

    void Start()
    {
        // حفظ الموقع الأصلي للباب وهو مغلق
        closedPosition = transform.position;
        // حساب الموقع النهائي تحت الأرض بناءً على الـ Offset السالب
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (shouldOpen)
        {
            // تحريك الباب تدريجياً وسلساً نحو الموقع السفلي المتنحي تحت الأرض
            transform.position = Vector3.MoveTowards(transform.position, openPosition, speed * Time.deltaTime);
        }
    }

    // هذه الدالة يتم استدعاؤها من البلورة بعد شحنها 4 ثوانٍ
    public void OpenDoor()
    {
        shouldOpen = true;
    }

    // (اختياري) دالة لإغلاق الباب مجدداً إذا انقطع الليزر
    public void CloseDoor()
    {
        shouldOpen = false;
        // يمكنكِ إضافة كود تحريك للعودة للموقع الأصلي هنا إذا أردتِ إغلاقه
    }
}