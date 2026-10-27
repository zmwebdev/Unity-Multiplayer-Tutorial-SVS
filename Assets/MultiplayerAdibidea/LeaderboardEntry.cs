using System;
using Unity.Collections;
using Unity.Netcode;

public struct LeaderboardEntry : INetworkSerializable, IEquatable<LeaderboardEntry>
{
    public ulong ClientId;
    public FixedString64Bytes Name;
    public float Health;
    public float MaxHealth;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref MaxHealth);
    }

    public bool Equals(LeaderboardEntry other) =>
        ClientId == other.ClientId && Name.Equals(other.Name) &&
        Health == other.Health && MaxHealth == other.MaxHealth;
}
