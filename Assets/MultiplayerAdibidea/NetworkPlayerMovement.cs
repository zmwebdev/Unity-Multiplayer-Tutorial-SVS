using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        // Only the owning client should read this player's input.
        playerInput.enabled = IsOwner;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Vector2 input =
            playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 move = new Vector3(input.x, 0f, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);

        // Check for the "Hitzegin" input action
        if (playerInput.actions["Hitzegin"].WasPressedThisFrame())
        {
            Debug.Log($"Hitzegin sakatu da");
        }
        
    }
}