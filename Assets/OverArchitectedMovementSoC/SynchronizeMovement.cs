using Unity.Netcode;
using UnityEngine;

public sealed class SynchronizeMovement : MovementStrategy
{
    public SynchronizeMovement(NetworkVariable<Vector2> networkVariable, Transform transform)
        : base(networkVariable, transform)
    {
    }

    public override void OnNetworkSpawn()
    {
        _networkPosition.OnValueChanged += SetPositionOnClient;
    }

    void SetPositionOnClient(Vector2 previousPosition, Vector2 newPosition)
    {
        _transform.position = (Vector3)newPosition;
    }

    public override void OnNetworkDespawn()
    {
        _networkPosition.OnValueChanged -= SetPositionOnClient;
    }
}
