/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 5;
    public float turnSmoothTime = 0.1f;
    public Transform cam;

    float turnSmoothVelocity;


    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, 0, v).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0, angle, 0);
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
    }
}
*/ 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movimiento Base")]
    public CharacterController controller;
    public float speed = 5f;
    public float turnSmoothTime = 0.1f;
    public Transform cam;

    [Header("Juicy Movement (Squash & Stretch)")]
    public Transform modelMesh; // Asigna aquí el objeto hijo que contiene la malla/modelo 3D del robot
    public float stretchAmount = 0.15f; // Cuánto se deforma (0.15 = 15%)
    public float stretchSpeed = 12f;    // Velocidad de respuesta de la elasticidad
    public float bounceSpeed = 15f;     // Frecuencia del rebote al caminar

    private float turnSmoothVelocity;
    private Vector3 initialScale;
    private bool isMoving;

    void Start()
    {
        // Si no asignas manualmente el modelMesh, toma el primer hijo del GameObject
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
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, 0, v).normalized;
        bool wasMoving = isMoving;
        isMoving = direction.magnitude >= 0.1f;

        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            
            transform.rotation = Quaternion.Euler(0, angle, 0);
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        // Aplicar la deformación elástica al modelo
        ApplyJuicySquashAndStretch();
    }

    void ApplyJuicySquashAndStretch()
    {
        if (modelMesh == null) return;

        Vector3 targetScale = initialScale;

        if (isMoving)
        {
            // Crea un rebote rítmico mientras avanza
            float bounce = Mathf.Sin(Time.time * bounceSpeed) * stretchAmount;
            
            // Cuando sube la escala Y (se estira), X y Z se achican para conservar volumen
            targetScale.y = initialScale.y + bounce;
            targetScale.x = initialScale.x - (bounce * 0.5f);
            targetScale.z = initialScale.z - (bounce * 0.5f);
        }

        // Interpola suavemente la escala actual hacia la escala objetivo
        modelMesh.localScale = Vector3.Lerp(modelMesh.localScale, targetScale, Time.deltaTime * stretchSpeed);
    }
}