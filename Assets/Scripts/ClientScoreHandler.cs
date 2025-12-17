using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ClientScoreHandler : MonoBehaviour
{
    public GameObject nodePrefab; // drag Node prefab (with CircleController & Collider2D) in Inspector
    public Material edgeMaterial; // material for undirected edges
    public Material directedEdgeMaterial; // material for directed edges

    private Dictionary<int, CircleController> nodes = new Dictionary<int, CircleController>();
    private List<GameObject> edgeObjects = new List<GameObject>();
    private List<GameObject> directedEdgeObjects = new List<GameObject>();

    void Awake()
    {
        NetworkClient.RegisterHandler<GraphMessage>(OnGraphMessage, false);
        NetworkClient.RegisterHandler<NodeUpdateMessage>(OnNodeUpdateMessage, false);
        NetworkClient.RegisterHandler<DirectedEdgeMessage>(OnDirectedEdgeMessage, false);
        NetworkClient.RegisterHandler<TurretPlacedMessage>(OnTurretPlacedMessage, false);
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterHandler<GraphMessage>();
        NetworkClient.UnregisterHandler<NodeUpdateMessage>();
        NetworkClient.UnregisterHandler<DirectedEdgeMessage>();
        NetworkClient.UnregisterHandler<TurretPlacedMessage>();
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
        directedEdgeObjects.Add(go);
    }

    void OnTurretPlacedMessage(TurretPlacedMessage msg)
    {
        if (!nodes.TryGetValue(msg.nodeId, out CircleController cc))
        {
            Debug.LogWarning($"[Client] TurretPlaced for unknown nodeId {msg.nodeId}");
            return;
        }

        cc.transform.localScale = Vector3.one * msg.scale;

        var turretGO = new GameObject("TurretVis");
        turretGO.transform.parent = cc.transform;
        turretGO.transform.localPosition = Vector3.zero;
        var sr = turretGO.AddComponent<SpriteRenderer>();
        sr.sprite = cc.GetComponent<SpriteRenderer>().sprite;
        sr.sortingOrder = cc.GetComponent<SpriteRenderer>().sortingOrder + 1;
        sr.color = (msg.owner == 1) ? new Color(1f, 0.5f, 0.5f) : new Color(0.5f, 0.5f, 1f);
        turretGO.transform.localScale = Vector3.one * 0.4f;

        Debug.Log($"[Client] Turret visual placed at node {msg.nodeId} owner {msg.owner}");
    }
}
