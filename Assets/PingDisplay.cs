using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public sealed class PingDisplay : NetworkBehaviour
{
    TextMeshProUGUI _tmpPing;
    NetworkVariable<double> _ping = new NetworkVariable<double>();
    [SerializeField] float _refreshRateInSeconds = 1.0f;
    float _timePassed = 0.0f;

    void Awake()
    {
        _tmpPing = GetComponent<TextMeshProUGUI>();
        SetDefaultDisplay();
    }

    void SetDefaultDisplay()
    {
        _tmpPing.text = "00 ms (offline)";
    }

    void Update()
    {
        if (IsClient)
        {
            PingServerRpc(DateTime.Now.Ticks);

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
