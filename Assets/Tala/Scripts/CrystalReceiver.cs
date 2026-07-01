using UnityEngine;

public class CrystalReceiver : MonoBehaviour
{
    public DoorController door; // اسحب الباب هنا من الـ Inspector
    private bool isHitThisFrame = false;

    public void ActivateCrystal()
    {
        isHitThisFrame = true;
    }

    void LateUpdate()
    {
        // إذا كان الليزر يصطدم بالبلورة، افتح الباب، وإذا انقطع قفله
        if (isHitThisFrame)
            door.OpenDoor();
        else
            door.CloseDoor();

        // صفر الحالة للإطار القادم ليتأكد الليزر من الاصطدام مجدداً
        isHitThisFrame = false;
    }
}
