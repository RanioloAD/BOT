using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Stats")]
    public Image batteryImage;
    public TextMeshProUGUI materialesText;
    public TextMeshProUGUI fragmentosText;

    [Header("UI Diálogos")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI messageText;
    public Image speakerPhotoImage;
    public Button nextButton;
    public Button closeButton; 

    [Header("Efecto de Texto")]
    [SerializeField] private float typingSpeed = 0.03f; 

    private DialogueLine[] currentLines;
    private int currentLineIndex;
    private bool isDialogueActive;
    private bool isTyping; 
    private Coroutine typingCoroutine;
    private UnityEvent onCompleteCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (materialesText != null) 
            materialesText.text = "Materiales = " + GameManager.Instance.materiales.ToString();
        
        if (fragmentosText != null) 
            fragmentosText.text = "Fragmentos = " + $"{GameManager.Instance.fragmentos}/{GameManager.Instance.fragmentosTotales}";
        
        if (batteryImage != null) 
            batteryImage.fillAmount = GameManager.Instance.battery / GameManager.Instance.maxBattery;

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextLine);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(EndDialogue);
        }
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    public void StartDialogue(DialogueLine[] lines, UnityEvent onComplete)
    {
        currentLines = lines;
        onCompleteCallback = onComplete;
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        ShowLine();
    }

    private void ShowLine()
    {
        DialogueLine line = currentLines[currentLineIndex];

        if (speakerNameText != null) speakerNameText.text = line.speakerName;

        if (speakerPhotoImage != null)
        {
            speakerPhotoImage.sprite = line.speakerPhoto;
            speakerPhotoImage.gameObject.SetActive(line.speakerPhoto != null);
        }


        if (nextButton != null) nextButton.gameObject.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(line.message));

        line.onLineStart?.Invoke();
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        messageText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            messageText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        
        if (nextButton != null) nextButton.gameObject.SetActive(true);
    }

    public void NextLine()
    {
        if (!isDialogueActive) return;

        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            messageText.text = currentLines[currentLineIndex].message;
            isTyping = false;

            if (nextButton != null) nextButton.gameObject.SetActive(true);
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        isDialogueActive = false;
        isTyping = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        onCompleteCallback?.Invoke();
    }

    public void ActualizarMateriales(int materiales)
    {
        if (materialesText != null) materialesText.text = "Materiales = " + materiales.ToString();
    }

    public void ActualizarFragmentos(int fragments, int maxFragments)
    {
        if (fragmentosText != null) fragmentosText.text = "Fragmentos = " + $"{fragments}/{maxFragments}";
    }
}