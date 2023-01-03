using Unity.Netcode;
using UnityEngine;

public class CubeTest : NetworkBehaviour
{
    protected readonly NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
        Vector2.zero,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner
        );

    [SerializeField] float _movespeed = 5.0f;
    MovementStrategy _movementStrategy;

    public override void OnNetworkSpawn()
    {
        if (NetworkObject.IsOwner)
        {
            _movementStrategy = new OwnerAuthoritativeMovement(_networkPosition, transform, _movespeed);
        }
        else
        {
            _movementStrategy = new SynchronizeMovement(_networkPosition, transform);
        }

        _movementStrategy.OnNetworkSpawn();
    }

    void Update()
    {
        _movementStrategy.Update();
    }

    public override void OnNetworkDespawn()
    {
        _movementStrategy.OnNetworkDespawn();
    }
}
