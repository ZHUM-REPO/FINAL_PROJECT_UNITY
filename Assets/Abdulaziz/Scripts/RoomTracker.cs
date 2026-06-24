using System.Collections.Generic;
using UnityEngine;


public class RoomTracker : MonoBehaviour
{
    [Header("Origin")]
    [Tooltip("Where 'the original room' is. Leave empty to use the player's " +
             "position when the game starts.")]
    [SerializeField] private Transform originPoint;


    private readonly List<Snapshot> history = new List<Snapshot>();
    private Snapshot origin;

    private struct Snapshot
    {
        public Vector3 position;
        public Quaternion rotation;
    }


    public int Depth => history.Count;

    private void Start()
    {

        origin = originPoint != null
            ? new Snapshot { position = originPoint.position, rotation = originPoint.rotation }
            : new Snapshot { position = transform.position, rotation = transform.rotation };
    }

    public void RecordLocation()
    {
        history.Add(new Snapshot
        {
            position = transform.position,
            rotation = transform.rotation
        });
    }

    public void ReturnToPrevious()
    {
        if (history.Count == 0) return;

        int last = history.Count - 1;
        Snapshot previous = history[last];
        history.RemoveAt(last);
        MovePlayer(previous);
    }

    public void ReturnToOrigin()
    {
        history.Clear();
        MovePlayer(origin);
    }

    private void MovePlayer(Snapshot target)
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.SetPositionAndRotation(target.position, target.rotation);

        if (cc != null) cc.enabled = true;
    }



    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Vector3 originPos = originPoint != null ? originPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(originPos, 0.35f);
    }
}