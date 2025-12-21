using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Text goldText;
    public Text nodesText;
    public Image backgroundPanel;
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
        if (myOwnerId == 0)
        {
            myOwnerId = msg.ownerId;
            if (backgroundPanel != null)
            {
                if (myOwnerId == 1) backgroundPanel.color = Color.red;
                else if (myOwnerId == 2) backgroundPanel.color = Color.blue;
            }
        }

        if (goldText != null) goldText.text = $"Gold: {msg.gold}";
        if (nodesText != null) nodesText.text = $"Nodes: {msg.ownedNodes}";
    }
}
