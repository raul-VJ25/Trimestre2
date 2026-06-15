using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Gestor de creacion y mejora de personajes
// Permite asignar puntos de estadisticas al inicio o al subir de nivel
public class CharacterCreationManager : MonoBehaviour
{
    // Referencia al documento UI
    public UIDocument UIDocument;

    // Constantes del sistema
    private const int MAX_INITIAL_POINTS = 7;
    private const int MIN_STAT_VALUE = 1;
    private const int XP_PER_POINT = 100;

    // Elementos UI para nombre
    private TextField m_NameField;
    private Label m_NameLabel;

    // Campos de estadisticas
    private IntegerField m_StrengthField;
    private IntegerField m_AgilityField;
    private IntegerField m_IntelligenceField;
    private IntegerField m_HealthField;

    // Labels informativos
    private Label m_PointsLabel;
    private Label m_PreviewLifeLabel;
    private Label m_TitleLabel;
    private Label m_XPLabel;

    // Botones principales
    private Button m_PlayButton;
    private Button m_BackButton;
    private Button m_ExchangeXPButton;

    // Botones de incremento/decremento
    private Button m_StrPlus, m_StrMinus;
    private Button m_AgiPlus, m_AgiMinus;
    private Button m_IntPlus, m_IntMinus;
    private Button m_HpPlus, m_HpMinus;

    // Estados del juego
    private bool m_IsRetrying = false;
    private bool m_IsLevelUp = false;
    private string m_RetryName = "";
    private int m_AvailablePoints = MAX_INITIAL_POINTS;
    private int m_CurrentXP = 0;

    // Inicializacion y configuracion segun el modo de juego
    private void Start()
    {
        // Determina si es reintento o level up
        if (SessionManager.Instance != null)
        {
            m_IsRetrying = SessionManager.Instance.IsRetrying;
            m_IsLevelUp = SessionManager.Instance.IsLevelUp;

            if ((m_IsRetrying || m_IsLevelUp) && SessionManager.Instance.CurrentPlayerData != null)
            {
                m_RetryName = SessionManager.Instance.CurrentPlayerData.Name;
            }

            if (m_IsLevelUp)
            {
                m_CurrentXP = SessionManager.Instance.CurrentXP;
            }
        }

        var root = UIDocument.rootVisualElement;

        // Obtiene referencias a elementos UI
        m_TitleLabel = root.Q<Label>(className: "title");
        m_NameField = root.Q<TextField>("NameInput");
        m_NameLabel = root.Q<Label>(className: "section-label");

        m_StrengthField = root.Q<IntegerField>("StrInput");
        m_AgilityField = root.Q<IntegerField>("AgiInput");
        m_IntelligenceField = root.Q<IntegerField>("IntInput");
        m_HealthField = root.Q<IntegerField>("HpInput");

        m_PointsLabel = root.Q<Label>("PointsLabel");
        m_PreviewLifeLabel = root.Q<Label>("PreviewLife");
        m_PlayButton = root.Q<Button>("PlayButton");

        m_StrPlus = root.Q<Button>("StrPlus");
        m_StrMinus = root.Q<Button>("StrMinus");
        m_AgiPlus = root.Q<Button>("AgiPlus");
        m_AgiMinus = root.Q<Button>("AgiMinus");
        m_IntPlus = root.Q<Button>("IntPlus");
        m_IntMinus = root.Q<Button>("IntMinus");
        m_HpPlus = root.Q<Button>("HpPlus");
        m_HpMinus = root.Q<Button>("HpMinus");

        // Configura la pantalla segun el modo
        if (m_IsLevelUp)
        {
            SetupLevelUpMode();
        }
        else if (m_IsRetrying)
        {
            SetupRetryMode();
        }
        else
        {
            SetupNewGameMode();
        }

        // Hace los campos de entrada de solo lectura
        m_StrengthField.isReadOnly = true;
        m_AgilityField.isReadOnly = true;
        m_IntelligenceField.isReadOnly = true;
        m_HealthField.isReadOnly = true;

        // Asigna callbacks a botones
        m_StrPlus.clicked += () => ModifyStat(m_StrengthField, 1);
        m_StrMinus.clicked += () => ModifyStat(m_StrengthField, -1);

        m_AgiPlus.clicked += () => ModifyStat(m_AgilityField, 1);
        m_AgiMinus.clicked += () => ModifyStat(m_AgilityField, -1);

        m_IntPlus.clicked += () => ModifyStat(m_IntelligenceField, 1);
        m_IntMinus.clicked += () => ModifyStat(m_IntelligenceField, -1);

        m_HpPlus.clicked += () => ModifyStat(m_HealthField, 1);
        m_HpMinus.clicked += () => ModifyStat(m_HealthField, -1);

        m_PlayButton.clicked += OnPlayButtonClicked;

        RefreshUI();
    }

