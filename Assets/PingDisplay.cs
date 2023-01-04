using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class PingDisplay : NetworkBehaviour
{
    [SerializeField] float _refreshRateInSeconds = 1.0f;
    [SerializeField] TextMeshProUGUI _tmpPing;
    readonly NetworkVariable<double> _ping = new NetworkVariable<double>(
        -1.0f,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
        );

    float _timePassed = 0.0f;

    void Awake()
    {
        Assert.IsNotNull(_tmpPing, $"{nameof(_tmpPing)} may not be null.");
        SetDefaultDisplay();
    }

    void SetDefaultDisplay()
    {
        _tmpPing.text = "00ms (offline)";
    }

    void Update()
    {
        if (IsClient)
        {
            PingServerRpc(DateTime.Now.Ticks); // ToDo: use OnValueChange and refresh UI in there. And use time to send RPCs

            _timePassed += Time.deltaTime;
            if (_timePassed > _refreshRateInSeconds)
            {
                Refresh(_ping.Value);
                _timePassed = 0.0f;
            }
        }
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable, RequireOwnership = false)]
    void PingServerRpc(long ticks)
    {
        _ping.Value = (DateTime.Now - new DateTime(ticks)).TotalMilliseconds;
    }

    void Refresh(double ping)
    {
        _tmpPing.text = $"{ping:00}ms";
    }

    public override void OnNetworkDespawn()
    {
        SetDefaultDisplay();
    }
}
