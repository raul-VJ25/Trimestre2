using UnityEngine;
using UnityEngine.Serialization;

public class XPGemObject : CellObject
{
    [FormerlySerializedAs("XPAmount")][SerializeField] private int m_XPAmount = 15;

    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeXP(m_XPAmount);
    }
}