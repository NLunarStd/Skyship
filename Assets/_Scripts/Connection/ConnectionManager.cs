using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using System.Collections;

public class ConnectionManager : NetworkBehaviour
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

    private bool isLeavingManually = false;

    // -------- SERVER DATA --------
    private readonly HashSet<string> _connectedNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ulong, string> _clientIdToName = new();
    private readonly Dictionary<ulong, int> _clientSlots = new();

    private bool[] _slotUsed;

    public NetworkVariable<FixedString32Bytes> roomCodeNet =
    new NetworkVariable<FixedString32Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        _slotUsed = new bool[spawnPoints.Length];

        roomCodeNet.OnValueChanged += OnRoomCodeChanged;
    }

    private void OnRoomCodeChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        if (roomCodeDisplay != null)
        {
            roomCodeDisplay.text = "Room Code: " + newValue.ToString();
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

        if (roomCodeDisplay != null)
        {
            roomCodeDisplay.text = "Room Code: " + roomCodeNet.Value;
        }
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
            Debug.Log("INVALID PAYLOAD");
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

        Debug.Log("APPROVAL OK!");

        ulong clientId = request.ClientNetworkId;

        _clientSlots[clientId] = slot;
        _clientIdToName[clientId] = username;
        StartCoroutine(SetNameNextFrame(clientId, username));
        _connectedNames.Add(username);

        response.Approved = true;
        response.CreatePlayerObject = true;
        //response.PlayerPrefabHash = null; // ใช้ default prefab
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

            GameManager.instance.EnterLobby();
        }

        if (roomCodeDisplay != null)
        {
            roomCodeDisplay.text = "Room Code: " + roomCodeNet.Value.ToString();
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

            if (_clientSlots.TryGetValue(clientId, out int slot))
            {
                GameManager.instance.SetPlayerName(slot, "None");
            }

            FreeSlot(clientId);
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            bool wasHost = NetworkManager.Singleton.IsHost;

            SetUIConnected(false);

            // ?? ถ้าเรากดออกเอง
            if (isLeavingManually)
            {
                SetError("You left the game");
                isLeavingManually = false;
                return;
            }

            string reason = NetworkManager.Singleton.DisconnectReason;

            if (!string.IsNullOrEmpty(reason))
            {
                if (reason.Contains("Name already in use"))
                {
                    SetError("invalid username");
                }
                else if (reason.Contains("Room is full"))
                {
                    SetError("room is full");
                }
                else
                {
                    SetError("Disconnected"); // ?? ไม่โชว์ raw error แล้ว
                }
            }
            else
            {
                if (!wasHost)
                    SetError("Disconnected");
                else
                    SetError("You left the game");
            }

            if (GameManager.instance != null)
            {
                GameManager.instance.ExitLobby();
            }
        }
    }

    // -------------------------
    // UI
    // -------------------------
    private void SetUIConnected(bool connected)
    {
        loginPanel.SetActive(!connected);

        if (leaveButton != null)
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
        isLeavingManually = true;

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
        string username = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            SetError("pls insert username");
            return;
        }

        LocalUsername = username;

        string joinCode = await relayManager.StartHostWithRelay(username, 0);

        roomCodeNet.Value = joinCode.ToString();

        //roomCodeDisplay.text = "Room Code: " + joinCode;


    }

    public async void StartClientWithRoom()
    {
        string code = roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            SetError("Enter room code");
            return;
        }

        string username = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            SetError("pls insert username");
            return;
        }

        LocalUsername = username;

        bool success = await relayManager.StartClientWithRelay(code, username, 0);

        if (!success)
        {
            SetError("Join failed");
            return;
        }

        GameManager.instance.EnterLobby();

    }

    // -------------------------
    // PAYLOAD
    // -------------------------
    private bool TryParseConnectionPayload(ArraySegment<byte> payload, out string username)
    {
        username = "";
        if (payload.Array == null || payload.Count == 0) return false;

        string decoded = Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);
        if (string.IsNullOrWhiteSpace(decoded)) return false;

        // แยก username ออกจาก characterId
        string[] parts = decoded.Split('|');
        username = parts[0].Trim();
        return true;
    }

    public Vector3 GetSpawnPosition(ulong clientId)
    {
        if (_clientSlots.TryGetValue(clientId, out int slot))
        {
            return spawnPoints[slot].position;
        }
        return Vector3.zero; // หรือจุดเกิดสำรอง
    }

    public int GetPlayerSlot(ulong clientId)
    {
        if (_clientSlots.TryGetValue(clientId, out int slot))
        {
            return slot;
        }

        return -1; // กันพลาด
    }

    public void CopyJoinCode()
    {
        if (roomCodeDisplay == null) return;

        string text = roomCodeDisplay.text;

        if (string.IsNullOrEmpty(text))
        {
            SetError("No room code to copy");
            return;
        }

        // ถ้า text เป็น "Room Code: ABC123" ? ตัดเอาแค่โค้ด
        string code = text.Replace("Room Code: ", "").Trim();

        GUIUtility.systemCopyBuffer = code;

        Debug.Log("Copied join code: " + code);

    }

    public string GetPlayerName(ulong clientId)
    {
        if (_clientIdToName.TryGetValue(clientId, out string name))
        {
            return name;
        }
        return "None";
    }

    private IEnumerator SetNameNextFrame(ulong clientId, string username)
    {
        yield return new WaitUntil(() => GameManager.instance != null && GameManager.instance.IsSpawned);

        int slot = GetPlayerSlot(clientId);

        if (slot >= 0)
        {
            GameManager.instance.SetPlayerName(slot, username);
        }
    }
}