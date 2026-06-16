// Comida que restaura vida al jugador
public class FoodObject : CellObject
{
    public int AmountGranted = 10;

    // Al entrar el jugador, otorga vida y se destruye
    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeFood(AmountGranted);
        GameEvents.RaiseFoodPicked(AmountGranted);
    }
}