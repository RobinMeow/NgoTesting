using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

// One moves, quicker, when moving from within the Unity Editor. Probably Update, Refreshrate, tick-rate idk.

public sealed class MoveWithServAuthRigidbody : Unity.Netcode.NetworkBehaviour
{
    [SerializeField] float _moveStength = 10.0f;
    [SerializeField] Rigidbody _rigidbody;
    
    NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>(
        Vector2.zero, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    MoveControls _moveControls;
    Vector2 _moveDirection = Vector2.zero;
    
    void Awake() {
        Assert.IsNotNull(_rigidbody, $"{nameof(_rigidbody)} may not be null.");
        Assert.IsTrue(_moveStength > 0.0f, $"{nameof(_moveStength)} has to be gt 0.");
    }
    
    public override void OnNetworkSpawn() {
        if (IsOwner)
        {
            _moveControls = new MoveControls();
            _moveControls.Enable();
            _moveControls.Cube.Movement.performed += (ctx)=>{
                _moveDirection = ctx.ReadValue<Vector2>();
            };
            _moveControls.Cube.Movement.canceled += (ctx)=>{
                _moveDirection = Vector2.zero;
            };
        }
        
        if (!IsServer)
        {
            // sync all non-servers
            _networkPosition.OnValueChanged += (p, n) => { // ToDo: unsub propperly
                transform.position = n;
            };
        }
    }
    
    void Update() {
        if (_moveDirection != Vector2.zero)
        {
            SendRigidbodyForceServerRpc(_moveDirection); 
        }
    }
    
    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = true)] 
    void SendRigidbodyForceServerRpc(Vector2 direction) {
        Vector2 movement = _moveStength * Time.deltaTime * direction;
        _rigidbody.MovePosition(_rigidbody.position + new Vector3(movement.x, movement.y, 0.0f));
        _networkPosition.Value = _rigidbody.position; 
    }
}
