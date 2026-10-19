
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(PlayerInput))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        // Only the owning client reads input.
        playerInput.enabled = IsOwner;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Vector2 input =
            playerInput.actions["Mugitu"].ReadValue<Vector2>();

        // A/D or Left/Right: rotate.
        transform.Rotate(
            Vector3.up * input.x * rotationSpeed * Time.deltaTime
        );

        // W/S or Up/Down: move forward/backward.
        transform.position +=
            transform.forward * input.y * moveSpeed * Time.deltaTime;

        // Check for the "Hitzegin" input action.
        if (playerInput.actions["Hitzegin"].WasPressedThisFrame())
        {
            Debug.Log("Hitzegin sakatu da");
        }
    }
}
