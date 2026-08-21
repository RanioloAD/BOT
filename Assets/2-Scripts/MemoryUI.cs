using System.Collections;
using UnityEngine;
using TMPro;

public class MemoryUI : MonoBehaviour
{
    public static MemoryUI Instance { get; private set; }

    [Header("UI Components")]
    public GameObject memoryPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;

    [Header("Textos por Fragmento")]
    [TextArea(1, 2)] public string[] titulos = {
        "FRAGMENTO 1: ORIGEN",
        "FRAGMENTO 2: GUERRA",
        "FRAGMENTO 3: ALIANZA",
        "FRAGMENTO 4: VERDAD",
        "FRAGMENTO 5: DECISIÓN"
    };

    [TextArea(2, 5)] public string[] mensajes = {
        "Fui creado para ayudar.",
        "Todo cambió cuando los robots se rebelaron.",
        "Los humanos confiaron en nosotros.",
        "\"Los humanos están destruyendo el mundo que compartimos.\"\nDestruyen ecosistemas, contaminan el planeta, agotan recursos y destruyen otras formas de vida.",
        "Debo elegir un bando."
    };

    private bool leyendoMemoria = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Si el panel está abierto y apretás ESPACIO o ENTER, se cierra y el juego sigue
        if (leyendoMemoria && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E)))
        {
            CerrarFragmento();
        }
    }

    public void MostrarFragmento(int numeroFragmento)
    {
        int index = Mathf.Clamp(numeroFragmento - 1, 0, mensajes.Length - 1);

        if (titleText != null) titleText.text = titulos[index];
        if (messageText != null) messageText.text = mensajes[index];

        // 1. Mostrar la UI
        if (memoryPanel != null) memoryPanel.SetActive(true);
        
        // 2. PAUSAR EL TIEMPO
        Time.timeScale = 0f; 
        leyendoMemoria = true;

        // Opcional: Agregar al texto de la UI un "Presiona ESPACIO para continuar..." parpadeando
    }

    private void CerrarFragmento()
    {
        leyendoMemoria = false;
        
        // 1. Ocultar la UI
        if (memoryPanel != null) memoryPanel.SetActive(false);

        // 2. DESPAUSAR EL TIEMPO
        Time.timeScale = 1f;
    }
}