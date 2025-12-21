using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    [Header("Settings")]
    public float disconnectDelay = 3f;
    
    private GameObject gameOverPanel;
    private Text gameOverText;
    private Text winnerText;
    private Canvas canvas;
    private bool gameEnded = false;

    void Start()
    {
        CreateUI();
        ClientScoreHandler.OnGameOver += OnGameOverMessage;
    }

    void CreateUI()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameOverCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);

        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(gameOverPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = new Vector2(0, 50);
        
        gameOverText = textObj.AddComponent<Text>();
        gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameOverText.fontSize = 80;
        gameOverText.alignment = TextAnchor.MiddleCenter;
        gameOverText.color = Color.white;
        gameOverText.text = "GAME OVER";
        
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        GameObject winnerObj = new GameObject("WinnerText");
        winnerObj.transform.SetParent(gameOverPanel.transform, false);
        
        RectTransform winnerRect = winnerObj.AddComponent<RectTransform>();
        winnerRect.anchorMin = new Vector2(0.5f, 0.5f);
        winnerRect.anchorMax = new Vector2(0.5f, 0.5f);
        winnerRect.sizeDelta = new Vector2(600, 100);
        winnerRect.anchoredPosition = new Vector2(0, -50);
        
        winnerText = winnerObj.AddComponent<Text>();
        winnerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winnerText.fontSize = 40;
        winnerText.alignment = TextAnchor.MiddleCenter;
        winnerText.color = Color.yellow;
        winnerText.text = "";
        
        Outline winnerOutline = winnerObj.AddComponent<Outline>();
        winnerOutline.effectColor = Color.black;
        winnerOutline.effectDistance = new Vector2(2, -2);

        gameOverPanel.SetActive(false);
        
        Debug.Log("[GameOverUI] UI created programmatically");
    }

    void OnDestroy()
    {
        ClientScoreHandler.OnGameOver -= OnGameOverMessage;
    }

    private void OnGameOverMessage(GameOverMessage msg)
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log($"[GameOverUI] Game Over! IsWinner: {msg.isWinner}, WinnerOwnerId: {msg.winnerOwnerId}");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
        {
            if (msg.isWinner)
            {
                gameOverText.text = "YOU WIN!";
                gameOverText.color = Color.green;
            }
            else
            {
                gameOverText.text = "YOU LOSE!";
                gameOverText.color = Color.red;
            }
        }

        if (winnerText != null)
        {
            string winnerColor = msg.winnerOwnerId == 1 ? "RED" : "BLUE";
            winnerText.text = $"Winner: {winnerColor} Player";
        }

        StartCoroutine(DisconnectAfterDelay());
    }

    private IEnumerator DisconnectAfterDelay()
    {
        yield return new WaitForSeconds(disconnectDelay);

        Debug.Log("[GameOverUI] Disconnecting from server...");

        if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        Debug.Log("[GameOverUI] Client disconnected.");
    }
}
