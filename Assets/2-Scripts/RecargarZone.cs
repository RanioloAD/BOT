using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecargarZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.RecargarBateria(10);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.RecargarBateria(10);
        }
    }

}
