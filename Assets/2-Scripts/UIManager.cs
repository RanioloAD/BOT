using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Image batteryImage;

    public TextMeshProUGUI materialesText;
    public TextMeshProUGUI fragmentosText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        materialesText.text = "Materiales = " + GameManager.Instance.materiales.ToString();
        fragmentosText.text = "Fragmentos = " + $"{GameManager.Instance.fragmentos}/{GameManager.Instance.fragmentosTotales}";
        batteryImage.fillAmount = GameManager.Instance.battery / GameManager.Instance.maxBattery;
    }

    public void ActualizarMateriales(int materiales)
    {
        materialesText.text = "Materiales = " + materiales.ToString();
    }

    public void ActualizarFragmentos(int fragments, int maxFragments)
    {
        fragmentosText.text = "Fragmentos = " + $"{fragments}/{maxFragments}";
    }


}
