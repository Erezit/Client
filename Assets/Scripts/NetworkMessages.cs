using Mirror;
using UnityEngine;

public struct GraphMessage : NetworkMessage
{
    public int[] nodeIds;
    public Vector2[] positions;
    public int[] scores;
    public byte[] owners;        // 0 = neutral, 1 = player1 (red), 2 = player2 (blue)
    public int[] edgeFrom;
    public int[] edgeTo;
}

public struct ClickMessage : NetworkMessage
{
    public int nodeId;
}

public struct NodeUpdateMessage : NetworkMessage
{
    public int nodeId;
    public int score;
    public byte owner; // 0/1/2
}

public struct PlayerStatsMessage : NetworkMessage
{
    public byte ownerId;    // 1 or 2
    public int gold;
    public int ownedNodes;
}

public struct DirectedEdgePurchaseMessage : NetworkMessage
{
    public int fromNodeId;
    public int toNodeId;
}

public struct DirectedEdgeMessage : NetworkMessage
{
    public int fromNodeId;
    public int toNodeId;
    public byte owner; // who created it (1 or 2)
}

public struct PurchaseResultMessage : NetworkMessage
{
    public bool success;
    public string reason;
    public int newGold;
}

// --- turret purchase + turret placed ---
public struct TurretPurchaseMessage : NetworkMessage
{
    public int nodeId;
}

public struct TurretPlacedMessage : NetworkMessage
{
    public int nodeId;
    public byte owner;    // 1 or 2
    public float scale;   // visual scale multiplier
}
