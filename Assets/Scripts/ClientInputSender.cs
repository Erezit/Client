using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class ClientInputSender : MonoBehaviour
{
    void Update()
    {
        if (!NetworkClient.isConnected) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 world = Camera.main.ScreenToWorldPoint(mousePos);
            Collider2D col = Physics2D.OverlapPoint(world);
            if (col != null && col.CompareTag("Node"))
            {
                var cc = col.GetComponent<CircleController>();
                if (cc != null)
                {
                    ClickMessage msg = new ClickMessage { nodeId = cc.nodeId };
                    NetworkClient.Send(msg);
                    Debug.Log($"[Client] Sent ClickMessage nodeId={cc.nodeId}");
                }
            }
        }
    }
}
