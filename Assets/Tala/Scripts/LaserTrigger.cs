using UnityEngine;

public class LaserTrigger : MonoBehaviour
{
    [Header("Laser Setup")]
    [Tooltip("اسحبي مجسم الليزر الرئيسي أو الكود الخاص به هنا")]
    public GameObject laserObject;

    private void Start()
    {
        // التأكد من أن الليزر مغلق تماماً في بداية اللعبة
        if (laserObject != null)
        {
            laserObject.SetActive(false);
        }
    }

    // هذه الدالة تشتغل تلقائياً أول ما يلمس كبسولة اللاعب الـ Trigger
    private void OnTriggerEnter(Collider other)
    {
        // التأكد من أن الذي دخل هو اللاعب (تأكدي أن الـ Tag للاعب هو Player)
        if (other.CompareTag("Player"))
        {
            if (laserObject != null)
            {
                laserObject.SetActive(true); // تشغيل الليزر فوراً!
                Debug.Log("اللاعب دخل الممر.. تم تشغيل الليزر!");
            }

            // تدمير الـ Trigger لكي لا يشتغل مرة أخرى إذا رجع اللاعب
            Destroy(gameObject);
        }
    }
}