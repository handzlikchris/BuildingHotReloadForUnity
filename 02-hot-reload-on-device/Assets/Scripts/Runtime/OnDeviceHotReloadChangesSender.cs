using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class OnDeviceHotReloadChangesSender: MonoBehaviour, INetEventListener, INetLogger
{
    public static int PortToUse = 15597; //WARN: if you see errors port-related, change
    
    private NetManager _netServer;
    private NetPeer _connectedPeer;
    private NetDataWriter _dataWriter;
    private string _requireConnectionKey;
    
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        NetDebug.Logger = this;
        _dataWriter = new NetDataWriter();
        _netServer = new NetManager(this);

        _netServer.Start(PortToUse);
        _netServer.BroadcastReceiveEnabled = true;
        _netServer.UpdateTime = 15;
    }
    
    void Update()
    {
        _netServer.PollEvents();
    }
    
    public void SendChangesToConnectedDevice(Assembly dynamicallyLoadedAssemblyWithUpdates)
    {
        if (_connectedPeer != null)
        {
            _dataWriter.Reset();
            _dataWriter.PutBytesWithLength(File.ReadAllBytes(dynamicallyLoadedAssemblyWithUpdates.Location));
            
            _connectedPeer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
        }
        else
        {
            Debug.LogWarning($"No client connected, changes will not be send (editor will still Hot-Reload).");
        }
    }
    
    void OnDestroy()
    {
        NetDebug.Logger = null;
        if (_netServer != null)
            _netServer.Stop();
    }
    
    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log($"New device connected from " + peer.EndPoint);
        _connectedPeer = peer;
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log($"Network error " + socketErrorCode);
    }
    
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.Broadcast)
        {
            Debug.Log($"Received discovery request. Send discovery response");
            var resp = new NetDataWriter();
            resp.Put(1);
            _netServer.SendUnconnectedMessage(resp, remoteEndPoint);
        }
    }
    
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Debug.Log($"client tires to connect " + request.RemoteEndPoint);
        request.Accept();
    }
    
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log($"client disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
        _connectedPeer = null;
    }
    
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
    }
    
    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }
}