using UnityEngine;
using UnityEngine.InputSystem;

// Sistema de gestión de pausa
public class PauseSystem : MonoBehaviour
{
    private bool m_IsPaused = false;
    public bool IsPaused => m_IsPaused;

    public event System.Action OnGamePaused;
    public event System.Action OnGameResumed;

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    public void TogglePause()
    {
        if (m_IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (m_IsPaused) return;
        m_IsPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!m_IsPaused) return;
        m_IsPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke();
    }
}