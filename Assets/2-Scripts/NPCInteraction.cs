using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct DialogueLine
{
    public string speakerName;
    public Sprite speakerPhoto;
    [TextArea(2, 5)] public string message;
    public UnityEvent onLineStart; 
}

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [Header("Diálogo y Eventos")]
    public DialogueLine[] lines;
    public UnityEvent onDialogueComplete;

    public void Interact()
    {
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null && lines.Length > 0)
        {
            ui.StartDialogue(lines, onDialogueComplete);
        }
    }
}