using UnityEngine;
using UnityEngine.UIElements;

public class UICharacterCreationManager : MonoBehaviour
{
    public static UICharacterCreationManager Instance { get; private set; }

    [Header("UIDocument")]
    public UIDocument UIDocument;

    private TextField m_NameField;
    private Label m_NameLabel; // Referencia al label "Nombre del Héroe"
    private IntegerField m_StrengthField, m_AgilityField, m_IntelligenceField, m_HealthField;
    private Label m_PointsLabel, m_PreviewLifeLabel, m_TitleLabel, m_XPLabel;
    private Button m_PlayButton, m_ExchangeXPButton, m_BackButton;
    private Button m_StrPlus, m_StrMinus, m_AgiPlus, m_AgiMinus, m_IntPlus, m_IntMinus, m_HpPlus, m_HpMinus;
    private VisualElement m_XPContainer;

    private const int MIN_STAT_VALUE = 1;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (UIDocument == null) return;
        var root = UIDocument.rootVisualElement;

        m_TitleLabel = root.Q<Label>(className: "title");
        m_NameLabel = root.Q<Label>("NameLabel"); // Obtener referencia al label
        m_NameField = root.Q<TextField>("NameInput");
        m_StrengthField = root.Q<IntegerField>("StrInput");
        m_AgilityField = root.Q<IntegerField>("AgiInput");
        m_IntelligenceField = root.Q<IntegerField>("IntInput");
        m_HealthField = root.Q<IntegerField>("HpInput");
        m_PointsLabel = root.Q<Label>("PointsLabel");
        m_PreviewLifeLabel = root.Q<Label>("PreviewLife");
        m_PlayButton = root.Q<Button>("PlayButton");

        m_StrPlus = root.Q<Button>("StrPlus"); m_StrMinus = root.Q<Button>("StrMinus");
        m_AgiPlus = root.Q<Button>("AgiPlus"); m_AgiMinus = root.Q<Button>("AgiMinus");
        m_IntPlus = root.Q<Button>("IntPlus"); m_IntMinus = root.Q<Button>("IntMinus");
        m_HpPlus = root.Q<Button>("HpPlus"); m_HpMinus = root.Q<Button>("HpMinus");

        m_StrengthField.isReadOnly = true; m_AgilityField.isReadOnly = true;
        m_IntelligenceField.isReadOnly = true; m_HealthField.isReadOnly = true;

