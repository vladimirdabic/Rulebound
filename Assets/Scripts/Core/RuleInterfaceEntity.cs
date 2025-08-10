using UnityEngine;

public class RuleInterfaceEntity : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        RuleInterfaceSystem.Instance.OpenInterface();
    }
}
