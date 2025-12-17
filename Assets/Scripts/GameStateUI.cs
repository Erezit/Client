using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class GameStateUI : MonoBehaviour
{
    [Header("Game State UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI resultText;
    public Button quitButton;
    
    private byte myOwnerId = 0;
    private byte currentGameState = 0; // 0=Playing, 1=Player1Won, 2=Player2Won

    void Awake()
    {
        NetworkClient.RegisterHandler<GameStateMessage>(OnGameStateMessage, false);
        NetworkClient.RegisterHandler<PlayerStatsMessage>(OnPlayerStatsMessage, false);
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterHandler<GameStateMessage>();
        NetworkClient.UnregisterHandler<PlayerStatsMessage>();
    }

    void OnPlayerStatsMessage(PlayerStatsMessage msg)
    {
        // Remember who we are
        if (myOwnerId == 0)
        {
            myOwnerId = msg.ownerId;
        }
    }

    void OnGameStateMessage(GameStateMessage msg)
    {
        currentGameState = msg.gameState;
        
        if (msg.gameState != 0) // Game is over
        {
            ShowGameOver(msg.winnerOwnerId);
        }
    }

    private void ShowGameOver(byte winnerOwnerId)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        if (winnerOwnerId == myOwnerId)
        {
            // We won!
            if (gameOverText != null)
            {
                gameOverText.text = "ПОБЕДА!";
                gameOverText.color = Color.green;
            }
            if (resultText != null)
            {
                resultText.text = "Вы захватили все узлы противника!\nПоздравляем с победой!";
            }
        }
        else
        {
            // We lost
            if (gameOverText != null)
            {
                gameOverText.text = "ПОРАЖЕНИЕ";
                gameOverText.color = Color.red;
            }
            if (resultText != null)
            {
                resultText.text = "Вы потеряли все свои узлы.\nПопробуйте еще раз!";
            }
        }
    }

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
