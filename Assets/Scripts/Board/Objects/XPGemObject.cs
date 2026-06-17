using UnityEngine;
using UnityEngine.Serialization;

public class XPGemObject : CellObject, IInteractable
{
    [FormerlySerializedAs("XPAmount")][SerializeField] private int m_XPAmount = 15;

    public void Interact()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeXP(m_XPAmount);
    }

    public override void PlayerEntered()
    {
        Interact();
    }
}