using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class OwnerAuthoritativeMovement : MovementStrategy
{
    readonly float _movespeed;
    readonly MoveControls _moveControls;
    Vector3 _movementDirection = Vector3.zero;

    public OwnerAuthoritativeMovement(NetworkVariable<Vector2> networkVariable, Transform transform, float movespeed)
        : base(networkVariable, transform)
    {
        _movespeed = movespeed;
        _moveControls = new MoveControls();
    }

    public override void OnNetworkSpawn()
    {
        _moveControls.Cube.Movement.performed += SetInputMoveDirection;
        _moveControls.Cube.Movement.canceled += SetInputMoveDirection;
        _moveControls.Enable();
    }

    void SetInputMoveDirection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 input = context.ReadValue<Vector2>();
            _movementDirection = input; 
            // _movementDirection = new Vector3(input.x, 0.0f, input.y); // use for 3D movement
        }
        else if (context.canceled)
        {
            _movementDirection = Vector3.zero;
        }
    }

    public override void Update()
    {
        if (_movementDirection != Vector3.zero) 
        {
            _transform.Translate(_movespeed * Time.deltaTime * _movementDirection, Space.Self);
            _networkPosition.Value = _transform.position;
        }
    }

    public override void OnNetworkDespawn()
    {
        _moveControls.Disable();
        _moveControls.Cube.Movement.performed -= SetInputMoveDirection;
        _moveControls.Cube.Movement.canceled -= SetInputMoveDirection;
    }
}
