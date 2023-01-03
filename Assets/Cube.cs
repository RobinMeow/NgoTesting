using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cube : NetworkBehaviour
{
    [SerializeField] float _movespeed = 5.0f;
    MoveControls _moveControls;
    Vector3 _movementDirection = Vector3.zero;
    NetworkVariable<Vector3> _networkMoveDirection = new NetworkVariable<Vector3>(
        Vector3.zero,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner
        );

    public override void OnNetworkSpawn()
    {
        if (NetworkObject.IsOwner)
        {
            // Read inputs, only from the owner instance of the game.
            _moveControls = new MoveControls();
            _moveControls.Cube.Movement.performed += SetInputMoveDirection;
            _moveControls.Cube.Movement.canceled += SetInputMoveDirection;
            _moveControls.Enable();
        }
        else
        {
            // non-owner have to update the movement via the network varaible.
            _networkMoveDirection.OnValueChanged += SetNetworkMoveDirection;
        }
    }

    void SetNetworkMoveDirection(Vector3 previousValue, Vector3 newValue)
    {
        _movementDirection = newValue;
    }

    void SetInputMoveDirection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 input = context.ReadValue<Vector2>(); // use the comment below for 3D movement!
            _movementDirection = input; // new Vector3(input.x, 0.0f, input.y);
            _networkMoveDirection.Value = _movementDirection;
        }
        else if (context.canceled)
        {
            _movementDirection = Vector3.zero;
            _networkMoveDirection.Value = _movementDirection;
        }
    }

    void Update()
    {
        if (_movementDirection != Vector3.zero)
        {
            Move();
        }
    }

    void Move()
    {
        transform.Translate(_movespeed * Time.deltaTime * _movementDirection);
    }

    public override void OnNetworkDespawn()
    {
        _moveControls.Disable();
        _moveControls.Cube.Movement.performed -= SetInputMoveDirection;
        _moveControls.Cube.Movement.canceled -= SetInputMoveDirection;
    }
}
