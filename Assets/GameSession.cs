using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class GameSession : MonoBehaviour
{
    public enum RoundState { Title, Playing, Paused, Won, Lost }
    private const string HighScoreKey = "SpaceInvaders.HighScore";
    public static GameSession Instance { get; private set; }
    public RoundState State { get; private set; } = RoundState.Title;
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int Level { get; private set; } = 1;
    public int Lives { get; private set; } = GameSettings.StartingLives;
    private int levelStartingScore;
    private bool HasNextLevel => Level < int.MaxValue;
    public bool IsPlaying => State == RoundState.Playing && Time.frameCount > gameplayBeginsAfterFrame;
    private int gameplayBeginsAfterFrame = -1;
    public float DefeatLineY { get; private set; }
    private Spaceship player;
    private TextMeshProUGUI shieldText;
    private TextMeshProUGUI livesText;

    private InputAction pauseAction;
    private InputAction startRestartAction;
    private static bool startAfterReload;
    private static int pendingLevel = 1;
    private static int pendingScore;
    private static int pendingLives = GameSettings.StartingLives;
    private bool reloading;
    private Canvas canvas;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI menuTitle;
    private TextMeshProUGUI menuScore;
    private GameObject menu;
    private Button resumeButton;
    private Button restartButton;
    private Button startButton;
    private Button nextLevelButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRestartRequest()
    {
        startAfterReload = false;
        pendingLevel = 1;
        pendingScore = 0;
        pendingLives = GameSettings.StartingLives;
    }

    void Awake()
    {
        Instance = this;
        Level = pendingLevel;
        Score = pendingScore;
        levelStartingScore = Score;
        Lives = pendingLives;
        Time.timeScale = 0f;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        startRestartAction = new InputAction("Start or restart", InputActionType.Button);
        startRestartAction.AddBinding("<Keyboard>/space");
        startRestartAction.AddBinding("<Gamepad>/buttonWest");
    }

    void OnEnable()
    {
        pauseAction.Enable();
        startRestartAction.Enable();
    }

    void OnDisable()
    {
        pauseAction.Disable();
        startRestartAction.Disable();
    }

    void Start()
    {
        player = FindAnyObjectByType<Spaceship>();
        SetDefeatLineForPlayer(player != null ? player.transform.position.y : -6.83f);
        BuildInterface();
        RefreshScore();
        gameObject.AddComponent<SpaceBackdrop>();
        if (startAfterReload)
        {
            startAfterReload = false;
            BeginRound();
        }
        else ShowMenu();
    }

    public void BeginRound()
    {
        if (State != RoundState.Title) return;
        State = RoundState.Playing;
        // The input used to press Start should not also fire a laser.
        gameplayBeginsAfterFrame = Time.frameCount;
        Time.timeScale = 1f;
        menu.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void SetDefeatLineForPlayer(float playerY)
    {
        DefeatLineY = playerY + GameSettings.DefeatLineHeightAboveShip;
    }

    void Update()
    {
        if (startRestartAction.WasPressedThisFrame() && !reloading)
        {
            if (State == RoundState.Title)
                BeginRound();
            else if (State == RoundState.Won && HasNextLevel && menu.activeSelf)
                NextLevel();
            else if ((State == RoundState.Paused || State == RoundState.Won || State == RoundState.Lost)
                && menu.activeSelf)
                Restart();
        }
        if (shieldText != null)
        {
            int charges = player != null ? player.ShieldCharges : 0;
            shieldText.text = $"SHIELD  {charges}/{GameSettings.ShieldCapacity}";
            shieldText.color = charges > 0 ? new Color(0.3f, 1f, 0.9f) : new Color(1f, 0.45f, 0.45f);
        }
        if (pauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void AlienDefeated(bool fleetCleared)
    {
        if (!IsPlaying) return;
        Score += GameSettings.PointsPerAlien;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
        RefreshScore();
        if (fleetCleared) FinishRound(true);
    }

    public void FinishRound(bool won)
    {
        if (!IsPlaying) return;
        State = won ? RoundState.Won : RoundState.Lost;
        if (!won) Lives = Mathf.Max(0, Lives - 1);
        RefreshScore();
        Time.timeScale = 0f;
        StartCoroutine(ShowResultsAfterEffect());
    }

    private System.Collections.IEnumerator ShowResultsAfterEffect()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        ShowMenu();
    }

    public void TogglePause()
    {
        if (State == RoundState.Playing)
        {
            State = RoundState.Paused;
            Time.timeScale = 0f;
            ShowMenu();
        }
        else if (State == RoundState.Paused)
        {
            State = RoundState.Playing;
            gameplayBeginsAfterFrame = Time.frameCount;
            Time.timeScale = 1f;
            menu.SetActive(false);
            EventSystem.current?.SetSelectedGameObject(null);
        }
    }

    public void Restart()
    {
        if (State == RoundState.Lost && Lives > 0)
            ReloadRound(Level, levelStartingScore, Lives);
        else
            ReloadRound(1, 0, GameSettings.StartingLives);
    }

    public void NextLevel()
    {
        if (State != RoundState.Won || !HasNextLevel) return;
        ReloadRound(Level + 1, Score, Lives);
    }

    private void ReloadRound(int level, int score, int lives)
    {
        if (reloading) return;
        reloading = true;
        pendingLevel = level;
        pendingScore = score;
        pendingLives = lives;
        startAfterReload = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RefreshScore()
    {
        if (scoreText != null)
            scoreText.text = $"Level {Level}   Score: {Score:N0}   Best: {HighScore:N0}";
        if (livesText != null)
            livesText.text = $"LIVES  {Lives}";
    }

    private void ShowMenu()
    {
        bool isTitle = State == RoundState.Title;
        bool canAdvance = State == RoundState.Won && HasNextLevel;
        menuTitle.text = isTitle ? "SPACE INVADERS" : State == RoundState.Paused ? "PAUSED"
            : State == RoundState.Won ? (canAdvance ? $"LEVEL {Level} CLEARED!" : "ALL LEVELS CLEARED!")
            : Lives > 0 ? "LIFE LOST" : "GAME OVER";
        menuScore.text = isTitle ? $"Defend the line!     High score: {HighScore:N0}"
            : $"Score: {Score:N0}   Best: {HighScore:N0}   Lives: {Lives}";
        restartButton.GetComponentInChildren<TextMeshProUGUI>().text =
            State == RoundState.Lost && Lives > 0 ? "Retry Level" : "Restart Run";
        startButton.gameObject.SetActive(isTitle);
        resumeButton.gameObject.SetActive(State == RoundState.Paused);
        restartButton.gameObject.SetActive(!isTitle);
        nextLevelButton.gameObject.SetActive(canAdvance);
        menu.SetActive(true);
        Canvas.ForceUpdateCanvases();
        EventSystem.current?.SetSelectedGameObject(
            isTitle ? startButton.gameObject : canAdvance ? nextLevelButton.gameObject
                : State == RoundState.Paused ? resumeButton.gameObject : restartButton.gameObject);
    }

    private void BuildInterface()
    {
        GameObject root = new GameObject("Round UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        AlienCounter alienCounter = FindAnyObjectByType<AlienCounter>();
        if (alienCounter != null)
        {
            TextMeshProUGUI counterText = alienCounter.GetComponent<TextMeshProUGUI>();
            RectTransform counterRect = counterText.rectTransform;
            counterRect.SetParent(root.transform, false);
            counterRect.anchorMin = counterRect.anchorMax = new Vector2(0, 1);
            counterRect.pivot = new Vector2(0.5f, 0.5f);
            counterRect.anchoredPosition = new Vector2(170, -28);
            counterRect.sizeDelta = new Vector2(320, 42);
            counterText.fontSize = 24;
            counterText.alignment = TextAlignmentOptions.Center;
            counterText.raycastTarget = false;
        }

        shieldText = MakeText(root.transform, "Shield charges", "", new Vector2(270, 42), new Vector2(150, -76), 24);
        shieldText.rectTransform.anchorMin = shieldText.rectTransform.anchorMax = new Vector2(0, 1);
        livesText = MakeText(root.transform, "Lives", "", new Vector2(270, 42), new Vector2(150, -118), 24);
        livesText.rectTransform.anchorMin = livesText.rectTransform.anchorMax = new Vector2(0, 1);

        scoreText = MakeText(root.transform, "Score", "", new Vector2(480, 42), new Vector2(-260, -28), 24);
        RectTransform scoreRect = scoreText.rectTransform;
        scoreRect.anchorMin = scoreRect.anchorMax = new Vector2(1, 1);
        Button pauseButton = MakeButton(root.transform, "Pause / Controls", new Vector2(-125, -80), TogglePause);
        RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
        pauseRect.anchorMin = pauseRect.anchorMax = new Vector2(1, 1);
        pauseRect.sizeDelta = new Vector2(220, 44);

        menu = new GameObject("Round menu", typeof(RectTransform), typeof(Image));
        menu.transform.SetParent(root.transform, false);
        RectTransform overlay = menu.GetComponent<RectTransform>();
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.sizeDelta = Vector2.zero;
        menu.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.06f, 0.96f);
        menuTitle = MakeText(menu.transform, "Title", "", new Vector2(640, 70), new Vector2(0, 220), 42);
        menuScore = MakeText(menu.transform, "Results", "", new Vector2(640, 45), new Vector2(0, 155), 26);
        MakeText(menu.transform, "Controls",
            "CONTROLS\n\nMove: Arrow keys / A-D / Left stick\nShoot (tap or hold): Space / Enter / Left click\nGamepad shoot (tap or hold): X / Square\nTouch: Drag to move, tap to shoot\nPause / Resume: Escape / Gamepad Start\nStart / Next / Retry: Space / Gamepad X or Square\n\n3 lives per run. Catch a blue + to refill your shield.\nKeep aliens above the red defeat line.",
            new Vector2(680, 310), new Vector2(0, -20), 21);
        resumeButton = MakeButton(menu.transform, "Resume", new Vector2(-150, -220), TogglePause);
        restartButton = MakeButton(menu.transform, "Restart", new Vector2(150, -220), Restart);
        startButton = MakeButton(menu.transform, "Start Game", new Vector2(0, -220), BeginRound);
        nextLevelButton = MakeButton(menu.transform, "Next Level", new Vector2(-150, -220), NextLevel);
        TextMeshProUGUI credits = MakeText(menu.transform, "Art credits",
            "<link=\"https://game-icons.net/1x1/lorc/alien-stare.html\"><color=#80D9FF><u>Alien Stare</u></color></link> by Lorc / Game-icons.net | <link=\"https://creativecommons.org/licenses/by/3.0/\"><color=#80D9FF><u>CC BY 3.0</u></color></link> (tinted green)\nShip and lasers: Kenney.nl (CC0) | Click the underlined credits to open their pages.\nDeveloped with AI-assisted programming support from OpenAI Codex.",
            new Vector2(760, 76), new Vector2(0, -300), 14);
        credits.raycastTarget = true;
        credits.gameObject.AddComponent<CreditLinks>();
        menu.SetActive(false);
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, string content, Vector2 size, Vector2 position, float fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static Button MakeButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(240, 56);
        rect.anchoredPosition = position;
        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.12f, 0.3f, 0.5f);
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.65f, 0.9f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(action);
        MakeText(obj.transform, "Label", label, new Vector2(220, 50), Vector2.zero, 24);
        return button;
    }

    void OnDestroy()
    {
        pauseAction.Dispose();
        startRestartAction.Dispose();
        if (canvas != null) Destroy(canvas.gameObject);
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }
}
