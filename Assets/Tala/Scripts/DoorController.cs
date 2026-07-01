using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0, 3, 0); // المسافة التي سيتحركها الباب لأعلى
    public float speed = 2f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;
    }

    void Update()
    {
        // تحريك الباب بسلاسة نحو الهدف
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
    }

    public void OpenDoor()
    {
        targetPosition = closedPosition + openOffset;
    }

    public void CloseDoor()
    {
        targetPosition = closedPosition;
    }
}