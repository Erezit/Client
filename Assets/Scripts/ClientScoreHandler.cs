using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ClientScoreHandler : MonoBehaviour
{
    public GameObject nodePrefab; // drag Node prefab (with CircleController & Collider2D) in Inspector
    public Material edgeMaterial; // optional material for LineRenderer

    private Dictionary<int, CircleController> nodes = new Dictionary<int, CircleController>();
    private List<GameObject> edgeObjects = new List<GameObject>(); // LineRenderer objects

    void Awake()
    {
        NetworkClient.RegisterHandler<GraphMessage>(OnGraphMessage, false);
        NetworkClient.RegisterHandler<NodeUpdateMessage>(OnNodeUpdateMessage, false);
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterHandler<GraphMessage>();
        NetworkClient.UnregisterHandler<NodeUpdateMessage>();
    }

    void OnGraphMessage(GraphMessage msg)
    {
        // Clear old
        foreach (var kv in nodes) Destroy(kv.Value.gameObject);
        nodes.Clear();
        foreach (var e in edgeObjects) Destroy(e);
        edgeObjects.Clear();

        // spawn nodes
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

        // spawn edges (LineRenderer)
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
}
