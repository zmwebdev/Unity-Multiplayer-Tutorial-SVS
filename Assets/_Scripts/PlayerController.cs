using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private MyPlayerInput m_playerInput;
    [SerializeField] private AgentMover m_agentMover;
    [SerializeField] private InteractionDetector m_interactionDetector;
    [SerializeField] private Animator m_animator;
    [SerializeField] private AnimationEvents m_animationEvents;

    private bool m_isInteracting;
    [SerializeField] private GameObject m_axeModel, m_pickAxeModel, m_woodModel, m_stoneModel;

    // This is the network variable that will hold the network object id 
    // of the pickable item that the player is currently holding
    private NetworkVariable<ulong> m_heldNetworkObjectId = new(ulong.MaxValue);
    private NetworkVariable<ObjectType> m_heldObjectType = new(ObjectType.None);

    private void OnEnable()
    {
        if (m_playerInput != null)
        {
            m_playerInput.OnPickUpPressed += HandlePickUpPressed;
        }
    }

    private void OnDisable()
    {
        if (m_playerInput != null)
        {
            m_playerInput.OnPickUpPressed -= HandlePickUpPressed;
        }
    }

    private void HandlePickUpPressed()
    {
        if (IsOwner == false)
        {
            return;
        }

        if (m_isInteracting)
        {
            return;
        }

        if (m_interactionDetector.ClosestInteractable == null)
        {
            return;
        }

        m_animator.SetBool("Interact", true);
        m_isInteracting = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_interactionDetector.Initialize(IsOwner);
        m_heldObjectType.OnValueChanged += HandleHeldItemChanged;
        HandleItemOnJoin();

        if (IsOwner)
        {
            m_animationEvents.OnInteract += HandleInteractAction;
            m_animationEvents.OnAnimationDone += HandleAnimationDone;
        }
    }

    private void HandleItemOnJoin()
    {
        if (m_heldObjectType.Value != ObjectType.None)
        {
            HandleHeldItemChanged(ObjectType.None, m_heldObjectType.Value);
        }
    }

    private void HandleHeldItemChanged(ObjectType previousValue, ObjectType newValue)
    {
        m_axeModel.SetActive(newValue == ObjectType.Axe);
        m_pickAxeModel.SetActive(newValue == ObjectType.PickAxe);
        m_woodModel.SetActive(newValue == ObjectType.Wood);
        m_stoneModel.SetActive(newValue == ObjectType.Stone);
    }

    override public void OnNetworkDespawn()
    {
        m_heldObjectType.OnValueChanged -= HandleHeldItemChanged;
        base.OnNetworkDespawn();
        if (IsOwner)
        {
            m_animationEvents.OnInteract -= HandleInteractAction;
            m_animationEvents.OnAnimationDone -= HandleAnimationDone;
        }
    }

    private void HandleAnimationDone()
    {
        m_isInteracting = false;
    }

    private void HandleInteractAction()
    {
        if (m_interactionDetector.ClosestInteractable is PickableBase)
        {
            RequestPickUpServerRpc(
                m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
        }

        PickableBase pickable = m_interactionDetector.ClosestInteractable as PickableBase;
        if (pickable != null)
        {
            pickable.PickUp();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestPickUpServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects
            .TryGetValue(networkObjectId, out NetworkObject target))
        {
            return;
        }
        if (!target.TryGetComponent<PickableBase>(out PickableBase pickableItem))
        {
            return;
        }
        if (!pickableItem.canBePickedUp)
        {
            return;
        }
        if (pickableItem is PickableTool)
        {
            m_heldNetworkObjectId.Value = networkObjectId;
        }

        pickableItem.PickUp();
    }

    private void Update()
    {
        if (IsOwner == false)
        {
            return;
        }
        Vector2 movementInput = m_playerInput.MovementInput;
        m_agentMover.Move(movementInput);
    }
}
