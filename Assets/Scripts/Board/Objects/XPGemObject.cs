using UnityEngine;
using UnityEngine.Serialization;

// Gema de experiencia que otorga XP al jugador
public class XPGemObject : CellObject, IInteractable
{
    [FormerlySerializedAs("XPAmount")][SerializeField] private int m_XPAmount = 15;

    // IMPLEMENTACIÓN DE LA INTERFAZ IInteractable
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