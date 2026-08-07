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

    private bool m_isInteracting, m_isChopping;
    [SerializeField] private GameObject m_axeModel, m_pickAxeModel, m_woodModel, m_stoneModel;

    // This is the network variable that will hold the network object id 
    // of the pickable item that the player is currently holding
    private NetworkVariable<ulong> m_heldNetworkObjectId = new(ulong.MaxValue);
    private NetworkVariable<ObjectType> m_heldObjectType = new(ObjectType.None);

    private void OnEnable()
    {
        m_playerInput.OnPickUpPressed += HandlePickUpPressed;
        m_playerInput.OnInteractPressed += HandleActionPressed;
    }

    private void OnDisable()
    {
        m_playerInput.OnPickUpPressed -= HandlePickUpPressed;
        m_playerInput.OnInteractPressed += HandleActionPressed;
    }

    private void HandlePickUpPressed()
    {
        if (m_isInteracting || m_isChopping)
            return;
        if(m_interactionDetector.ClosestInteractable == null)
            return;
        m_animator.SetBool("Interact", true);
        m_isInteracting = true;
    }

    private void HandleActionPressed()
    {
        if(IsOwner == false)
        {
            return;
        }
        if (m_isChopping || m_isInteracting)
            return;
        if(m_heldObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            m_isChopping = true;
            m_animator.SetTrigger("Chop");
        }
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
            m_animationEvents.OnChop += HandleChopAction;
        }
    }

    private void HandleChopAction()
    {
          if(m_heldObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            if(m_interactionDetector.ClosestInteractable is ResourceNode)
            {
                RequestResourceNodeInteractionServerRpc(
                    m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestResourceNodeInteractionServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects
               .TryGetValue(networkObjectId, out NetworkObject target))
            return;

        if (!target.TryGetComponent(out ResourceNode node))
            return;

        node.Harvest(m_heldObjectType.Value);
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

        // Avoid sending RPCs during despawn; this object may already be unspawned.
        // Tutorialean beste modu batean agertzen da, baino, errorea ematen du, beraz, 
        // ez da RPC bidali behar despawn denean.
        if (IsServer)
        {
            DropCurrentItem();
        }

        if (IsOwner)
        {
            m_animationEvents.OnInteract -= HandleInteractAction;
            m_animationEvents.OnAnimationDone -= HandleAnimationDone;
            m_animationEvents.OnChop -= HandleChopAction;
        }
        base.OnNetworkDespawn();
    }

    private void HandleAnimationDone()
    {
        m_isInteracting = false;
        m_isChopping = false;
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
        if (m_heldObjectType.Value != ObjectType.None)
        {
            DropCurrentItem();
        }
        if (pickableItem is PickableTool)
        {
            m_heldNetworkObjectId.Value = networkObjectId;
        }

        m_heldObjectType.Value = pickableItem.objectType;

        pickableItem.PickUp();
    }

    private void DropCurrentItem()
    {
        if (IsServer == false)
        {
            return;
        }
        if (m_heldObjectType.Value == ObjectType.None)
        {
            m_heldNetworkObjectId.Value = ulong.MaxValue;
            return;
        }
        if (m_heldObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                m_heldNetworkObjectId.Value, out NetworkObject target))
            {
                if (target.TryGetComponent<PickableTool>(out PickableTool pickableItem))
                {
                    pickableItem.Drop(transform.position);
                }
            }
        }
        m_heldObjectType.Value = ObjectType.None;
        m_heldNetworkObjectId.Value = ulong.MaxValue;
    }

    private void Update()
    {
        if (IsOwner == false)
        {
            return;
        }
        
        Vector2 movementInput = m_playerInput.MovementInput;
        
        if(m_isChopping || m_isInteracting)
        {
            movementInput = Vector2.zero;
        }

        m_agentMover.Move(movementInput);
    }
}
