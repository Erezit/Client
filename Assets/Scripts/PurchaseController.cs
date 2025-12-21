using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.InputSystem;
using TMPro;

public class PurchaseController : MonoBehaviour
{
    public Button buyDirectedEdgeButton;
    public TMP_Text infoText;
    public int cost = 100;

    private bool inPurchaseMode = false;
    private int? firstNode = null;
    private bool gameEnded = false;

    void Awake()
    {
        NetworkClient.RegisterHandler<PurchaseResultMessage>(OnPurchaseResultMessage, false);
        ClientScoreHandler.OnGameOver += OnGameOverMessage;
    }

    void Start()
    {
        if (buyDirectedEdgeButton != null) buyDirectedEdgeButton.onClick.AddListener(OnBuyClicked);
        UpdateInfo();
    }

    void OnDestroy()
    {
        if (buyDirectedEdgeButton != null) buyDirectedEdgeButton.onClick.RemoveListener(OnBuyClicked);
        NetworkClient.UnregisterHandler<PurchaseResultMessage>();
        ClientScoreHandler.OnGameOver -= OnGameOverMessage;
    }

    private void OnGameOverMessage(GameOverMessage msg)
    {
        gameEnded = true;
        inPurchaseMode = false;
        firstNode = null;
        if (buyDirectedEdgeButton != null) buyDirectedEdgeButton.interactable = false;
        SetInfo("Game Over!");
        Debug.Log("[PurchaseController] Game ended, purchases disabled.");
    }

    void OnBuyClicked()
    {
        if (gameEnded) return;
        inPurchaseMode = !inPurchaseMode;
        firstNode = null;
        UpdateInfo();
    }

    void Update()
    {
        if (!inPurchaseMode || gameEnded) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();
            Vector2 world = Camera.main.ScreenToWorldPoint(mouse);
            Collider2D col = Physics2D.OverlapPoint(world);
            if (col != null && col.CompareTag("Node"))
            {
                var cc = col.GetComponent<CircleController>();
                if (cc == null) return;
                if (!firstNode.HasValue)
                {
                    firstNode = cc.nodeId;
                    SetInfo($"Now pick TO node.");
                }
                else
                {
                    int from = firstNode.Value;
                    int to = cc.nodeId;
                    DirectedEdgePurchaseMessage msg = new DirectedEdgePurchaseMessage { fromNodeId = from, toNodeId = to };
                    if (NetworkClient.isConnected)
                    {
                        NetworkClient.Send(msg);
                        SetInfo($"Sent purchase request {from} -> {to} (cost {cost}). Waiting result...");
                    }
                    else
                    {
                        SetInfo("Not connected");
                    }
                    inPurchaseMode = false;
                    firstNode = null;
                    UpdateInfo();
                }
            }
        }
    }

    void UpdateInfo()
    {
        if (infoText == null) return;
        if (inPurchaseMode) infoText.text = "select FROM node";
        else infoText.text = "Buy directed edge";
    }

    void SetInfo(string s)
    {
        if (infoText != null) infoText.text = s;
        else Debug.Log(s);
    }

    void OnPurchaseResultMessage(PurchaseResultMessage msg)
    {
        if (msg.success)
        {
            SetInfo($"Purchase successful. Gold: {msg.newGold}");
        }
        else
        {
            SetInfo($"Purchase failed: {msg.reason}");
        }

        var hud = FindObjectOfType<PlayerHUD>();
        if (hud != null)
        {
            if (msg.success && msg.newGold >= 0)
            {
                PlayerStatsMessage p = new PlayerStatsMessage { ownerId = hudBackgroundOwnerId(hud), gold = msg.newGold, ownedNodes = -1 };
                if (hud.goldText != null) hud.goldText.text = $"Gold: {msg.newGold}";
            }
        }
    }

    byte hudBackgroundOwnerId(PlayerHUD hud)
    {
        return 0;
    }
}
