using System.Collections.Generic;
using UnityEngine;

// هذه الخاصية تسمح للكود بالعمل داخل الـ Editor تلقائياً لرؤية الليزر أثناء التصميم
[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    public int maxReflections = 5;       // أقصى عدد للمرايا التي ينعكس عليها الشعاع
    public float maxStepDistance = 100f; // مدى مسافة الشعاع القصوى

    private LineRenderer lineRenderer;
    private List<Vector3> laserPoints = new List<Vector3>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        BuildLaser();
    }

    void BuildLaser()
    {
        // تصفية النقاط القديمة والبدء من موقع مصدر الليزر الحالي
        laserPoints.Clear();
        laserPoints.Add(transform.position);

        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = transform.forward; // ينطلق الشعاع للأمام بناءً على دوران المجسم

        for (int i = 0; i < maxReflections; i++)
        {
            Ray ray = new Ray(currentPosition, currentDirection);
            RaycastHit hit;

            // إطلاق الشعاع الفيزيائي
            if (Physics.Raycast(ray, out hit, maxStepDistance))
            {
                laserPoints.Add(hit.point); // أضف نقطة الاصطدام الحالية لخط الليزر

                // 1. إذا اصطدم بمراية
                if (hit.collider.CompareTag("Mirror"))
                {
                    // حساب زاوية الانعكاس الرياضية وتحديث الاتجاه والموقع لنقطة الانعكاس التالية
                    currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                    currentPosition = hit.point;
                }
                // 2. إذا اصطدم بالبلورة المستقبِلة
                else if (hit.collider.CompareTag("Crystal"))
                {
                    CrystalReceiver crystal = hit.collider.GetComponent<CrystalReceiver>();
                    if (crystal != null)
                    {
                        crystal.ActivateCrystal();
                    }
                    break; // يتوقف الليزر عند البلورة ولا يخترقها
                }
                // 3. إذا اصطدم بجدار أو أي جسم آخر
                else
                {
                    break; // توقف عن حساب الانعكاسات
                }
            }
            else
            {
                // إذا لم يصطدم بأي شيء، ارسم الشعاع إلى أقصى مدى متاح ثم توقف
                laserPoints.Add(currentPosition + currentDirection * maxStepDistance);
                break;
            }
        }

        // تحديث الـ LineRenderer ليرسم النقاط المحسوبة بالكامل
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = laserPoints.Count;
            lineRenderer.SetPositions(laserPoints.ToArray());
        }
    }
}