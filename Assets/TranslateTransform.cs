using UnityEngine;
using Unity.Netcode;

public sealed class TranslateTransform : NetworkBehaviour
{
    [SerializeField] float _movespeed = 5.0f;
    MoveControls _moveControls;
    Vector3 _moveDirection = Vector3.zero;
    
    public override void OnNetworkSpawn()
    {
        if (NetworkObject.IsOwner)
        {
            // Todo: do properly
            _moveControls = new MoveControls();
            _moveControls.Enable();
            _moveControls.Cube.Movement.performed += (ctx) => _moveDirection = ctx.ReadValue<Vector2>();
            _moveControls.Cube.Movement.canceled += (_) => _moveDirection = Vector3.zero;
        }
    }
    
    void Update() 
    {
        if (_moveDirection != Vector3.zero)
        {
            Vector3 movement = _movespeed * Time.deltaTime * _moveDirection;
            transform.Translate(movement, Space.Self);
        }
    }
}
