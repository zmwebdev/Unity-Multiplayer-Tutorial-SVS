using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerNameUI : NetworkBehaviour
{
    [SerializeField] private UIDocument m_uiPlayerNameDocument;
    private Label m_playerNameLabel;
    
    private NetworkVariable<FixedString32Bytes> m_playerName = new NetworkVariable<FixedString32Bytes>(
        string.Empty,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake() {
        m_playerNameLabel = m_uiPlayerNameDocument.rootVisualElement.Q<Label>("NameLabel");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_playerName.OnValueChanged += HandlePlayerNameChanged;
        if (IsServer)
        {
            m_playerName.Value = $"Player {OwnerClientId}";
        }

        m_playerNameLabel.text = m_playerName.Value.ToString();
    }

    private void HandlePlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        m_playerNameLabel.text = newValue.ToString();
    }

    override public void OnNetworkDespawn()
    {
        m_playerName.OnValueChanged -= HandlePlayerNameChanged;
        base.OnNetworkDespawn();
    }
}
