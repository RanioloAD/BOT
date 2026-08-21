using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Contenedor Principal de la UI")]
    public GameObject shopPanel; // El panel general 'SHOP' o 'Backgrounds'

    [Header("Precios de Mejoras")]
    public int precioGrab = 50;
    public int precioFlashlight = 30;
    public int precioFan = 120;
    public int precioAntena = 80;
    public int precioWeapon = 200;
    public int precioRun = 100;

    [Header("Referencias a los 6 Botones del UI")]
    public Button btnGrab;
    public Button btnFlashlight;
    public Button btnFan;
    public Button btnAntena;
    public Button btnWeapon;
    public Button btnRun;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Asignar dinámicamente las funciones a cada botón de la UI
        if (btnGrab != null) btnGrab.onClick.AddListener(ComprarGrab);
        if (btnFlashlight != null) btnFlashlight.onClick.AddListener(ComprarFlashlight);
        if (btnFan != null) btnFan.onClick.AddListener(ComprarFan);
        if (btnAntena != null) btnAntena.onClick.AddListener(ComprarAntena);
        if (btnWeapon != null) btnWeapon.onClick.AddListener(ComprarWeapon);
        if (btnRun != null) btnRun.onClick.AddListener(ComprarRun);

        // Asegurar que comience cerrado al iniciar la partida
        CerrarShop();
    }

    private void Update()
    {
        // Permitir cerrar la tienda con ESC si está abierta
        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarShop();
        }
    }

    public void AbrirShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            ActualizarEstadoBotones();
            
            // Pausar o liberar cursor si es necesario
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CerrarShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            
            // Bloquear cursor nuevamente para seguir jugando
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // --- Métodos de Compra Individuaes ---

    public void ComprarGrab()
    {
        if (ProcesarCompra(precioGrab, GameManager.Instance.grab))
        {
            GameManager.Instance.UnlockGrab();
            Debug.Log("✅ Mejora 'Grab' comprada exitosamente.");
            RobotVisualsManager.Instance.ActualizarVisuales();
            ActualizarEstadoBotones();
        }
    }

    public void ComprarFlashlight()
    {
        if (ProcesarCompra(precioFlashlight, GameManager.Instance.flashlight))
        {
            GameManager.Instance.UnlockFlashlight();
            Debug.Log("✅ Mejora 'Flashlight' comprada exitosamente.");
            RobotVisualsManager.Instance.ActualizarVisuales();
            ActualizarEstadoBotones();
        }
    }

    public void ComprarFan()
    {
        if (ProcesarCompra(precioFan, GameManager.Instance.fan))
        {
            GameManager.Instance.UnlockFan();
            Debug.Log("✅ Mejora 'Fan' comprada exitosamente.");
            RobotVisualsManager.Instance.ActualizarVisuales();
            ActualizarEstadoBotones();
        }
    }

    public void ComprarAntena()
    {
        if (ProcesarCompra(precioAntena, GameManager.Instance.antena))
        {
            GameManager.Instance.UnlockAntena();
            Debug.Log("✅ Mejora 'Antena' comprada exitosamente.");
            RobotVisualsManager.Instance.ActualizarVisuales();
            ActualizarEstadoBotones();
        }
    }

    public void ComprarWeapon()
    {
        if (ProcesarCompra(precioWeapon, GameManager.Instance.weapon))
        {
            GameManager.Instance.UnlockWeapon();
            Debug.Log("✅ Mejora 'Weapon' comprada exitosamente.");
            RobotVisualsManager.Instance.ActualizarVisuales();
            ActualizarEstadoBotones();
        }
    }

    public void ComprarRun()
    {
        if (ProcesarCompra(precioRun, GameManager.Instance.run))
        {
            GameManager.Instance.UnlockRun();
            Debug.Log("✅ Mejora 'Run' comprada exitosamente.");
            RobotVisualsManager.Instance.ActualizarVisuales();
            ActualizarEstadoBotones();
        }
    }

    // --- Validación de Materiales y Estado ---

    private bool ProcesarCompra(int costo, bool yaComprado)
    {
        if (yaComprado)
        {
            Debug.LogWarning("⚠️ Esta mejora ya ha sido adquirida.");
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.materiales >= costo)
        {
            // Descontar materiales
            GameManager.Instance.materiales -= costo;
            return true;
        }

        Debug.LogWarning($"❌ Materiales insuficientes. Necesitás {costo} materiales.");
        return false;
    }

    public void ActualizarEstadoBotones()
    {
        if (GameManager.Instance == null) return;

        // Desactiva el botón si ya está comprado o si no alcanzan los materiales
        ValidarBoton(btnGrab, GameManager.Instance.grab, precioGrab);
        ValidarBoton(btnFlashlight, GameManager.Instance.flashlight, precioFlashlight);
        ValidarBoton(btnFan, GameManager.Instance.fan, precioFan);
        ValidarBoton(btnAntena, GameManager.Instance.antena, precioAntena);
        ValidarBoton(btnWeapon, GameManager.Instance.weapon, precioWeapon);
        ValidarBoton(btnRun, GameManager.Instance.run, precioRun);
    }

    private void ValidarBoton(Button btn, bool comprado, int costo)
    {
        if (btn == null) return;

        // Si ya está comprado se apaga la interactividad del botón completamente
        if (comprado)
        {
            btn.interactable = false;
        }
        else
        {
            // Si no está comprado, requiere que el jugador tenga materiales suficientes
            btn.interactable = GameManager.Instance.materiales >= costo;
        }
    }
}