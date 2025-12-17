using System.Collections;
using UnityEngine;
using Mirror;

public class ClientBootstrap : MonoBehaviour
{
    public string serverAddress = "127.0.0.1";
    public ushort serverPort = 7777;
    public bool autoConnectInEditor = true;
    public float delay = 0.2f;

    IEnumerator Start()
    {
#if UNITY_EDITOR
        if (!autoConnectInEditor) yield break;
#endif
        yield return new WaitForSeconds(delay);
        var nm = GetComponent<NetworkManager>() ?? FindObjectOfType<NetworkManager>();
        if (nm == null) { Debug.LogError("[ClientBootstrap] No NetworkManager found"); yield break; }
        nm.networkAddress = serverAddress;
        var tele = nm.GetComponent<TelepathyTransport>();
        if (tele != null) tele.port = serverPort;
        nm.StartClient();
        Debug.Log($"[ClientBootstrap] StartClient -> {serverAddress}:{serverPort}");
    }
}
