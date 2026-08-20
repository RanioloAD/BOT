using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Material : MonoBehaviour
{
    bool unaVez;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !unaVez)
        {
            unaVez = true;
            GameManager.Instance.AgregarMateriales(1);
            UIManager.Instance.ActualizarMateriales(GameManager.Instance.materiales);
            Destroy(gameObject);
        }
    }
}
