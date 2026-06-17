using UnityEngine;
using UnityEngine.Serialization;

// Comida que restaura vida al jugador
public class FoodObject : CellObject, IInteractable
{
    [FormerlySerializedAs("AmountGranted")][SerializeField] private int m_AmountGranted = 10;

    // IMPLEMENTACIÓN DE LA INTERFAZ IInteractable
    public void Interact()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeFood(m_AmountGranted);
        GameEvents.RaiseFoodPicked(m_AmountGranted);
    }

    public override void PlayerEntered()
    {
        Interact();
    }
}