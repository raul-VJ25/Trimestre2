using UnityEngine;

// Gema de experiencia que otorga XP al jugador
public class XPGemObject : CellObject
{
    public int XPAmount = 15;

    // Al ser recogida, otorga XP y se destruye
    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeXP(XPAmount);
    }
}