using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.InputSystem;
using TMPro;

public class PurchaseController : MonoBehaviour
{ 
    [Header("Buttons")]
    public Button buyDirectedEdgeButton;
    public Button buyTurretButton;

    [Header("Button Texts")]
    public TMP_Text directedEdgeButtonText;
    public TMP_Text turretButtonText;

    [Header("Costs")]
    public int directedEdgeCost = 100;
    public int turretCost = 500;

    [Header("Status Info")]
    public TMP_Text infoText;

    private bool inDirectedMode = false;
    private bool inTurretMode = false;
    private int? firstNode = null;

    private string turretButtonOriginalText;
    private string directedButtonOriginalText;

    void Awake()
    {
        NetworkClient.RegisterHandler<PurchaseResultMessage>(OnPurchaseResult, false);

        if (turretButtonText != null) turretButtonOriginalText = turretButtonText.text;
        if (directedEdgeButtonText != null) directedButtonOriginalText = directedEdgeButtonText.text;
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterHandler<PurchaseResultMessage>();
        OnDestroyUI();
    }

    void Start()
    {
        if (buyDirectedEdgeButton != null) buyDirectedEdgeButton.onClick.AddListener(OnBuyDirectedClicked);
        if (buyTurretButton != null) buyTurretButton.onClick.AddListener(OnBuyTurretClicked);
        UpdateInfo();
    }

    void OnDestroyUI()
    {
        if (buyDirectedEdgeButton != null) buyDirectedEdgeButton.onClick.RemoveListener(OnBuyDirectedClicked);
        if (buyTurretButton != null) buyTurretButton.onClick.RemoveListener(OnBuyTurretClicked);
    }

    void OnBuyDirectedClicked()
    {
        inDirectedMode = !inDirectedMode;
        inTurretMode = false;
        firstNode = null;

        // Меняем текст кнопки
        if (inDirectedMode && directedEdgeButtonText != null)
            directedEdgeButtonText.text = "Выберите вершину FROM";
        else if (directedEdgeButtonText != null)
            directedEdgeButtonText.text = directedButtonOriginalText;

        // Сбрасываем текст турели
        if (turretButtonText != null) turretButtonText.text = turretButtonOriginalText;

        UpdateInfo();
    }

    void OnBuyTurretClicked()
    {
        inTurretMode = !inTurretMode;
        inDirectedMode = false;
        firstNode = null;

        // Меняем текст кнопки
        if (inTurretMode && turretButtonText != null)
            turretButtonText.text = "Выберите вершину";
        else if (turretButtonText != null)
            turretButtonText.text = turretButtonOriginalText;

        // Сбрасываем текст направленного ребра
        if (directedEdgeButtonText != null) directedEdgeButtonText.text = directedButtonOriginalText;

        UpdateInfo();
    }

    void Update()
    {
        if ((!inDirectedMode && !inTurretMode) || Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();
            Vector2 world = Camera.main.ScreenToWorldPoint(mouse);
            Collider2D col = Physics2D.OverlapPoint(world);
            if (col != null && col.CompareTag("Node"))
            {
                var cc = col.GetComponent<CircleController>();
                if (cc == null) return;

                if (inDirectedMode)
                {
                    if (!firstNode.HasValue)
                    {
                        firstNode = cc.nodeId;
                        SetInfo($"Selected FROM: {firstNode}. Now pick TO node.");
                    }
                    else
                    {
                        int from = firstNode.Value;
                        int to = cc.nodeId;
                        DirectedEdgePurchaseMessage msg = new DirectedEdgePurchaseMessage { fromNodeId = from, toNodeId = to };
                        if (NetworkClient.isConnected) NetworkClient.Send(msg);
                        SetInfo($"Sent directed-edge purchase {from}->{to} (cost {directedEdgeCost}).");
                        inDirectedMode = false;
                        firstNode = null;

                        // Сброс текста кнопки
                        if (directedEdgeButtonText != null) directedEdgeButtonText.text = directedButtonOriginalText;
                        UpdateInfo();
                    }
                }
                else if (inTurretMode)
                {
                    int node = cc.nodeId;
                    TurretPurchaseMessage msg = new TurretPurchaseMessage { nodeId = node };
                    if (NetworkClient.isConnected) NetworkClient.Send(msg);
                    SetInfo($"Sent turret purchase for node {node} (cost {turretCost}).");
                    inTurretMode = false;
                    firstNode = null;

                    // Меняем текст кнопки на куплено
                    if (turretButtonText != null) turretButtonText.text = "Куплено";
                    UpdateInfo();
                }
            }
        }
    }

    void UpdateInfo()
    {
        if (infoText == null) return;
        if (inDirectedMode) infoText.text = "Directed-edge: select FROM node";
        else if (inTurretMode) infoText.text = "Turret: select node to build";
        else infoText.text = "Buy: choose option";
    }

    void SetInfo(string s)
    {
        if (infoText != null) infoText.text = s;
        else Debug.Log(s);
    }

    void OnPurchaseResult(PurchaseResultMessage msg)
    {
        if (msg.success)
            SetInfo($"Purchase succeeded. Gold left: {msg.newGold}");
        else
            SetInfo($"Purchase failed: {msg.reason}");
    }
}
