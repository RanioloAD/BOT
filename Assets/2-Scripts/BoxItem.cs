using System.Collections;
using UnityEngine;

public class BoxItem : MonoBehaviour, IInteractable
{
    private bool isHeld;
    private bool canDrop;

    private Transform holdPoint;
    private Transform playerCamera;

    private Rigidbody rb;
    private Collider col;

    [Header("Colocación")]
    [SerializeField] private float reachDistance = 4f;
    [SerializeField] private LayerMask placementLayer;

    [Header("Ghost")]
    [SerializeField] private UnityEngine.Material ghostMaterial;
    [SerializeField] private Color validColor = new Color(0.2f, 1f, 0.3f, 0.35f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.15f, 0.15f, 0.35f);

    private GameObject ghostObject;
    private Renderer[] ghostRenderers;
    private MaterialPropertyBlock propertyBlock;

    private Vector3 ghostPosition;
    private Quaternion ghostRotation;
    private bool ghostValid;

    private Collider supportCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (!isHeld)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.ConsumirBateria(1);

        UpdateGhostPosition();

        if (canDrop && Input.GetKeyDown(KeyCode.E))
            Drop();
    }

    public void Interact()
    {
        if (!GameManager.Instance.grab || isHeld)
            return;

        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
            return;

        if (Camera.main != null)
            playerCamera = Camera.main.transform;

        holdPoint = player.transform.Find("HandHoldPoint");

        if (holdPoint == null)
            holdPoint = player.transform;

        Grab();
    }

    private void Grab()
    {
        isHeld = true;
        canDrop = false;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = new Vector3(0f, 0f, 0.8f);
        transform.localRotation = Quaternion.identity;

        CreateGhost();

        StartCoroutine(EnableDrop());
    }

    private IEnumerator EnableDrop()
    {
        yield return null;
        canDrop = true;
    }

    private void CreateGhost()
    {
        DestroyGhost();

        ghostObject = Instantiate(gameObject);
        ghostObject.name = "BOX_GHOST";

        BoxItem script = ghostObject.GetComponent<BoxItem>();
        if (script != null)
            Destroy(script);

        Rigidbody ghostRb = ghostObject.GetComponent<Rigidbody>();
        if (ghostRb != null)
            Destroy(ghostRb);

        Collider[] colliders = ghostObject.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
            c.enabled = false;

        ghostRenderers = ghostObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in ghostRenderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;

            if (ghostMaterial != null)
            {
                UnityEngine.Material[] mats =
                    new UnityEngine.Material[r.sharedMaterials.Length];

                for (int i = 0; i < mats.Length; i++)
                    mats[i] = ghostMaterial;

                r.sharedMaterials = mats;
            }
        }

        ghostObject.transform.position =
            transform.position + Vector3.down * 1f;

        ghostObject.transform.rotation = Quaternion.identity;

        ghostObject.SetActive(false);
        ghostValid = false;
    }

    private void UpdateGhostPosition()
    {
        if (ghostObject == null || playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.position,
            playerCamera.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            reachDistance,
            placementLayer,
            QueryTriggerInteraction.Ignore))
        {
            ghostValid = false;
            supportCollider = null;
            ghostObject.SetActive(false);
            return;
        }

        supportCollider = hit.collider;

        float halfHeight = col.bounds.extents.y;

        ghostPosition = hit.point + Vector3.up * halfHeight;
        ghostRotation = Quaternion.identity;

        ghostObject.transform.SetPositionAndRotation(
            ghostPosition,
            ghostRotation
        );

        ghostObject.SetActive(true);

        ghostValid = CanPlaceHere();

        UpdateGhostColor();
    }

    private bool CanPlaceHere()
    {
        Vector3 halfExtents = col.bounds.extents * 0.9f;

        Collider[] hits = Physics.OverlapBox(
            ghostPosition,
            halfExtents,
            Quaternion.identity,
            placementLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform.root == transform.root)
                continue;

            if (hit == supportCollider)
                continue;

            return false;
        }

        return true;
    }

    private void UpdateGhostColor()
    {
        Color color = ghostValid ? validColor : invalidColor;

        foreach (Renderer r in ghostRenderers)
        {
            r.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);

            r.SetPropertyBlock(propertyBlock);
        }
    }

    private void DestroyGhost()
    {
        if (ghostObject != null)
            Destroy(ghostObject);

        ghostObject = null;
        ghostRenderers = null;
    }

    private void Drop()
    {
        isHeld = false;
        canDrop = false;

        transform.SetParent(null);

        if (ghostObject != null &&
            ghostObject.activeSelf &&
            ghostValid)
        {
            transform.position = ghostPosition;
            transform.rotation = ghostRotation;
        }

        DestroyGhost();

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Invoke(nameof(EnableCollider), 0.15f);
    }

    private void EnableCollider()
    {
        if (col != null)
            col.enabled = true;
    }
}