        m_StrPlus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Strength, 1);
        m_StrMinus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Strength, -1);
        m_AgiPlus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Agility, 1);
        m_AgiMinus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Agility, -1);
        m_IntPlus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Intelligence, 1);
        m_IntMinus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Intelligence, -1);
        m_HpPlus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Health, 1);
        m_HpMinus.clicked += () => CharacterCreationManager.Instance.ModifyStat(StatType.Health, -1);
        m_PlayButton.clicked += () => CharacterCreationManager.Instance.OnPlayButtonClicked();

        if (m_NameField != null) m_NameField.RegisterValueChangedCallback(evt => CharacterCreationManager.Instance.SetPlayerName(evt.newValue));

        if (CharacterCreationManager.Instance != null && CharacterCreationManager.Instance.IsLevelUp())
        {
            SetupLevelUpUI();
        }
        else if (CharacterCreationManager.Instance != null && CharacterCreationManager.Instance.IsRetrying())
        {
            SetupRetryUI();
        }
        else
        {
            SetupNewGameUI();
        }

        // INICIALIZACIÓN EXPLÍCITA DE VALORES PARA EVITAR QUE APAREZCAN EN 0
        if (CharacterCreationManager.Instance != null)
        {
            m_StrengthField.value = CharacterCreationManager.Instance.GetStatValue(StatType.Strength);
            m_AgilityField.value = CharacterCreationManager.Instance.GetStatValue(StatType.Agility);
            m_IntelligenceField.value = CharacterCreationManager.Instance.GetStatValue(StatType.Intelligence);
            m_HealthField.value = CharacterCreationManager.Instance.GetStatValue(StatType.Health);

            int basePoints = CharacterCreationManager.Instance.IsLevelUp() ?
                (SessionManager.Instance.CurrentPlayerData.Strength + SessionManager.Instance.CurrentPlayerData.Agility +
                SessionManager.Instance.CurrentPlayerData.Intelligence + SessionManager.Instance.CurrentPlayerData.Health) :
                (1 * 4);
            int currentTotal = CharacterCreationManager.Instance.GetStatValue(StatType.Strength) +
                               CharacterCreationManager.Instance.GetStatValue(StatType.Agility) +
                               CharacterCreationManager.Instance.GetStatValue(StatType.Intelligence) +
                               CharacterCreationManager.Instance.GetStatValue(StatType.Health);
            int pointsLeft = CharacterCreationManager.Instance.GetAvailablePoints() - (currentTotal - basePoints);

            if (m_PointsLabel != null) m_PointsLabel.text = $"Puntos disponibles: {pointsLeft}";

            PlayerData tempData = new PlayerData("Temp",
                CharacterCreationManager.Instance.GetStatValue(StatType.Strength),
                CharacterCreationManager.Instance.GetStatValue(StatType.Agility),
                CharacterCreationManager.Instance.GetStatValue(StatType.Intelligence),
                CharacterCreationManager.Instance.GetStatValue(StatType.Health));

            if (m_PreviewLifeLabel != null) m_PreviewLifeLabel.text = "Vida Inicial: " + tempData.StartingLife;

            // Actualizar estado de botones
            if (CharacterCreationManager.Instance.IsLevelUp())
            {
                var data = SessionManager.Instance.CurrentPlayerData;
                int str = m_StrengthField.value;
                int agi = m_AgilityField.value;
                int intel = m_IntelligenceField.value;
                int hp = m_HealthField.value;

                if (m_StrMinus != null) m_StrMinus.SetEnabled(str > data.Strength);
                if (m_AgiMinus != null) m_AgiMinus.SetEnabled(agi > data.Agility);
                if (m_IntMinus != null) m_IntMinus.SetEnabled(intel > data.Intelligence);
                if (m_HpMinus != null) m_HpMinus.SetEnabled(hp > data.Health);
            }
            else
            {
                if (m_StrMinus != null) m_StrMinus.SetEnabled(m_StrengthField.value > 1);
                if (m_AgiMinus != null) m_AgiMinus.SetEnabled(m_AgilityField.value > 1);
                if (m_IntMinus != null) m_IntMinus.SetEnabled(m_IntelligenceField.value > 1);
                if (m_HpMinus != null) m_HpMinus.SetEnabled(m_HealthField.value > 1);
            }

            if (m_StrPlus != null) m_StrPlus.SetEnabled(pointsLeft > 0);
            if (m_AgiPlus != null) m_AgiPlus.SetEnabled(pointsLeft > 0);
            if (m_IntPlus != null) m_IntPlus.SetEnabled(pointsLeft > 0);
            if (m_HpPlus != null) m_HpPlus.SetEnabled(pointsLeft > 0);

            bool canPlay = (CharacterCreationManager.Instance.IsRetrying() || CharacterCreationManager.Instance.IsLevelUp()) ||
                           !string.IsNullOrWhiteSpace(m_NameField?.value);
            if (m_PlayButton != null) m_PlayButton.SetEnabled(canPlay);
        }
    }

    private void SetupLevelUpUI()
    {
        if (m_TitleLabel != null) m_TitleLabel.text = $"NOCHE {SessionManager.Instance.CurrentNight} - MEJORA";
        if (m_NameField != null) m_NameField.style.display = DisplayStyle.None;
        if (m_NameLabel != null) m_NameLabel.style.display = DisplayStyle.None; // Ocultar label
        m_PlayButton.text = "CONTINUAR";
        CreateXPUI();
    }

    private void SetupRetryUI()
    {
        if (m_TitleLabel != null) m_TitleLabel.text = "REASIGNAR PUNTOS";
        if (m_NameField != null) m_NameField.style.display = DisplayStyle.None;
        if (m_NameLabel != null) m_NameLabel.style.display = DisplayStyle.None; // Ocultar label
        m_PlayButton.text = "REINTENTAR";
    }

    private void SetupNewGameUI()
    {
        CreateButtonRow();
    }

    private void CreateButtonRow()
    {
        var playParent = m_PlayButton.parent;
        var buttonRow = new VisualElement();
        buttonRow.style.flexDirection = FlexDirection.Row;
        buttonRow.style.justifyContent = Justify.SpaceBetween;
        buttonRow.style.marginTop = 25;

        m_BackButton = new Button { text = "Volver" };
        m_BackButton.AddToClassList("pause-btn"); m_BackButton.AddToClassList("pause-btn-exit");
        m_BackButton.style.width = 120; m_BackButton.style.height = 45; m_BackButton.style.marginRight = 10;
        m_BackButton.clicked += () => CharacterCreationManager.Instance.OnBackButtonClicked();

        m_PlayButton.style.marginTop = 0; m_PlayButton.style.flexGrow = 1;

        playParent.Remove(m_PlayButton);
        buttonRow.Add(m_BackButton); buttonRow.Add(m_PlayButton);
        playParent.Add(buttonRow);
    }

    private void CreateXPUI()
    {
        var root = UIDocument.rootVisualElement;
        var card = root.Q<VisualElement>(className: "card");
        if (card == null) return;

        m_XPContainer = new VisualElement();
        m_XPContainer.style.marginTop = 15; m_XPContainer.style.marginBottom = 10;
        m_XPContainer.style.paddingTop = 10; m_XPContainer.style.paddingBottom = 10;
        m_XPContainer.style.paddingLeft = 15; m_XPContainer.style.paddingRight = 15;
        m_XPContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
        m_XPContainer.style.borderTopLeftRadius = 8; m_XPContainer.style.borderTopRightRadius = 8;
        m_XPContainer.style.borderBottomLeftRadius = 8; m_XPContainer.style.borderBottomRightRadius = 8;

        m_XPLabel = new Label();
        m_XPLabel.style.color = new StyleColor(new Color(0.5f, 1f, 0.5f));
        m_XPLabel.style.fontSize = 12; m_XPLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        m_XPLabel.style.unityTextAlign = TextAnchor.MiddleCenter; m_XPLabel.style.marginBottom = 10;
        m_XPContainer.Add(m_XPLabel);

        m_ExchangeXPButton = new Button();
        m_ExchangeXPButton.AddToClassList("pause-btn");
        m_ExchangeXPButton.style.width = Length.Percent(100); m_ExchangeXPButton.style.height = 38;
        m_ExchangeXPButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.2f));
        m_ExchangeXPButton.clicked += () => CharacterCreationManager.Instance.ExchangeXPForPoint();
        m_XPContainer.Add(m_ExchangeXPButton);

        var playBtnIndex = card.IndexOf(m_PlayButton);
        if (playBtnIndex >= 0) card.Insert(playBtnIndex, m_XPContainer);
        else card.Add(m_XPContainer);
    }

    // Métodos públicos para que CharacterCreationManager actualice la UI
    public void UpdateStatsUI(int strength, int agility, int intelligence, int health, int pointsLeft)
    {
        if (m_StrengthField != null) m_StrengthField.value = strength;
        if (m_AgilityField != null) m_AgilityField.value = agility;
        if (m_IntelligenceField != null) m_IntelligenceField.value = intelligence;
        if (m_HealthField != null) m_HealthField.value = health;
        if (m_PointsLabel != null) m_PointsLabel.text = $"Puntos disponibles: {pointsLeft}";
    }

    public void UpdatePreviewLife(int life)
    {
        if (m_PreviewLifeLabel != null) m_PreviewLifeLabel.text = "Vida Inicial: " + life;
    }

    public void UpdateExchangeButton(int currentXP, int xpPerPoint)
    {
        if (m_ExchangeXPButton != null)
        {
            m_ExchangeXPButton.text = $"Cambiar {xpPerPoint} XP por 1 Punto";
            m_ExchangeXPButton.SetEnabled(currentXP >= xpPerPoint);
        }
        if (m_XPLabel != null) m_XPLabel.text = $"XP: {currentXP}";
    }

    public void SetPlayButtonEnabled(bool enabled)
    {
        if (m_PlayButton != null) m_PlayButton.SetEnabled(enabled);
    }

    public void UpdateButtonStates(PlayerData data, int str, int agi, int intel, int hp, int pointsLeft)
    {
        if (data != null) // IsLevelUp
        {
            if (m_StrMinus != null) m_StrMinus.SetEnabled(str > data.Strength);
            if (m_AgiMinus != null) m_AgiMinus.SetEnabled(agi > data.Agility);
            if (m_IntMinus != null) m_IntMinus.SetEnabled(intel > data.Intelligence);
            if (m_HpMinus != null) m_HpMinus.SetEnabled(hp > data.Health);
        }
        else
        {
            if (m_StrMinus != null) m_StrMinus.SetEnabled(str > MIN_STAT_VALUE);
            if (m_AgiMinus != null) m_AgiMinus.SetEnabled(agi > MIN_STAT_VALUE);
            if (m_IntMinus != null) m_IntMinus.SetEnabled(intel > MIN_STAT_VALUE);
            if (m_HpMinus != null) m_HpMinus.SetEnabled(hp > MIN_STAT_VALUE);
        }

        if (m_StrPlus != null) m_StrPlus.SetEnabled(pointsLeft > 0);
        if (m_AgiPlus != null) m_AgiPlus.SetEnabled(pointsLeft > 0);
        if (m_IntPlus != null) m_IntPlus.SetEnabled(pointsLeft > 0);
        if (m_HpPlus != null) m_HpPlus.SetEnabled(pointsLeft > 0);
    }
}