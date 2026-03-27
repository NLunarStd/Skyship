using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using Unity.Networking.Transport.Relay;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private int maxPlayers = 4;

    private bool _isInitializing;

    // -------------------------
    // Initialize Unity Services
    // -------------------------
    private async Task InitializeUnityServices()
    {
        if (_isInitializing) return;
        _isInitializing = true;

        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        _isInitializing = false;
    }

    // -------------------------
    // HOST (Create Room)
    // -------------------------
    public async Task<string> StartHostWithRelay(string username, int characterId)
    {
        await InitializeUnityServices();

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        // ? ADD THIS
        string payload = username + "|" + characterId;
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            System.Text.Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.StartHost();

        Debug.Log($"Relay Host started. Join Code: {joinCode}");

        return joinCode;
    }

    // -------------------------
    // CLIENT (Join Room)
    // -------------------------
    public async Task<bool> StartClientWithRelay(string joinCode, string username, int characterId)
    {
        await InitializeUnityServices();

        try
        {
            JoinAllocation joinAllocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            // ? ADD THIS
            string payload = username + "|" + characterId;
            NetworkManager.Singleton.NetworkConfig.ConnectionData =
                System.Text.Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.StartClient();
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Error: {e.Message}");
            return false;
        }
    }
}