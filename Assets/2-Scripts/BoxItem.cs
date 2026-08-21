using UnityEngine;

public class BoxItem : MonoBehaviour, IInteractable
{
    private bool isHeld;
    private Transform holdPoint;

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (isHeld)
        {
            GameManager.Instance.ConsumirBateria(1);
            if (Input.GetKeyDown(KeyCode.E))
            {
                Drop();
            }
        }
    }
    public void Interact()
    {
        if (!GameManager.Instance.grab) return;
        if (isHeld) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        holdPoint = player.transform.Find("HandHoldPoint");
        if (holdPoint == null) holdPoint = player.transform;

        Grab();
    }

    private void Grab()
    {
        isHeld = true;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = new Vector3(0f, 0f, 0.8f);
        transform.localRotation = Quaternion.identity;

    }

    private void Drop()
    {
        isHeld = false;

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Invoke(nameof(EnableCollider), 0.1f);
    }

    private void EnableCollider()
    {
        if (col != null) col.enabled = true;
    }
}