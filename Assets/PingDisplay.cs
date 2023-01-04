using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

// There is a problem currently. Where when the host is having a 0ms ping, it doesnt update, on late joining clients, because the 
// NetworkVariable.OnValueChanged is only raised, when the value change is high enough. Therefore late joining clients dont recieve the 
// the host's ping.

public sealed class PingDisplay : NetworkBehaviour
{
    [SerializeField] float _refreshRateInSeconds = 1.0f;
    [SerializeField] TMP_Text _tmpPing;
    readonly NetworkVariable<double> _ping = new NetworkVariable<double>(
        -1.0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server // I could make it ownerWrite, but by using the ServerRpc I can calculate the Ping c:
        );

    float _timePassed = 0.0f;

    void Awake()
    {
        Assert.IsNotNull(_tmpPing, $"{nameof(_tmpPing)} may not be null.");
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _ping.OnValueChanged += OnPingChanged;
            SetText(_ping.Value); // OnValueChanged will not be called, if the ms is veeery constant. 
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            _timePassed += Time.deltaTime;
            if (_timePassed > _refreshRateInSeconds)
            {
                long ticks = DateTime.Now.Ticks;
                ChangePingServerRpc(ticks);
                _timePassed = 0.0f;
            }
        }
    }
    
    void OnPingChanged(double previousPing, double newPing)
    {
        SetText(ping: newPing);
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable, RequireOwnership = false)]
    void ChangePingServerRpc(long ticks)
    {
        double ping = (DateTime.Now - new DateTime(ticks)).TotalMilliseconds;
        _ping.Value = ping;
    }

    void SetText(double ping)
    {
        _tmpPing.text = $"{ping:0} ms";
        _tmpPing.color = ping < 40d // ToDo: Use user-defined color gradient
        ? Color.green 
        : ping < 100 
            ? Color.yellow
            : Color.red;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _ping.OnValueChanged -= OnPingChanged;
        }
        
        _tmpPing.text = "offline";
    }
}
