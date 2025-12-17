// Assets/Scripts/PurchaseController.cs
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
    public TMP_Text infoText; // optional - show status
    public int cost = 100;

    private bool inPurchaseMode = false;
    private int? firstNode = null;

    void Awake()
    {
        // регистрируем handler для ответа сервера о покупке
        NetworkClient.RegisterHandler<PurchaseResultMessage>(OnPurchaseResultMessage, false);
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
    }

    void OnBuyClicked()
    {
        // go into selection mode
        inPurchaseMode = !inPurchaseMode;
        firstNode = null;
        UpdateInfo();
    }

    void Update()
    {
        if (!inPurchaseMode) return;
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
                    SetInfo($"Selected FROM: {firstNode}. Now pick TO node.");
                }
                else
                {
                    int from = firstNode.Value;
                    int to = cc.nodeId;
                    // send purchase request message
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
                    // exit purchase mode after attempt
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
        if (inPurchaseMode) infoText.text = "Purchase mode: select FROM node";
        else infoText.text = "Buy directed edge: click button";
    }

    void SetInfo(string s)
    {
        if (infoText != null) infoText.text = s;
        else Debug.Log(s);
    }

    // Новый: обработчик ответа сервера о результате покупки
    void OnPurchaseResultMessage(PurchaseResultMessage msg)
    {
        // Этот метод вызывается в главном потоке (Mirror).
        if (msg.success)
        {
            SetInfo($"Purchase successful. Gold: {msg.newGold}");
        }
        else
        {
            SetInfo($"Purchase failed: {msg.reason}");
        }

        // Сервер также шлёт PlayerStatsMessage (в твоём коде UpdateOwnedCountsAndNotify / SendPlayerStatsToConn),
        // поэтому HUD обновится. Но если хочешь обновить локально haste — можно искать PlayerHUD и обновлять:
        var hud = FindObjectOfType<PlayerHUD>();
        if (hud != null)
        {
            // Сервер вскоре пришлёт PlayerStatsMessage; если не пришлёт, можно временно показать newGold:
            if (msg.success && msg.newGold >= 0)
            {
                // создаём временный PlayerStatsMessage и передаём в HUD для прямого обновления
                PlayerStatsMessage p = new PlayerStatsMessage { ownerId = hudBackgroundOwnerId(hud), gold = msg.newGold, ownedNodes = -1 };
                // Но PlayerHUD ожидает PlayerStatsMessage; чтобы не ломать логику, просто обновим goldText directly if available
                if (hud.goldText != null) hud.goldText.text = $"Gold: {msg.newGold}";
            }
        }
    }

    // хак: получение ownerId, если нужен — иначе возвращаем 0
    byte hudBackgroundOwnerId(PlayerHUD hud)
    {
        // PlayerHUD хранит свой ownerId приватно; для простоты возвращаем 0
        return 0;
    }
}