    // Configura la pantalla para modo mejora de nivel
    private void SetupLevelUpMode()
    {
        var data = SessionManager.Instance.CurrentPlayerData;

        m_StrengthField.value = data.Strength;
        m_AgilityField.value = data.Agility;
        m_IntelligenceField.value = data.Intelligence;
        m_HealthField.value = data.Health;

        m_AvailablePoints = SessionManager.Instance.AvailableSkillPoints;

        if (m_TitleLabel != null)
        {
            int night = SessionManager.Instance.CurrentNight;
            m_TitleLabel.text = $"NOCHE {night} - MEJORA";
        }

        if (m_NameField != null) m_NameField.style.display = DisplayStyle.None;
        if (m_NameLabel != null) m_NameLabel.style.display = DisplayStyle.None;

        CreateXPUI();

        m_PlayButton.text = "CONTINUAR";
        m_PlayButton.SetEnabled(true);
    }

    // Crea la interfaz de intercambio de XP por puntos
    private void CreateXPUI()
    {
        var root = UIDocument.rootVisualElement;
        var card = root.Q<VisualElement>(className: "card");

        if (card == null) return;

        var xpContainer = new VisualElement();
        xpContainer.style.marginTop = 15;
        xpContainer.style.marginBottom = 10;
        xpContainer.style.paddingTop = 10;
        xpContainer.style.paddingBottom = 10;
        xpContainer.style.paddingLeft = 15;
        xpContainer.style.paddingRight = 15;
        xpContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
        xpContainer.style.borderTopLeftRadius = 8;
        xpContainer.style.borderTopRightRadius = 8;
        xpContainer.style.borderBottomLeftRadius = 8;
        xpContainer.style.borderBottomRightRadius = 8;

        m_XPLabel = new Label($"XP: {m_CurrentXP}");
        m_XPLabel.style.color = new StyleColor(new Color(0.5f, 1f, 0.5f));
        m_XPLabel.style.fontSize = 12;
        m_XPLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        m_XPLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        m_XPLabel.style.marginBottom = 10;
        xpContainer.Add(m_XPLabel);

        m_ExchangeXPButton = new Button();
        m_ExchangeXPButton.text = $"Cambiar {XP_PER_POINT} XP por 1 Punto";
        m_ExchangeXPButton.AddToClassList("pause-btn");
        m_ExchangeXPButton.style.width = Length.Percent(100);
        m_ExchangeXPButton.style.height = 38;
        m_ExchangeXPButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.2f));
        m_ExchangeXPButton.clicked += OnExchangeXPClicked;
        xpContainer.Add(m_ExchangeXPButton);

        var playBtnIndex = card.IndexOf(m_PlayButton);
        if (playBtnIndex >= 0)
        {
            card.Insert(playBtnIndex, xpContainer);
        }
        else
        {
            card.Add(xpContainer);
        }

        UpdateExchangeButton();
    }

    // Intercambia XP por puntos de habilidad
    private void OnExchangeXPClicked()
    {
        if (m_CurrentXP >= XP_PER_POINT)
        {
            m_CurrentXP -= XP_PER_POINT;
            m_AvailablePoints++;

            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.CurrentXP = m_CurrentXP;
                SessionManager.Instance.AvailableSkillPoints = m_AvailablePoints;
            }

            UpdateExchangeButton();
            RefreshUI();
        }
    }

    // Actualiza el estado del boton de intercambio
    private void UpdateExchangeButton()
    {
        if (m_ExchangeXPButton != null)
        {
            m_ExchangeXPButton.SetEnabled(m_CurrentXP >= XP_PER_POINT);
        }

        if (m_XPLabel != null)
        {
            m_XPLabel.text = $"XP: {m_CurrentXP}";
        }
    }

    // Configura la pantalla para modo reintento
    private void SetupRetryMode()
    {
        m_StrengthField.value = MIN_STAT_VALUE;
        m_AgilityField.value = MIN_STAT_VALUE;
        m_IntelligenceField.value = MIN_STAT_VALUE;
        m_HealthField.value = MIN_STAT_VALUE;
        m_AvailablePoints = MAX_INITIAL_POINTS;

        if (m_TitleLabel != null)
        {
            m_TitleLabel.text = "REASIGNAR PUNTOS";
        }

        if (m_NameField != null) m_NameField.style.display = DisplayStyle.None;
        if (m_NameLabel != null) m_NameLabel.style.display = DisplayStyle.None;

        m_PlayButton.text = "REINTENTAR";
        m_PlayButton.SetEnabled(true);
    }

    // Configura la pantalla para nueva partida
    private void SetupNewGameMode()
    {
        m_StrengthField.value = MIN_STAT_VALUE;
        m_AgilityField.value = MIN_STAT_VALUE;
        m_IntelligenceField.value = MIN_STAT_VALUE;
        m_HealthField.value = MIN_STAT_VALUE;
        m_AvailablePoints = MAX_INITIAL_POINTS;

        m_NameField.RegisterValueChangedCallback(OnNameChanged);
        CreateButtonRow();
    }

    // Crea la fila de botones Volver y Jugar
    private void CreateButtonRow()
    {
        var playParent = m_PlayButton.parent;

        var buttonRow = new VisualElement();
        buttonRow.style.flexDirection = FlexDirection.Row;
        buttonRow.style.justifyContent = Justify.SpaceBetween;
        buttonRow.style.marginTop = 25;

        m_BackButton = new Button { text = "Volver" };
        m_BackButton.AddToClassList("pause-btn");
        m_BackButton.AddToClassList("pause-btn-exit");
        m_BackButton.style.width = 120;
        m_BackButton.style.height = 45;
        m_BackButton.style.marginRight = 10;
        m_BackButton.clicked += OnBackButtonClicked;

        m_PlayButton.style.marginTop = 0;
        m_PlayButton.style.flexGrow = 1;

        playParent.Remove(m_PlayButton);

        buttonRow.Add(m_BackButton);
        buttonRow.Add(m_PlayButton);

        playParent.Add(buttonRow);
    }

    // Valida el nombre cuando cambia
    private void OnNameChanged(ChangeEvent<string> evt)
    {
        ValidatePlayButton();
    }

    // Habilita o deshabilita el boton de jugar segun validaciones
    private void ValidatePlayButton()
    {
        if (m_IsRetrying || m_IsLevelUp)
        {
            m_PlayButton.SetEnabled(true);
        }
        else
        {
            bool isNameValid = !string.IsNullOrWhiteSpace(m_NameField.value);
            m_PlayButton.SetEnabled(isNameValid);
        }
    }

    // Modifica el valor de una estadistica
    private void ModifyStat(IntegerField field, int amount)
    {
        int currentValue = field.value;

        if (amount > 0)
        {
            int basePoints = m_IsLevelUp ?
                (SessionManager.Instance.CurrentPlayerData.Strength +
                 SessionManager.Instance.CurrentPlayerData.Agility +
                 SessionManager.Instance.CurrentPlayerData.Intelligence +
                 SessionManager.Instance.CurrentPlayerData.Health) :
                (MIN_STAT_VALUE * 4);

            int currentTotal = m_StrengthField.value + m_AgilityField.value + m_IntelligenceField.value + m_HealthField.value;
            int pointsSpent = currentTotal - basePoints;

            if (pointsSpent >= m_AvailablePoints) return;
        }
        else
        {
            if (m_IsLevelUp)
            {
                var data = SessionManager.Instance.CurrentPlayerData;
                int minValue = field == m_StrengthField ? data.Strength :
                               field == m_AgilityField ? data.Agility :
                               field == m_IntelligenceField ? data.Intelligence :
                               data.Health;
                if (currentValue <= minValue) return;
            }
            else
            {
                if (currentValue <= MIN_STAT_VALUE) return;
            }
        }

        field.value += amount;
        RefreshUI();
    }

    // Actualiza todos los elementos de la UI
    private void RefreshUI()
    {
        int basePoints;
        if (m_IsLevelUp)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            basePoints = data.Strength + data.Agility + data.Intelligence + data.Health;
        }
        else
        {
            basePoints = MIN_STAT_VALUE * 4;
        }

        int currentTotal = m_StrengthField.value + m_AgilityField.value + m_IntelligenceField.value + m_HealthField.value;
        int pointsSpent = currentTotal - basePoints;
        int pointsLeft = m_AvailablePoints - pointsSpent;

        m_PointsLabel.text = $"Puntos disponibles: {pointsLeft}";

        PlayerData tempData = new PlayerData(
            "Temp",
            m_StrengthField.value,
            m_AgilityField.value,
            m_IntelligenceField.value,
            m_HealthField.value
        );

        if (m_PreviewLifeLabel != null)
        {
            m_PreviewLifeLabel.text = "Vida Inicial: " + tempData.StartingLife;
        }

        UpdateButtonStates(pointsLeft);
        ValidatePlayButton();
    }

    // Actualiza el estado habilitado/deshabilitado de botones
    private void UpdateButtonStates(int pointsLeft)
    {
        if (m_IsLevelUp)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            m_StrMinus.SetEnabled(m_StrengthField.value > data.Strength);
            m_AgiMinus.SetEnabled(m_AgilityField.value > data.Agility);
            m_IntMinus.SetEnabled(m_IntelligenceField.value > data.Intelligence);
            m_HpMinus.SetEnabled(m_HealthField.value > data.Health);
        }
        else
        {
            m_StrMinus.SetEnabled(m_StrengthField.value > MIN_STAT_VALUE);
            m_AgiMinus.SetEnabled(m_AgilityField.value > MIN_STAT_VALUE);
            m_IntMinus.SetEnabled(m_IntelligenceField.value > MIN_STAT_VALUE);
            m_HpMinus.SetEnabled(m_HealthField.value > MIN_STAT_VALUE);
        }

        m_StrPlus.SetEnabled(pointsLeft > 0);
        m_AgiPlus.SetEnabled(pointsLeft > 0);
        m_IntPlus.SetEnabled(pointsLeft > 0);
        m_HpPlus.SetEnabled(pointsLeft > 0);
    }

    // Maneja el click del boton Jugar/Continuar
    private void OnPlayButtonClicked()
    {
        string playerName;

        if (m_IsRetrying || m_IsLevelUp)
        {
            playerName = m_RetryName;
        }
        else
        {
            playerName = m_NameField.value;
        }

        PlayerData newData = new PlayerData(
            playerName,
            m_StrengthField.value,
            m_AgilityField.value,
            m_IntelligenceField.value,
            m_HealthField.value
        );

        if (SessionManager.Instance != null)
        {
            var oldData = SessionManager.Instance.CurrentPlayerData;

            if (oldData != null && (m_IsRetrying || m_IsLevelUp))
            {
                newData.BestLevel = oldData.BestLevel;
                newData.BestXP = oldData.BestXP;
                newData.Achievements = oldData.Achievements;

                if (m_IsLevelUp)
                {
                    newData.SavedLife = oldData.SavedLife;
                    newData.SavedNight = SessionManager.Instance.CurrentNight;

                    int basePoints = oldData.Strength + oldData.Agility + oldData.Intelligence + oldData.Health;
                    int newTotal = m_StrengthField.value + m_AgilityField.value + m_IntelligenceField.value + m_HealthField.value;
                    int pointsUsed = newTotal - basePoints;

                    SessionManager.Instance.AvailableSkillPoints -= pointsUsed;
                    SessionManager.Instance.CurrentXP = m_CurrentXP;
                }
            }

            SessionManager.Instance.CurrentPlayerData = newData;
            SessionManager.Instance.IsRetrying = false;
        }

        SceneManager.LoadScene("GameScene");
    }

    // Vuelve al menu principal
    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }
}