using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public struct ClientPing : INetworkSerializeByMemcpy, IEquatable<ClientPing>
{
    public ulong ClientId;
    public double Ping;
    
    public ClientPing(ulong clientId, double ping)
    {
        ClientId = clientId;
        Ping = ping;
    }

    public bool Equals(ClientPing other)
    {
        return ClientId == other.ClientId;
    }
}

// Should only be a single intance in scene
public sealed class PingDisplay : NetworkBehaviour
{
    [SerializeField] float _refreshRateInSeconds = 1.0f;
    [SerializeField] TextMeshProUGUI _tmpPing;
    // NetworkVariable<double> _ping = new NetworkVariable<double>(
    //     -1.0f,
    //     NetworkVariableReadPermission.Owner,
    //     NetworkVariableWritePermission.Server
    // );
    NetworkList<ClientPing> _clientPings = new NetworkList<ClientPing>(
        default,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );

    void Awake()
    {
        Assert.IsNotNull(_tmpPing, $"{nameof(_tmpPing)} may not be null.");
        SetDefaultDisplay();
    }

    void SetDefaultDisplay()
    {
        _tmpPing.text = "00ms (offline)";
    }

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            Debug.LogError("This should never happn, it is for future use, when I implement standalone server.");
            _tmpPing.text = "00ms (server)";
            enabled = false;
            return;
        }
        
        WaitForSeconds refreshRate = new WaitForSeconds(_refreshRateInSeconds);
        RegisterClientPingServerRpc(DateTime.Now.Ticks);
        StartCoroutine(SendPings(refreshRate));
        _clientPings.OnListChanged += OnClientPingChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    void RegisterClientPingServerRpc(long ticks, ServerRpcParams rpcPara = default)
    {
        if (_clientPings == null)
        {
            _clientPings = new NetworkList<ClientPing>(default, );
        }
        double ping = (DateTime.Now - new DateTime(ticks)).TotalMilliseconds;
        ClientPing clientPing = new ClientPing(rpcPara.Receive.SenderClientId, ping);
        _clientPings.Add(clientPing);
    }

    void OnClientPingChanged(NetworkListEvent<ClientPing> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<ClientPing>.EventType.Value)
        {
            double ping = _clientPings[changeEvent.Index].Ping;
            SetPingText(ping);
        } 
    }

    IEnumerator SendPings(WaitForSeconds refreshRate)
    {
        while (NetworkObject.IsSpawned)
        {
            Debug.Log($"Ping request sent to server, by: {OwnerClientId}");
            PingServerRpc(DateTime.Now.Ticks); 
            yield return refreshRate;
        }
        
        Debug.Log($"Ping is no longer refreshing because '{name}' despawned.");
        SetDefaultDisplay();
    }
            
    void SetPingText(double ping)
    {
        _tmpPing.text = $"{ping:00}ms";
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable, RequireOwnership = false)]
    void PingServerRpc(long ticks, ServerRpcParams rpcPara = default)
    {
        ulong senderClientId = rpcPara.Receive.SenderClientId;
        double ping = (DateTime.Now - new DateTime(ticks)).TotalMilliseconds;
        for (byte i = 0; i < _clientPings.Count; i++)
        {
            ClientPing clientPing = _clientPings[i];
            if (clientPing.ClientId == senderClientId)
            {
                clientPing.Ping = ping; // can tets it, if this one raises the on changed event, than the next line isnt needed.
                _clientPings[i] = clientPing; // idk if this is necessary. but its a struct, after all.
                Debug.Log($"Ping ({ping}) updated by server, requested by: {senderClientId}");
                return;
            }            
        }
    }
}
