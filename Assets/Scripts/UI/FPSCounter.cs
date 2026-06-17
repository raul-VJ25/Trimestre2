using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public static FPSCounter Instance { get; private set; }

    private float m_DeltaTime = 0f;
    private bool m_ShowFPS = false;
    private GUIStyle m_Style;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        m_ShowFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

        m_Style = new GUIStyle();
        m_Style.fontSize = 16;
        m_Style.fontStyle = FontStyle.Bold;
        m_Style.normal.textColor = Color.green;
        m_Style.padding = new RectOffset(10, 10, 5, 5);
    }

    private void Update()
    {
        if (m_ShowFPS)
        {
            m_DeltaTime += (Time.unscaledDeltaTime - m_DeltaTime) * 0.1f;
        }
    }

    // Dibuja los FPS en pantalla
    private void OnGUI()
    {
        if (!m_ShowFPS) return;

        int fps = Mathf.RoundToInt(1.0f / m_DeltaTime);
        string text = "fps: " + fps;

        Rect bgRect = new Rect(10, 10, 80, 25);
        GUI.Box(bgRect, GUIContent.none);

        GUI.Label(bgRect, text, m_Style);
    }

    // Activa o desactiva la visualizacion de FPS
    public void SetShowFPS(bool show)
    {
        m_ShowFPS = show;
        PlayerPrefs.SetInt("ShowFPS", show ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsShowingFPS()
    {
        return m_ShowFPS;
    }
}