using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple Player movement using NEW Input System (WASD keys)
/// New Input System erabiltzen duen player mugimendua (WASD)
///
/// Setup ikasteko / To learn setup:
/// → Assets/_Scripts/SimpleBehaviors/00_SCENE_SETUP.md
///
/// Setup Steps:
/// 1. Install Input System package (Package Manager)
/// 2. Create PlayerInputActions asset (Right-click → Create → Input Actions)
/// 3. Add "Move" action (Vector2, WASD 2D Vector Composite)
/// 4. Generate C# class (Inspector → Generate C# Class checkbox)
/// 5. Add Player Input component to Player GameObject
/// 6. Assign PlayerInputActions asset to Player Input → Actions field
/// 7. Add this script to Player GameObject
/// 8. Play and use W/A/S/D keys!
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SimplePlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found! Add Rigidbody to Player GameObject.");
            enabled = false;
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log("SimplePlayerMovement initialized. Use W/A/S/D to move.");
        }
    }

    /// <summary>
    /// Called automatically by Input System through Player Input component
    /// Input System-ek automatikoki deitzen du Player Input component bidez
    ///
    /// IMPORTANT: Method name must match action name in Input Actions!
    /// Method izena action izenarekin bat etorri behar da!
    /// </summary>
    public void OnMove(InputValue value)
    {
        // Irakurri Vector2 input balioa (x = horizontal, y = vertical)
        moveInput = value.Get<Vector2>();

        if (showDebugInfo && moveInput.magnitude > 0)
        {
            Debug.Log($"Move Input: X={moveInput.x:F2}, Y={moveInput.y:F2}");
        }
    }

    private void FixedUpdate()
    {
        // FixedUpdate erabiltzea Rigidbody-rekin (fisika zuzena)
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Mugitu soilik input badago (magnitude > 0.1 threshold)
        if (moveInput.magnitude >= 0.1f)
        {
            // Bihurtu 2D input (x, y) 3D mugimendu bektore batera (x, 0, z)
            // Input.x → World.x (left/right)
            // Input.y → World.z (forward/backward)
            // NOTE: Y negated because Input System "up" is -Z in world space
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            // Kalkulatu posizio berria
            Vector3 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

            // Mugitu Rigidbody (MovePosition fisika hobea da transform.position baino)
            rb.MovePosition(newPosition);

            // Biratu mugimenduko noranzkoan (karakterea aurrera begiratzen)
            if (movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo || !Application.isPlaying) return;

        // Erakutsi mugimenduko norabidea Scene View-n
        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            // Berdea: Mugimenduko norabidea
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + movement * 2f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up + movement * 2f, 0.2f);
        }

        // Ziana: Karakterea non dagoen
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    // Helper methods (optional)

    /// <summary>
    /// Get current move input (for other scripts)
    /// Oraingo move input-a lortu (beste script-entzat)
    /// </summary>
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    /// <summary>
    /// Is player moving?
    /// Jokalaria mugitzen ari da?
    /// </summary>
    public bool IsMoving()
    {
        return moveInput.magnitude >= 0.1f;
    }

    /// <summary>
    /// Get movement direction in world space
    /// Mugimenduko norabidea world space-n lortu
    /// </summary>
    public Vector3 GetMovementDirection()
    {
        if (moveInput.magnitude < 0.1f) return Vector3.zero;
        return new Vector3(moveInput.x, 0f, moveInput.y).normalized;
    }
}
