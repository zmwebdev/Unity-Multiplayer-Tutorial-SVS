using Unity.Netcode;
using UnityEngine;

public class ResourceSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject m_woodPrefab, m_stonePrefab;

    public void SpawnResource(ObjectType type, Vector3 position)
    {
        // only server can spawn resources
        if (!IsServer) return;

        NetworkObject resource = type == ObjectType.Wood ? m_woodPrefab : m_stonePrefab;

        GameObject spawnedResource = Instantiate(resource.gameObject, position,
            Quaternion.Euler(0, Random.Range(0, 360), 0));
        
        spawnedResource.GetComponent<NetworkObject>().Spawn();
    }
}
