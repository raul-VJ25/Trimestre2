using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UICharacterCreationManager : MonoBehaviour
{
    public static UICharacterCreationManager Instance { get; private set; }

    [Header("UIDocument")]
    public UIDocument UIDocument;

    [Header("Elementos UI - Nombre")]
    private TextField m_NameField;
    private Label m_NameLabel;

    [Header("Campos de Estadísticas")]
    private IntegerField m_StrengthField;
    private IntegerField m_AgilityField;
    private IntegerField m_IntelligenceField;
    private IntegerField m_HealthField;

    [Header("Labels Informativos")]
    private Label m_PointsLabel;
    private Label m_PreviewLifeLabel;
    private Label m_TitleLabel;
    private Label m_XPLabel;

    [Header("Botones Principales")]
    private Button m_PlayButton;
    private Button m_BackButton;
    private Button m_ExchangeXPButton;

    [Header("Botones +/-")]
    private Button m_StrPlus, m_StrMinus;
    private Button m_AgiPlus, m_AgiMinus;
    private Button m_IntPlus, m_IntMinus;
    private Button m_HpPlus, m_HpMinus;

    [Header("Contenedores")]
    private VisualElement m_XPContainer;

    private const int MAX_INITIAL_POINTS = 7;
    private const int MIN_STAT_VALUE = 1;
    private const int XP_PER_POINT = 100;

    private bool m_IsRetrying = false;
    private bool m_IsLevelUp = false;
    private string m_RetryName = "";
    private int m_AvailablePoints = MAX_INITIAL_POINTS;
    private int m_CurrentXP = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (UIDocument == null)
        {
            Debug.LogError("UICharacterCreationManager: No se ha asignado el UIDocument");
            return;
        }

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

        m_StrengthField.isReadOnly = true;
        m_AgilityField.isReadOnly = true;
        m_IntelligenceField.isReadOnly = true;
        m_HealthField.isReadOnly = true;

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

    void SetupLevelUpMode()
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

    void CreateXPUI()
    {
        var root = UIDocument.rootVisualElement;
        var card = root.Q<VisualElement>(className: "card");
        if (card == null) return;

        m_XPContainer = new VisualElement();
        m_XPContainer.style.marginTop = 15;
        m_XPContainer.style.marginBottom = 10;
        m_XPContainer.style.paddingTop = 10;
        m_XPContainer.style.paddingBottom = 10;
        m_XPContainer.style.paddingLeft = 15;
        m_XPContainer.style.paddingRight = 15;
        m_XPContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
        m_XPContainer.style.borderTopLeftRadius = 8;
        m_XPContainer.style.borderTopRightRadius = 8;
        m_XPContainer.style.borderBottomLeftRadius = 8;
        m_XPContainer.style.borderBottomRightRadius = 8;

        m_XPLabel = new Label($"XP: {m_CurrentXP}");
        m_XPLabel.style.color = new StyleColor(new Color(0.5f, 1f, 0.5f));
        m_XPLabel.style.fontSize = 12;
        m_XPLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        m_XPLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        m_XPLabel.style.marginBottom = 10;
        m_XPContainer.Add(m_XPLabel);

        m_ExchangeXPButton = new Button();
        m_ExchangeXPButton.text = $"Cambiar {XP_PER_POINT} XP por 1 Punto";
        m_ExchangeXPButton.AddToClassList("pause-btn");
        m_ExchangeXPButton.style.width = Length.Percent(100);
        m_ExchangeXPButton.style.height = 38;
        m_ExchangeXPButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.2f));
        m_ExchangeXPButton.clicked += OnExchangeXPClicked;
        m_XPContainer.Add(m_ExchangeXPButton);

        var playBtnIndex = card.IndexOf(m_PlayButton);
        if (playBtnIndex >= 0)
        {
            card.Insert(playBtnIndex, m_XPContainer);
        }
        else
        {
            card.Add(m_XPContainer);
        }

        UpdateExchangeButton();
    }

    void OnExchangeXPClicked()
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

    void UpdateExchangeButton()
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

    void SetupRetryMode()
    {
        m_StrengthField.value = MIN_STAT_VALUE;
        m_AgilityField.value = MIN_STAT_VALUE;
        m_IntelligenceField.value = MIN_STAT_VALUE;
        m_HealthField.value = MIN_STAT_VALUE;

        m_AvailablePoints = MAX_INITIAL_POINTS;

        if (m_TitleLabel != null) m_TitleLabel.text = "REASIGNAR PUNTOS";

        if (m_NameField != null) m_NameField.style.display = DisplayStyle.None;
        if (m_NameLabel != null) m_NameLabel.style.display = DisplayStyle.None;

        m_PlayButton.text = "REINTENTAR";
        m_PlayButton.SetEnabled(true);
    }

    void SetupNewGameMode()
    {
        m_StrengthField.value = MIN_STAT_VALUE;
        m_AgilityField.value = MIN_STAT_VALUE;
        m_IntelligenceField.value = MIN_STAT_VALUE;
        m_HealthField.value = MIN_STAT_VALUE;

        m_AvailablePoints = MAX_INITIAL_POINTS;

        if (m_NameField != null)
        {
            m_NameField.RegisterValueChangedCallback(OnNameChanged);
        }

        CreateButtonRow();
    }

    void CreateButtonRow()
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

    void OnNameChanged(ChangeEvent<string> evt)
    {
        ValidatePlayButton();
    }

    void ValidatePlayButton()
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

    void ModifyStat(IntegerField field, int amount)
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

            int currentTotal = m_StrengthField.value + m_AgilityField.value +
                              m_IntelligenceField.value + m_HealthField.value;
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

    void RefreshUI()
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

        int currentTotal = m_StrengthField.value + m_AgilityField.value +
                          m_IntelligenceField.value + m_HealthField.value;
        int pointsSpent = currentTotal - basePoints;
        int pointsLeft = m_AvailablePoints - pointsSpent;

        if (m_PointsLabel != null)
        {
            m_PointsLabel.text = $"Puntos disponibles: {pointsLeft}";
        }

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

    void UpdateButtonStates(int pointsLeft)
    {
        if (m_IsLevelUp)
        {
            var data = SessionManager.Instance.CurrentPlayerData;
            if (m_StrMinus != null) m_StrMinus.SetEnabled(m_StrengthField.value > data.Strength);
            if (m_AgiMinus != null) m_AgiMinus.SetEnabled(m_AgilityField.value > data.Agility);
            if (m_IntMinus != null) m_IntMinus.SetEnabled(m_IntelligenceField.value > data.Intelligence);
            if (m_HpMinus != null) m_HpMinus.SetEnabled(m_HealthField.value > data.Health);
        }
        else
        {
            if (m_StrMinus != null) m_StrMinus.SetEnabled(m_StrengthField.value > MIN_STAT_VALUE);
            if (m_AgiMinus != null) m_AgiMinus.SetEnabled(m_AgilityField.value > MIN_STAT_VALUE);
            if (m_IntMinus != null) m_IntMinus.SetEnabled(m_IntelligenceField.value > MIN_STAT_VALUE);
            if (m_HpMinus != null) m_HpMinus.SetEnabled(m_HealthField.value > MIN_STAT_VALUE);
        }

        if (m_StrPlus != null) m_StrPlus.SetEnabled(pointsLeft > 0);
        if (m_AgiPlus != null) m_AgiPlus.SetEnabled(pointsLeft > 0);
        if (m_IntPlus != null) m_IntPlus.SetEnabled(pointsLeft > 0);
        if (m_HpPlus != null) m_HpPlus.SetEnabled(pointsLeft > 0);
    }

    void OnPlayButtonClicked()
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

                    int basePoints = oldData.Strength + oldData.Agility +
                                  oldData.Intelligence + oldData.Health;
                    int newTotal = m_StrengthField.value + m_AgilityField.value +
                                  m_IntelligenceField.value + m_HealthField.value;
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

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    // =====================================================
    // MÉTODOS PÚBLICOS PARA CHARACTERCREATIONMANAGER
    // =====================================================

    public void InitializeUI(bool isRetrying, bool isLevelUp, string retryName,
                             int availablePoints, int currentXP)
    {
        // Ya se inicializa en Start
    }

    public void UpdateStatsUI(int strength, int agility, int intelligence,
                              int health, int pointsLeft)
    {
        if (m_StrengthField != null) m_StrengthField.value = strength;
        if (m_AgilityField != null) m_AgilityField.value = agility;
        if (m_IntelligenceField != null) m_IntelligenceField.value = intelligence;
        if (m_HealthField != null) m_HealthField.value = health;
        if (m_PointsLabel != null) m_PointsLabel.text = $"Puntos disponibles: {pointsLeft}";
    }

    public void SetPlayButtonEnabled(bool enabled)
    {
        if (m_PlayButton != null) m_PlayButton.SetEnabled(enabled);
    }
}