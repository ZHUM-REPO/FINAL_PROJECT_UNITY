using System.Collections.Generic;
using UnityEngine;

public class TorchPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("ضع المشاعل هنا بالترتيب الصحيح لحل اللغز")]
    public List<Torch> correctOrder;

    [Header("Target Object")]
    [Tooltip("الرسالة أو الكائن الذي سيظهر بعد حل اللغز")]
    public GameObject messageObject;

    private List<Torch> currentOrder = new List<Torch>();
    private bool isPuzzleSolved = false;

    void Start()
    {
        if (messageObject != null)
            messageObject.SetActive(false); // إخفاء الرسالة في البداية
    }

    // يتم استدعاء هذه الدالة من سكريبت المشعل عندما يشعله اللاعب
    public void TorchActivated(Torch torch)
    {
        if (isPuzzleSolved) return;

        // إذا كان المشعل مشتعل مسبقاً، لا تفعل شيء
        if (currentOrder.Contains(torch)) return;

        currentOrder.Add(torch);
        CheckPuzzleProgress();
    }

    private void CheckPuzzleProgress()
    {
        int currentIndex = currentOrder.Count - 1;

        // التحقق إذا كان المشعل الأخير المشتعل في الترتيب الصحيح
        if (currentOrder[currentIndex] != correctOrder[currentIndex])
        {
            // إذا أخطأ اللاعب، يتم إعادة تعيين اللغز
            Debug.Log("ترتيب خاطئ! إعادة تعيين المشاعل...");
            ResetPuzzle();
            return;
        }

        // إذا وصل اللاعب لنفس عدد المشاعل الصحيحة بالترتيب الصح
        if (currentOrder.Count == correctOrder.Count)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        isPuzzleSolved = true;
        Debug.Log("تم حل اللغز بنجاح!");

        if (messageObject != null)
        {
            messageObject.SetActive(true); // إظهار الرسالة في المكان المحدد
        }
    }

    public void ResetPuzzle()
    {
        currentOrder.Clear();

        // إطفاء جميع المشاعل في اللعبة مجدداً
        foreach (Torch torch in correctOrder)
        {
            torch.Extinguish();
        }
    }
}