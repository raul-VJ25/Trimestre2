using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnEnemyKilled;
    public static event Action OnWallDestroyed;
    public static event Action<int> OnFoodPicked;
    public static event Action OnPlayerStep;

    public static void RaiseEnemyKilled()
    {
        OnEnemyKilled?.Invoke();
    }

    public static void RaiseWallDestroyed()
    {
        OnWallDestroyed?.Invoke();
    }

    public static void RaiseFoodPicked(int amount)
    {
        OnFoodPicked?.Invoke(amount);
    }

    public static void RaisePlayerStep()
    {
        OnPlayerStep?.Invoke();
    }
}