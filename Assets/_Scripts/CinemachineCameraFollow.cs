using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;


public class CinemachineCameraFollow : NetworkBehaviour
{
    private CinemachineCamera virtualCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        virtualCamera = FindFirstObjectByType<CinemachineCamera>();

        virtualCamera.Target.TrackingTarget = transform;
    }
}