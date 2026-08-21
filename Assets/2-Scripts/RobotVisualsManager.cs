using UnityEngine;

public class RobotVisualsManager : MonoBehaviour
{
    public static RobotVisualsManager Instance { get; private set; }

    [Header("Referencias 3D de las Partes")]
    public GameObject grabPart;
    public GameObject flashlightPart;
    public GameObject fanPart;
    public GameObject antenaPart;
    public GameObject weaponPart;
    public GameObject runPart;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ActualizarVisuales();
    }

    public void ActualizarVisuales()
    {
        if (GameManager.Instance == null) return;

        if (grabPart != null) grabPart.SetActive(GameManager.Instance.grab);
        if (flashlightPart != null) flashlightPart.SetActive(GameManager.Instance.flashlight);
        if (fanPart != null) fanPart.SetActive(GameManager.Instance.fan);
        if (antenaPart != null) antenaPart.SetActive(GameManager.Instance.antena);
        if (weaponPart != null) weaponPart.SetActive(GameManager.Instance.weapon);
        if (runPart != null) runPart.SetActive(GameManager.Instance.run);
    }
}