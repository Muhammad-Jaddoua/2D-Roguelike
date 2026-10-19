using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene references")]
    [SerializeField, FormerlySerializedAs("BoardManager")] private BoardManager m_BoardManager;
    [SerializeField, FormerlySerializedAs("PlayerController")] private PlayerController m_PlayerController;
    [SerializeField, FormerlySerializedAs("UIDoc")] private UIDocument m_UIDoc;

    [Header("Tuning")]
    [SerializeField, Range(5, 200), Tooltip("Food the player starts every new game with")]
    private int m_StartingFood = 20;

    public BoardManager BoardManager => m_BoardManager;
    public PlayerController PlayerController => m_PlayerController;
    public TurnManager TurnManager { get; private set; }

    private int m_FoodAmount;
    private int m_CurrentLevel = 1;
    private bool m_IsGameOver;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    private Label m_FoodLabel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        m_FoodLabel = m_UIDoc.rootVisualElement.Q<Label>("FoodLabel");

        m_GameOverPanel = m_UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");

        StartNewGame();
    }

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_IsGameOver = false;
        m_CurrentLevel = 1;
        m_FoodAmount = m_StartingFood;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        m_BoardManager.Clean();
        m_BoardManager.Init(m_CurrentLevel);

        m_PlayerController.Init();
        m_PlayerController.Spawn(m_BoardManager, new Vector2Int(1, 1));
    }

    public void NewLevel()
    {
        m_CurrentLevel++;
        SaveSystem.RecordLevelReached(m_CurrentLevel);

        m_BoardManager.Clean();
        m_BoardManager.Init(m_CurrentLevel);
        m_PlayerController.Spawn(m_BoardManager, new Vector2Int(1, 1));
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        // m_IsGameOver makes sure the game over sound/save only happen once
        if (m_FoodAmount <= 0 && !m_IsGameOver)
        {
            m_IsGameOver = true;
            m_PlayerController.GameOver();
            SoundManager.PlayGameOver();

            // Bonus: persist the best result between sessions
            SaveData data = SaveSystem.Load();
            data.GamesPlayed++;
            if (m_CurrentLevel > data.BestLevel)
            {
                data.BestLevel = m_CurrentLevel;
            }
            SaveSystem.Save(data);

            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " days\nBest: "
                                     + data.BestLevel + " days\n\n(Enter = restart)";
        }
    }

    [ContextMenu("Reset saved data")]
    private void ResetSavedData()
    {
        SaveSystem.Reset();
        Debug.Log("Saved data reset.");
    }
}
