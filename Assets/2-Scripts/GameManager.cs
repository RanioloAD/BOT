using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float battery;
    public float maxBattery;

    public int materiales = 0;
    public int fragmentos = 0;
    public int fragmentosTotales = 5;

    public bool grab = false;
    public bool flashlight = false;
    public bool fan = false;
    public bool antena = false;
    public bool weapon = false;
    public bool run = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AgregarMateriales(int amount)
    {
        materiales += amount;
    }

    public void AgregarFragmento(int amount)
    {
        fragmentos += amount;
        Debug.Log("Fragmento conseguido");

        if (fragmentos >= fragmentosTotales)
        {
            Debug.Log("Todos los fragmentos recuperados");
        }
    }

    public void ConsumirBateria(float rate)
    {
        battery -= rate * Time.deltaTime;
        battery = Mathf.Clamp(battery, 0f, maxBattery);
        UIManager.Instance.batteryImage.fillAmount = battery / maxBattery;
    }

    public void RecargarBateria(float rate)
    {
        battery += rate * Time.deltaTime;
        battery = Mathf.Clamp(battery, 0f, maxBattery);
        UIManager.Instance.batteryImage.fillAmount = battery / maxBattery;
    }

    public void GastoRapido(float rate)
    {
        battery -= rate;
        battery = Mathf.Clamp(battery, 0f, maxBattery);
        UIManager.Instance.batteryImage.fillAmount = battery / maxBattery;
    }


    public void UnlockGrab()
    {
        grab = true;
    }

    public void UnlockFlashlight()
    {
        flashlight = true;
    }

    public void UnlockFan()
    {
        fan = true;
    }

    public void UnlockAntena()
    {
        antena = true;
    }

    public void UnlockWeapon()
    {
        weapon = true;
    }
    public void UnlockRun()
    {
        run = true;
    }

}
