using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ClientScoreHandler : MonoBehaviour
{
    public GameObject nodePrefab;
    public Material edgeMaterial;
    public Material directedEdgeMaterial;

    private Dictionary<int, CircleController> nodes = new Dictionary<int, CircleController>();
    private List<GameObject> edgeObjects = new List<GameObject>();
    private List<GameObject> directedEdgeObjects = new List<GameObject>();

    public static System.Action<GameOverMessage> OnGameOver;

    void Awake()
    {
        NetworkClient.RegisterHandler<GraphMessage>(OnGraphMessage, false);
        NetworkClient.RegisterHandler<NodeUpdateMessage>(OnNodeUpdateMessage, false);
        NetworkClient.RegisterHandler<DirectedEdgeMessage>(OnDirectedEdgeMessage, false);
        NetworkClient.RegisterHandler<GameOverMessage>(OnGameOverMessage, false);
        
        if (FindObjectOfType<GameOverUI>() == null)
        {
            GameObject uiObj = new GameObject("GameOverUIController");
            uiObj.AddComponent<GameOverUI>();
            Debug.Log("[ClientScoreHandler] Created GameOverUI automatically");
        }
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterHandler<GraphMessage>();
        NetworkClient.UnregisterHandler<NodeUpdateMessage>();
        NetworkClient.UnregisterHandler<DirectedEdgeMessage>();
        NetworkClient.UnregisterHandler<GameOverMessage>();
    }

    void OnGraphMessage(GraphMessage msg)
    {
        foreach (var kv in nodes) Destroy(kv.Value.gameObject);
        nodes.Clear();
        foreach (var e in edgeObjects) Destroy(e);
        edgeObjects.Clear();
        foreach (var d in directedEdgeObjects) Destroy(d);
        directedEdgeObjects.Clear();

        for (int i = 0; i < msg.nodeIds.Length; i++)
        {
            int id = msg.nodeIds[i];
            Vector2 pos = msg.positions[i];
            int score = msg.scores[i];
            byte owner = msg.owners[i];

            GameObject go = Instantiate(nodePrefab, pos, Quaternion.identity);
            go.name = $"Node_{id}";
            var cc = go.GetComponent<CircleController>();
            cc.nodeId = id;
            cc.SetColorAndScore(score, owner);
            nodes[id] = cc;
        }

        for (int i = 0; i < msg.edgeFrom.Length; i++)
        {
            int a = msg.edgeFrom[i];
            int b = msg.edgeTo[i];
            if (!nodes.ContainsKey(a) || !nodes.ContainsKey(b)) continue;

            GameObject edgeGO = new GameObject($"Edge_{a}_{b}");
            var lr = edgeGO.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.widthCurve = AnimationCurve.Constant(0,1,0.03f);
            if (edgeMaterial != null) lr.material = edgeMaterial;
            lr.SetPosition(0, nodes[a].transform.position);
            lr.SetPosition(1, nodes[b].transform.position);
            lr.sortingOrder = -1;
            edgeObjects.Add(edgeGO);
        }

        Debug.Log($"[Client] Graph received: nodes={msg.nodeIds.Length}, edges={msg.edgeFrom.Length}");
    }

    void OnNodeUpdateMessage(NodeUpdateMessage msg)
    {
        if (nodes.TryGetValue(msg.nodeId, out CircleController cc))
        {
            cc.SetColorAndScore(msg.score, msg.owner);
            Debug.Log($"[Client] Node update id={msg.nodeId} score={msg.score} owner={msg.owner}");
        }
        else
        {
            Debug.LogWarning($"[Client] NodeUpdate for unknown nodeId {msg.nodeId}");
        }
    }

    void OnDirectedEdgeMessage(DirectedEdgeMessage msg)
    {
        if (!nodes.ContainsKey(msg.fromNodeId) || !nodes.ContainsKey(msg.toNodeId)) return;

        Vector3 aPos = nodes[msg.fromNodeId].transform.position;
        Vector3 bPos = nodes[msg.toNodeId].transform.position;

        GameObject go = new GameObject($"Directed_{msg.fromNodeId}_{msg.toNodeId}");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.widthCurve = AnimationCurve.Constant(0,1,0.04f);
        if (directedEdgeMaterial != null) lr.material = directedEdgeMaterial;
        else lr.material = new Material(Shader.Find("Sprites/Default"));
        
        Color col = (msg.owner == 1) ? Color.red : (msg.owner == 2) ? Color.blue : Color.white;
        lr.startColor = col; lr.endColor = col;
        lr.SetPosition(0, aPos);
        lr.SetPosition(1, bPos);
        lr.sortingOrder = -1;

        
        float arrowLen = 0.3f * Mathf.Clamp(1f, 0.3f, 1f);
        Vector3 dir = (bPos - aPos).normalized;
        Vector3 right = Quaternion.Euler(0,0,20) * -dir;
        Vector3 left = Quaternion.Euler(0,0,-20) * -dir;

        GameObject ah = new GameObject("arrowhead");
        ah.transform.parent = go.transform;
        var lr2 = ah.AddComponent<LineRenderer>();
        lr2.positionCount = 4;
        lr2.useWorldSpace = true;
        lr2.widthCurve = AnimationCurve.Constant(0,1,0.04f);
        if (directedEdgeMaterial != null) lr2.material = directedEdgeMaterial;
        lr2.startColor = col; lr2.endColor = col;
        
        lr2.SetPosition(0, bPos);
        lr2.SetPosition(1, bPos + right * arrowLen);
        lr2.SetPosition(2, bPos);
        lr2.SetPosition(3, bPos + left * arrowLen);
        lr2.sortingOrder = -1;

        directedEdgeObjects.Add(go);
    }

    void OnGameOverMessage(GameOverMessage msg)
    {
        Debug.Log($"[ClientScoreHandler] Game Over received: isWinner={msg.isWinner}, winnerOwnerId={msg.winnerOwnerId}");
        
        
        OnGameOver?.Invoke(msg);
    }
}
