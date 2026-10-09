using System;
using UnityEngine;

/// <summary>
/// Gestiona el estado global del juego.
/// Implementa Singleton y notifica cambios mediante eventos (Observer).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> OnEnergyChanged;

    public int Energy { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Energy = 0;
    }

    public void AddEnergy(int amount = 1)
    {
        Energy += amount;

        OnEnergyChanged?.Invoke(Energy);
    }
}