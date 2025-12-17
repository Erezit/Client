using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Text goldText;
    public Text nodesText;
    public Text clickPowerText;
    public Image backgroundPanel; // optional - tint according to player color
    private byte myOwnerId = 0;

    void Awake()
    {
        NetworkClient.RegisterHandler<PlayerStatsMessage>(OnPlayerStatsMessage, false);
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterHandler<PlayerStatsMessage>();
    }

    void OnPlayerStatsMessage(PlayerStatsMessage msg)
    {
        // first message will tell who you are (ownerId)
        if (myOwnerId == 0)
        {
            myOwnerId = msg.ownerId;
            // tint background
            if (backgroundPanel != null)
            {
                if (myOwnerId == 1) backgroundPanel.color = new Color(1f, 0.5f, 0.5f, 0.5f);
                else if (myOwnerId == 2) backgroundPanel.color = new Color(0.5f, 0.5f, 1f, 0.5f);
            }
        }

        if (goldText != null) goldText.text = $"Gold: {msg.gold}";
        if (nodesText != null) nodesText.text = $"Nodes: {msg.ownedNodes}";
        if (clickPowerText != null) clickPowerText.text = $"Click Power: {msg.clickPower}";
    }
}
