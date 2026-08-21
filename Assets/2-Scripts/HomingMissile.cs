using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [Header("Parámetros de Vuelo")]
    public float speed = 20f;
    public float turnSpeed = 600f; // Velocidad de giro precisa
    public float detectionRadius = 30f;
    [Range(0, 180)] public float maxDetectionAngle = 90f; // Ángulo de visión para buscar
    public LayerMask targetLayer;

    [Header("Destrucción / Explosión")]
    public float lifetime = 5f;

    private Transform target;

    void Start()
    {
        BuscarObjetivoMasCercano();
        Destroy(gameObject, lifetime); // Limpieza si vuela al vacío
    }

    void Update()
    {
        if (target != null)
        {
            // Vector hacia el objetivo
            Vector3 targetDirection = (target.position - transform.position).normalized;

            // Rotación suave pero ultra precisa hacia la dirección del objetivo
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Avanza siempre hacia su propio 'frente' a velocidad constante
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    void BuscarObjetivoMasCercano()
    {
        Collider[] candidates = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        float minDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (Collider col in candidates)
        {
            Vector3 dirToTarget = (col.transform.position - transform.position).normalized;

            // Validar que el objetivo esté dentro del cono frontal del misil/jugador
            if (Vector3.Angle(transform.forward, dirToTarget) <= maxDetectionAngle)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestTarget = col.transform;
                }
            }
        }

        target = bestTarget;

        if (target != null)
        {
            Debug.Log($"🚀 [MISIL] Objetivo fijado: {target.name} a {minDistance:F1}m");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Explotar(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explotar(collision.gameObject);
    }

    void Explotar(GameObject hitObject)
    {
        Debug.Log($"💥 [EXPLOSIÓN] Misil impactó contra: {hitObject.name}");

        // Acá podés instanciar un prefab de partículas de explosión en el futuro
        // Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);

        // Desaparición inmediata del misil
        Destroy(gameObject);
    }
}