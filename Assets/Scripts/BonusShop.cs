using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class BonusShop : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject shopPanel;
    public Button closeButton;
    
    [Header("Bonus Buttons")]
    public Button increaseClickPowerButton;
    public Button boostNodeScoreButton;
    public Button boostGoldGenButton;
    public Button attackEnemyButton;
    
    [Header("Info Texts")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI helpText;
    
    private int selectedNodeId = -1;
    private bool isSelectingNode = false;
    private int pendingBonusType = -1;

    void Awake()
    {
        NetworkClient.RegisterHandler<BonusResponseMessage>(OnBonusResponse, false);
        
        if (shopPanel != null) shopPanel.SetActive(false);
        
        // Setup button listeners
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        if (increaseClickPowerButton != null) increaseClickPowerButton.onClick.AddListener(() => BuyBonus(0, -1));
        if (boostNodeScoreButton != null) boostNodeScoreButton.onClick.AddListener(() => StartNodeSelection(1));
        if (boostGoldGenButton != null) boostGoldGenButton.onClick.AddListener(() => BuyBonus(2, -1));
        if (attackEnemyButton != null) attackEnemyButton.onClick.AddListener(() => StartNodeSelection(3));
        
        if (messageText != null) messageText.text = "";
        if (helpText != null) helpText.text = "Нажмите B чтобы открыть магазин";
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterHandler<BonusResponseMessage>();
    }

    void Update()
    {
        // Toggle shop with B key
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(!shopPanel.activeSelf);
                if (!shopPanel.activeSelf)
                {
                    isSelectingNode = false;
                    pendingBonusType = -1;
                    if (helpText != null) helpText.text = "Нажмите B чтобы открыть магазин";
                }
            }
        }
        
        // Handle node selection
        if (isSelectingNode && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 world = Camera.main.ScreenToWorldPoint(mousePos);
            Collider2D col = Physics2D.OverlapPoint(world);
            
            if (col != null && col.CompareTag("Node"))
            {
                var cc = col.GetComponent<CircleController>();
                if (cc != null)
                {
                    selectedNodeId = cc.nodeId;
                    BuyBonus(pendingBonusType, selectedNodeId);
                    isSelectingNode = false;
                    pendingBonusType = -1;
                    if (helpText != null) helpText.text = "";
                }
            }
        }
    }

    private void StartNodeSelection(int bonusType)
    {
        isSelectingNode = true;
        pendingBonusType = bonusType;
        
        if (bonusType == 1)
            if (helpText != null) helpText.text = "Выберите СВОЙ узел для усиления";
        else if (bonusType == 3)
            if (helpText != null) helpText.text = "Выберите ВРАЖЕСКИЙ узел для атаки";
    }

    private void BuyBonus(int bonusType, int targetNodeId)
    {
        if (!NetworkClient.isConnected)
        {
            ShowMessage("Не подключен к серверу");
            return;
        }
        
        BuyBonusMessage msg = new BuyBonusMessage 
        { 
            bonusType = bonusType, 
            targetNodeId = targetNodeId 
        };
        NetworkClient.Send(msg);
        
        if (messageText != null) messageText.text = "Отправка запроса...";
    }

    private void OnBonusResponse(BonusResponseMessage msg)
    {
        ShowMessage(msg.message);
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 3f);
        }
    }

    private void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }

    private void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        isSelectingNode = false;
        pendingBonusType = -1;
        if (helpText != null) helpText.text = "Нажмите B чтобы открыть магазин";
    }
}
