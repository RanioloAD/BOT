using UnityEngine;
using UnityEngine.Events;

public class FloorButton : MonoBehaviour
{
    [Header("Configuración del Botón")]
    [SerializeField] private LayerMask objectMask;
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(1f, 0.5f, 1f);
    [SerializeField] private Vector3 detectionBoxOffset = new Vector3(0f, 0.25f, 0f);

    [Header("Eventos de Conexión")]
    public UnityEvent OnButtonPressed;
    public UnityEvent OnButtonReleased;

    [Header("Visuales")]
    [SerializeField] private Transform buttonCap;
    [SerializeField] private Vector3 pressedOffset = new Vector3(0f, -0.1f, 0f);

    private Vector3 initialPosition;
    private bool isPressed = false;

    public Animator anim;

    private void Start()
    {
        if (buttonCap != null) initialPosition = buttonCap.localPosition;
    }

    private void Update()
    {
        CheckForObjects();
    }

    private void CheckForObjects()
    {
        Vector3 center = transform.position + transform.TransformDirection(detectionBoxOffset);
        Collider[] colliders = Physics.OverlapBox(center, detectionBoxSize / 2f, transform.rotation, objectMask);
        bool hasObjectOnTop = false;
        foreach (var col in colliders)
        {
            if (col.gameObject.activeInHierarchy && !col.isTrigger)
            {
                hasObjectOnTop = true;
                break;
            }
        }
        if (hasObjectOnTop && !isPressed)
        {
            PressButton();
        }
        else if (!hasObjectOnTop && isPressed)
        {
            ReleaseButton();
        }
    }

    private void PressButton()
    {
        isPressed = true;
        if (buttonCap != null) buttonCap.localPosition = initialPosition + pressedOffset;
        OnButtonPressed.Invoke();
        Debug.Log("¡Botón Presionado!");
    }

    private void ReleaseButton()
    {
        isPressed = false;
        if (buttonCap != null) buttonCap.localPosition = initialPosition;
        OnButtonReleased.Invoke();
        Debug.Log("Botón Soltado");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.TransformDirection(detectionBoxOffset), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, detectionBoxSize);
    }

    public void AbrirPuerta()
    {
        anim.SetTrigger("Abrir");
    }

    public void CerrarPuerta()
    {
        anim.SetTrigger("Cerrar");
    }
}