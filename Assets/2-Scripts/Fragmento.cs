using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fragmento : MonoBehaviour
{
    bool unaVez;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !unaVez)
        {
            unaVez = true;
            GameManager.Instance.AgregarFragmento(1);
            UIManager.Instance.ActualizarFragmentos(GameManager.Instance.fragmentos,GameManager.Instance.fragmentosTotales);
            Destroy(gameObject);
        }
    }
}
