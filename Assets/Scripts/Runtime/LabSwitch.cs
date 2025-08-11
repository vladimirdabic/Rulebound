using UnityEngine;

public class LabSwitch : MonoBehaviour, IInteractable
{
    public Sprite SwitchEnabledSprite;
    public GameObject ObjectToDestroy;
    public GameObject RuleObject;

    public void OnInteract()
    {
        if (!enabled) return;

        GetComponent<SpriteRenderer>().sprite = SwitchEnabledSprite;
        RuleObject.GetComponent<JSONDialogueInteraction>().enabled = false;
        RuleObject.GetComponent<PickupItem>().enabled = true;
        Destroy(ObjectToDestroy);

        enabled = false;
    }
}
