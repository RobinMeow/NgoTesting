using Unity.Netcode;
using UnityEngine;

public abstract class MovementStrategy 
{
    /// <summary>
    /// has to be passed in, because a NetworkVariable can only be created by a NetworkBehavior/Object attached to a GameObject
    /// </summary>
    protected readonly NetworkVariable<Vector2> _networkPosition;

    protected Transform _transform;

    protected MovementStrategy(NetworkVariable<Vector2> networkVariable, Transform transform)
    {
        _transform = transform;
        _networkPosition = networkVariable;
    }

    public virtual void OnNetworkSpawn() { }
    public virtual void Update() { }
    public virtual void OnNetworkDespawn() { }
}
