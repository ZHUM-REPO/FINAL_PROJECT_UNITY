using UnityEngine;

public class SecondCrystalReward : MonoBehaviour
{
    [Header("Crystal Setup")]
    [Tooltip("مجسم الكريستالة الثانية نفسه (تأكدي من سحبه هنا)")]
    public GameObject crystalObject;

    [Tooltip("سرعة دوران الكريستالة")]
    public float rotationSpeed = 70f;

    [Header("Puzzle Status")]
    [Tooltip("اجعليها صح يدويًا للتجربة ✅")]
    public bool isPuzzleSolved = false;

    private bool isCrystalCollected = false;

    // اللاعب يقدر يأخذها فقط إذا انحل اللغز ولم تُجمع بعد
    public bool CanCollectCrystal => isPuzzleSolved && !isCrystalCollected;

    void Start()
    {
        if (crystalObject != null)
            crystalObject.SetActive(true);

        // حركة ذكية: إذا نسيتي إضافة Collider، الكود سيضيفه تلقائيًا لتستطيعي التقاطها!
        if (GetComponent<Collider>() == null && (crystalObject != null && crystalObject.GetComponent<Collider>() == null))
        {
            gameObject.AddComponent<BoxCollider>();
            Debug.Log($"[SecondCrystal] تم إضافة Box Collider تلقائيًا على {gameObject.name} لتمكين الالتقاط!");
        }
    }

    void Update()
    {
        if (!isCrystalCollected && crystalObject != null)
        {
            // حل مشكلة اللفة الكبيرة: ندور الكريستالة حول محورها المحلي الخاص (Space.Self) 
            // لتدور في مكانها بالضبط بغض النظر عن الـ Pivot
            crystalObject.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);

            // حركة تموجية خفيفة وناعمة للأعلى والأسفل في مكانها
            float newY = Mathf.Sin(Time.time * 1.5f) * 0.05f;
            crystalObject.transform.Translate(Vector3.up * Mathf.Cos(Time.time * 1.5f) * 0.05f * Time.deltaTime, Space.Self);
        }
    }

    public void OnSecondPuzzleSolved()
    {
        if (isPuzzleSolved) return;
        isPuzzleSolved = true;
    }

    public void CollectCrystal()
    {
        if (isCrystalCollected) return;
        isCrystalCollected = true;

        if (crystalObject != null)
            crystalObject.SetActive(false);

        Debug.Log("تم التقاط الكريستالة الثانية بنجاح!");

        if (GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.CollectCrystal(2);
        }
    }
}