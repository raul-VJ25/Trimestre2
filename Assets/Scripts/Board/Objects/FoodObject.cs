using UnityEngine;
using UnityEngine.Serialization;

public class FoodObject : CellObject, IInteractable
{
    [FormerlySerializedAs("AmountGranted")][SerializeField] private int m_AmountGranted = 10;

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