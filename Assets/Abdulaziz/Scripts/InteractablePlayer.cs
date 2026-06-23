using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionDistance = 3.0f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private IInteractable currentInteractable;

    private void Update()
    {
        CheckLookAtObject();
    }

    private void CheckLookAtObject()
    {

        Ray rayCast = new Ray(transform.position, transform.forward);
        RaycastHit hit;


        if (Physics.Raycast(rayCast, out hit, interactionDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {

                if (interactable != currentInteractable)
                {

                    if (currentInteractable != null) currentInteractable.Interact();

                    currentInteractable = interactable;
                    currentInteractable.Interact();
                }

                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact(); 
            currentInteractable = null;    
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * interactionDistance);
    }
}