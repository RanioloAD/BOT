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

    public Transform modelMesh; 
    public float stretchAmount = 0.15f; 
    public float stretchSpeed = 12f;    
    public float bounceSpeed = 15f;     

    private float turnSmoothVelocity;
    private Vector3 initialScale;
    private bool isMoving;

    void Start()
    {
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

            GameManager.Instance.ConsumirBateria(3);
        }
        ApplyJuicySquashAndStretch();
    }

    void ApplyJuicySquashAndStretch()
    {
        if (modelMesh == null) return;

        Vector3 targetScale = initialScale;

        if (isMoving)
        {
            float bounce = Mathf.Sin(Time.time * bounceSpeed) * stretchAmount;
            targetScale.y = initialScale.y + bounce;
            targetScale.x = initialScale.x - (bounce * 0.5f);
            targetScale.z = initialScale.z - (bounce * 0.5f);
        }
        modelMesh.localScale = Vector3.Lerp(modelMesh.localScale, targetScale, Time.deltaTime * stretchSpeed);
    }
}