using UnityEngine;

[ExecuteAlways]
public class InteractableMirror : MonoBehaviour
{
    [Header("Mirror Type")]
    [Tooltip("ضع علامة صح إذا كنت تريد هذه المرآة أن تدور، اتركها فارغة إذا كانت ثابتة")]
    public bool isMovable = true;

    [Header("Sensitivity Settings")]
    [Tooltip("سرعة دوران المرآة مع حركة الفأرة")]
    public float rotationSpeed = 5f;

    // دالة تدوير حرّة تستدعيها حركة الفأرة من اللاعب
    public void RotateWithMouse(float mouseInput)
    {
        if (isMovable)
        {
            // حساب قيمة الدوران بناءً على سحب الفأرة وسرعة الدوران
            float rotationAmount = mouseInput * rotationSpeed;

            // تدوير المرآة حول المحور العمودي (Y-Axis) بسلاسة كاملة
            transform.Rotate(0, rotationAmount, 0);
        }
    }
}