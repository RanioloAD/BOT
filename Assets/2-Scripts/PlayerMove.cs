using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Componentes y Cámara")]
    public CharacterController controller;
    public Transform cam;

    [Header("Movimiento Base y Correr")]
    public float speed = 5f;
    public float runSpeedMultiplier = 2f; // Multiplicador al presionar Shift
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private bool isMoving;
    private bool isRunning;

    [Header("Gravedad y Salto")]
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float sphereRadius = 0.3f;
    public LayerMask groundMask;
    public float jumpHeight = 3f;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Modo Vuelo (Fan Upgrade)")]
    public float flySpeedVertical = 5f;
    private bool isFlying = false;
    private bool canDoubleJump = false;

    [Header("Efectos Visuales")]
    public Transform modelMesh;
    public float stretchAmount = 0.15f;
    public float stretchSpeed = 12f;
    public float bounceSpeed = 15f;
    private Vector3 initialScale;

    [Header("Cohetes (Rockets Upgrade)")]
    public GameObject missilePrefab;
    public Transform missileSpawnPoint;
    public float fireRate = 0.8f;
    private float nextFireTime;

    [Header("Antena (Radar Upgrade)")]
    public float radarRadius = 25f;
    public LayerMask interactableLayer;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();

        if (modelMesh == null && transform.childCount > 0)
        {
            modelMesh = transform.GetChild(0);
        }

        if (modelMesh != null)
        {
            initialScale = modelMesh.localScale;
        }
        else
        {
            initialScale = Vector3.one;
        }
    }

    void Update()
    {
        ManejarDisparoCohetes();
        ManejarAntena();
        VerificarSuelo();

        Vector3 finalMove = Vector3.zero;

        // 1. Desplazamiento Horizontal (WASD + Shift)
        Vector3 horizontalMove = CalcularMovimientoHorizontal();
        finalMove += horizontalMove;

        // 2. Control de Salto / Vuelo
        ProcesarEntradaEspacio();

        if (isFlying)
        {
            // VUELO ACTIVO: Sin Gravedad
            finalMove.y = ManejarVuelo();
        }
        else
        {
            // MODO NORMAL: Con Gravedad
            velocity.y += gravity * Time.deltaTime;
            finalMove.y = velocity.y;
        }

        // Aplicar movimiento final
        controller.Move(finalMove * Time.deltaTime);

        ApplyJuicySquashAndStretch();
    }

    void VerificarSuelo()
    {
        if (groundCheck != null)
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheck.position, sphereRadius, groundMask);

            if (isGrounded && !wasGrounded && velocity.y <= 0)
            {
                Debug.Log("🟢 [PLAYER] Toco el suelo. Desactivando modo vuelo y reseteando salto.");
                isFlying = false;
                canDoubleJump = false;
                velocity.y = -2f;
            }
            else if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
    }

    Vector3 CalcularMovimientoHorizontal()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, 0, v).normalized;
        isMoving = direction.magnitude >= 0.1f;

        if (isMoving)
        {
            // Verificar si tiene el upgrade 'run' y si sostiene Shift
            bool tieneRuedas = GameManager.Instance != null && GameManager.Instance.run;
            isRunning = tieneRuedas && Input.GetKey(KeyCode.LeftShift);

            // Calcular velocidad aplicable y consumo de batería
            float currentSpeed = speed;
            float batteryDrain = 1f;

            if (isRunning)
            {
                currentSpeed *= runSpeedMultiplier;
                batteryDrain = 2.5f; // Mayor consumo al correr a fondo
            }

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0, angle, 0);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ConsumirBateria(batteryDrain);
            }

            return moveDir.normalized * currentSpeed;
        }

        isRunning = false;
        return Vector3.zero;
    }

    void ProcesarEntradaEspacio()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                canDoubleJump = true;
                Debug.Log("🦘 [PLAYER] Primer Salto.");
            }
            else if (isFlying)
            {
                isFlying = false;
                velocity.y = 0f;
                Debug.Log("🔴 [PLAYER] Apagando Ventilador.");
            }
            else if (canDoubleJump)
            {
                bool tieneMejoraFan = GameManager.Instance != null && GameManager.Instance.fan;

                if (tieneMejoraFan)
                {
                    isFlying = true;
                    canDoubleJump = false;
                    velocity.y = 0f;
                    Debug.Log("🌀 [PLAYER] ¡Doble Salto! Ventilador Activado.");
                }
                else
                {
                    Debug.LogWarning("⚠️ [PLAYER] Intento doble salto sin la mejora GameManager.Instance.fan");
                }
            }
        }
    }

    float ManejarVuelo()
    {
        float verticalFly = 0f;

        if (Input.GetKey(KeyCode.E)) verticalFly = 1f;
        if (Input.GetKey(KeyCode.Q)) verticalFly = -1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ConsumirBateria(1.5f);
        }

        return verticalFly * flySpeedVertical;
    }

    void ApplyJuicySquashAndStretch()
    {
        if (modelMesh == null) return;

        Vector3 targetScale = initialScale;

        if (isMoving || isFlying)
        {
            // Aumenta la velocidad del rebote visual al correr a toda velocidad
            float currentBounceSpeed = isRunning ? bounceSpeed * 1.8f : bounceSpeed;
            float bounce = Mathf.Sin(Time.time * currentBounceSpeed) * stretchAmount;

            targetScale.y = initialScale.y + bounce;
            targetScale.x = initialScale.x - (bounce * 0.5f);
            targetScale.z = initialScale.z - (bounce * 0.5f);
        }
        modelMesh.localScale = Vector3.Lerp(modelMesh.localScale, targetScale, Time.deltaTime * stretchSpeed);
    }


    void ManejarDisparoCohetes()
    {
        bool tieneCohetes = GameManager.Instance != null && GameManager.Instance.weapon;

        if (tieneCohetes && Input.GetMouseButtonDown(1) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            Transform spawn = missileSpawnPoint != null ? missileSpawnPoint : transform;
            Instantiate(missilePrefab, spawn.position, spawn.rotation);

            GameManager.Instance.GastoRapido(10f);
            
            Debug.Log("🚀 [PLAYER] Cohete disparado.");
        }
    }

    void ManejarAntena()
    {
        bool tieneAntena = GameManager.Instance != null && GameManager.Instance.antena;

        if (tieneAntena && Input.GetKeyDown(KeyCode.R))
        {
            Collider[] items = Physics.OverlapSphere(transform.position, radarRadius, interactableLayer);
            Debug.Log($"📡 [ANTENA] Escaneo completado. Se detectaron {items.Length} objetos cercanos.");

            // Acá podés activar un ParticleSystem, emitir un sonido o resaltar los objetos detectados
            GameManager.Instance.ConsumirBateria(2f);
        }
    }
}