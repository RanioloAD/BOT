using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoGiratorio : MonoBehaviour
{

    public float rotateSpeed = 90f;
    public float floatAmplitude = 0.2f; 
    public float floatFrequency = 2f;   

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        float newY = startPos.y + (Mathf.Sin(Time.time * floatFrequency) * floatAmplitude);
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}