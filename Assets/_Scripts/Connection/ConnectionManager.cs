using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }
    public string LocalUsername { get; private set; } = "";

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject leaveButton;
    [SerializeField] private TMP_Text errorText;

    [Header("RELAY")]
    [SerializeField] private RelayManager relayManager;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_Text roomCodeDisplay;

    [Header("LOBBY SPAWN (4 slots)")]
    [SerializeField] private Transform[] spawnPoints;

    // -------- SERVER DATA --------
    private readonly HashSet<string> _connectedNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ulong, string> _clientIdToName = new();
    private readonly Dictionary<ulong, int> _clientSlots = new();

    private bool[] _slotUsed;

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        _slotUsed = new bool[spawnPoints.Length];
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    // -------------------------
    // SLOT SYSTEM
    // -------------------------
    private int GetAvailableSlot()
    {
        for (int i = 0; i < _slotUsed.Length; i++)
        {
            if (!_slotUsed[i])
            {
                _slotUsed[i] = true;
                return i;
            }
        }
        return -1;
    }

    private void FreeSlot(ulong clientId)
    {
        if (_clientSlots.TryGetValue(clientId, out int slot))
        {
            _slotUsed[slot] = false;
            _clientSlots.Remove(clientId);
        }
    }

    // -------------------------
    // CONNECTION APPROVAL
    // -------------------------
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        if (!TryParseConnectionPayload(request.Payload, out string username))
        {
            response.Approved = false;
            response.Reason = "Invalid payload";
            response.Pending = false;
            return;
        }

        if (_connectedNames.Contains(username))
        {
            response.Approved = false;
            response.Reason = "Name already in use";
            response.Pending = false;
            return;
        }

        int slot = GetAvailableSlot();
        if (slot == -1)
        {
            response.Approved = false;
            response.Reason = "Room is full";
            response.Pending = false;
            return;
        }

        ulong clientId = request.ClientNetworkId;

        _clientSlots[clientId] = slot;
        _clientIdToName[clientId] = username;
        _connectedNames.Add(username);

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null; // ใช้ default prefab
        response.Position = spawnPoints[slot].position;
        response.Rotation = Quaternion.Euler(0, 180, 0);
        response.Pending = false;

        Debug.Log($"Client {username} joined slot {slot}");
    }

    // -------------------------
    // EVENTS
    // -------------------------
    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            SetUIConnected(true);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetUIConnected(true);
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (_clientIdToName.TryGetValue(clientId, out string name))
            {
                _connectedNames.Remove(name);
                _clientIdToName.Remove(clientId);
            }

            FreeSlot(clientId);
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetUIConnected(false);
            SetError("Disconnected");
        }
    }

    // -------------------------
    // UI
    // -------------------------
    private void SetUIConnected(bool connected)
    {
        loginPanel.SetActive(!connected);
        leaveButton.SetActive(connected);

        if (connected)
            ClearError();
    }

    private void SetError(string msg)
    {
        if (errorText != null)
            errorText.text = msg;

        Debug.LogWarning(msg);
    }

    private void ClearError()
    {
        if (errorText != null)
            errorText.text = "";
    }

    public void OnLeaveButtonClick()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        loginPanel.SetActive(true);
        leaveButton.SetActive(false);

        roomCodeDisplay.text = "";
        roomCodeInput.text = "";
    }

    // -------------------------
    // RELAY HOST / JOIN
    // -------------------------
    public async void StartHostWithRoom()
    {
        string username = usernameInput.text;
        LocalUsername = username;

        string joinCode = await relayManager.StartHostWithRelay(username, 0);
        roomCodeDisplay.text = "Room Code: " + joinCode;
    }

    public async void StartClientWithRoom()
    {
        string code = roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            SetError("Enter room code");
            return;
        }

        string username = usernameInput.text;
        LocalUsername = username;

        bool success = await relayManager.StartClientWithRelay(code, username, 0);

        if (!success)
        {
            SetError("Join failed");
        }
    }

    // -------------------------
    // PAYLOAD
    // -------------------------
    private bool TryParseConnectionPayload(ArraySegment<byte> payload, out string username)
    {
        username = "";

        if (payload.Array == null || payload.Count == 0)
            return false;

        string decoded = Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);

        if (string.IsNullOrWhiteSpace(decoded))
            return false;

        username = decoded.Trim();
        return true;
    }
}