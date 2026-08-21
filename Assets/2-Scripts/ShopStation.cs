using UnityEngine;

public class ShopStation : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.AbrirShop();
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró la instancia de ShopManager en la escena.");
        }
    }
}