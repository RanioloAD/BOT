using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 1.2f;
    public LayerMask interactableLayer;
    public KeyCode actionKey = KeyCode.E;

    void Update()
    {
        if (Input.GetKeyDown(actionKey))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, transform.forward, out hit, interactDistance, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Vector3 maxEndPosition = origin + (transform.forward * interactDistance);

        // 1. Dibujar SIEMPRE la línea base (roja) hasta la distancia máxima
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, maxEndPosition);
        Gizmos.DrawWireSphere(maxEndPosition, 0.08f);

        // 2. Si hay impacto, sobreescribir con línea verde y bolita sólida en el punto de choque
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, hit.point);
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
    }
}