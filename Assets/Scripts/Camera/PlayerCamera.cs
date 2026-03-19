using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using System.Collections;
using UnityEngine.InputSystem;
public class PlayerCamera : NetworkBehaviour 
{
    CinemachineVirtualCamera virtualCamera;
    public Transform respawnPoint;
    int spectateIndex = 0;
    bool isDead = false;

    InputActionReference changeCameraSpectate;
    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            virtualCamera = GameObject.Find("MainVirtualCamera").GetComponent<CinemachineVirtualCamera>();
            SetCameraTarget(transform);
        }
    }

    [ClientRpc]
    public void OnplayerDeathClientRPC()
    {
        if (!IsLocalPlayer) 
        { return; }
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        isDead = true;

        SetCameraTarget(respawnPoint);

        float timer = 10f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if(changeCameraSpectate.action.WasPressedThisFrame())
            {
                SpectateNextPlayer();
            }
            yield return null;
        }
    }

    void SpectateNextPlayer()
    {
        var players = NetworkManager.Singleton.ConnectedClientsList;
        if (players.Count == 0) { return; }

        spectateIndex = (spectateIndex + 1) % players.Count;
        var targetPlayer = players[spectateIndex].PlayerObject;
        if (targetPlayer != null)
        {
            SetCameraTarget(targetPlayer.transform);
        }
        else
        {
            SetCameraTarget(respawnPoint);
        }
    }

    void SetCameraTarget(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
    }
}
