using System.Net;
using System.Net.Sockets;
using System.Reflection;
using LiteNetLib;
using UnityEngine;

public class OnDeviceHotReloadChangesReceiver: MonoBehaviour, INetEventListener
{
    //TODO: add regions to hide not relevant interface code
    private NetManager _netClient;
    
    void Start()
    {
#if UNITY_EDITOR
        //receiver should only run in build
        enabled = false;
        return;
#endif
        
        _netClient = new NetManager(this);
        _netClient.UnconnectedMessagesEnabled = true;
        _netClient.UpdateTime = 15;
        _netClient.Start();
    }

    private void Update()
    {
        _netClient.PollEvents();
        
        var peer = _netClient.FirstPeer;
        if (peer == null)
        {
            _netClient.SendBroadcast(new byte[] {1}, OnDeviceHotReloadChangesSender.PortToUse);
        }
    }
    
    void OnDestroy()
    {
        if (_netClient != null)
            _netClient.Stop();
    }
    
    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log($"connected to editor on " + peer.EndPoint);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log($"received error " + socketErrorCode);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        Debug.Log($"received data, trying to get DLL contents");
        var assemblyBytes = reader.GetBytesWithLength();

        var loadedAssembly = Assembly.Load(assemblyBytes);
        HotReloadDynamicAssemblyDetourManager.DynamicallyUpdateMethodsForCreatedAssembly(loadedAssembly);
    }
    
    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }
    
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.BasicMessage && _netClient.ConnectedPeersCount == 0 && reader.GetInt() == 1)
        {
            Debug.Log($"Received discovery response. Connecting to: " + remoteEndPoint);
            _netClient.Connect(remoteEndPoint, string.Empty);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
    }
    
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
    }
}