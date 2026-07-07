using System.Collections.Generic;
using UnityEngine;

public class TorchPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public List<Torch> correctOrder;

    [Header("Target Reward Setup")]
    [Tooltip("اسحبي كائن الجائزة (PuzzleTargetReward) هنا لتفعيله عند الحل الصحيح")]
    public PuzzleTargetReward targetReward;

    private List<Torch> currentOrder = new List<Torch>();
    private bool isPuzzleSolved = false;

    void Start()
    {
        // تم نقل التحكم في الرسالة والجائزة لكود الـ Reward المنفصل
    }

    public void TorchActivated(Torch torch)
    {
        if (isPuzzleSolved) return;
        if (currentOrder.Contains(torch)) return;

        currentOrder.Add(torch);
        CheckPuzzleProgress();
    }

    private void CheckPuzzleProgress()
    {
        int currentIndex = currentOrder.Count - 1;

        // إذا أخطأ اللاعب في الترتيب مقارنة بالقائمة الصحيحة
        if (currentOrder[currentIndex] != correctOrder[currentIndex])
        {
            Debug.Log("ترتيب خاطئ! إطفاء جميع المشاعل وإعادة تعيين اللغز...");
            ResetPuzzle();
            return;
        }

        // إذا تم تشغيل كل المشاعل بالترتيب الصحيح تماماً
        if (currentOrder.Count == correctOrder.Count)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        isPuzzleSolved = true;
        Debug.Log("تم حل لغز المشاعل بنجاح!");

        // تفعيل الجائزة (إظهار الكريستالة والدخان والنار الخاصة بالغرفة)
        if (targetReward != null)
        {
            targetReward.OnPuzzleSolved();
        }
    }

    public void ResetPuzzle()
    {
        currentOrder.Clear(); // تصفير قائمة المحاولات الحالية

        // إطفاء النار تماماً وتصفير الحالة البرمجية لكل المشاعل لتبدأ من جديد
        foreach (Torch torch in correctOrder)
        {
            torch.Extinguish();
        }
    }
}