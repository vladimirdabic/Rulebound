using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueInteraction : MonoBehaviour, IInteractable
{
    public Dialogue Dialogue;

    public void OnInteract()
    {
        DialogueSystem.Instance.PlayDialogue(Dialogue);
    }
}
