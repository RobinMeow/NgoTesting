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
        _networkPosition.OnValueChanged += SetLocalPosition;
    }

    /// <summary>
    /// sets the local position on the current client instance. Real (newPosition) position comes from the Server.
    /// </summary>
    void SetLocalPosition(Vector2 previousPosition, Vector2 newPosition)
    {
        _transform.position = (Vector3)newPosition;
    }

    public override void OnNetworkDespawn()
    {
        _networkPosition.OnValueChanged -= SetLocalPosition;
    }
}
