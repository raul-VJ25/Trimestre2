using UnityEngine;
using UnityEngine.Serialization;

public class FoodObject : CellObject
{
    [FormerlySerializedAs("AmountGranted")][SerializeField] private int m_AmountGranted = 10;

    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeFood(m_AmountGranted);
        GameEvents.RaiseFoodPicked(m_AmountGranted);
    }
}