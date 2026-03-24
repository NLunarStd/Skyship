using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System;
using Unity.Netcode.Components;
using Unity.Collections;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    private bool isInLobby = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // กันโดนสร้างซ้ำ
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [Header("Timer reference")]
    public TextMeshProUGUI timer;
    public float matchTime = 300f;
    private bool isGameStarted = false;

    [Header("Countdown Settings")]
    public TextMeshProUGUI countDownInThree;
    public float scaleUpMultiplier = 1.5f;
    public float animationSpeed = 5f;

    [Header("Player reference")]
    public PlayerUI[] playerUIs;
    public NetworkVariable<int> scoreP1 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> scoreP2 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> scoreP3 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> scoreP4 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("CANVAS")]
    public GameObject MenuCanvas;
    public GameObject LobbyCanvas;
    public GameObject GamePlayCanvas;

    [Header("CAMERA")]
    public Camera mainCam;
    public Camera lobbyCam;

    [Header("Lobby Canvas")]
    public TMP_Text copyCodeText;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private TMP_Text waitForHostText;

    public NetworkVariable<FixedString32Bytes> playerName1 = new("None");
    public NetworkVariable<FixedString32Bytes> playerName2 = new("None");
    public NetworkVariable<FixedString32Bytes> playerName3 = new("None");
    public NetworkVariable<FixedString32Bytes> playerName4 = new("None");

    public void EnterLobby()
    {
        MenuCanvas.SetActive(false);
        LobbyCanvas.SetActive(true);
        GamePlayCanvas.SetActive(false);

        SetupLobbyUI();
    }


    [System.Serializable]
    public class PlayerUI
    {
        public Transform root;
        public TextMeshProUGUI nameLabel;
        public Slider scoreSlider;
        public TextMeshProUGUI scoreLabel;
    }

    public override void OnNetworkSpawn()
    {
        scoreP1.OnValueChanged += (oldVal, newVal) => UpdateUI(0, newVal);
        scoreP2.OnValueChanged += (oldVal, newVal) => UpdateUI(1, newVal);
        scoreP3.OnValueChanged += (oldVal, newVal) => UpdateUI(2, newVal);
        scoreP4.OnValueChanged += (oldVal, newVal) => UpdateUI(3, newVal);

        UpdateUI(0, scoreP1.Value);
        UpdateUI(1, scoreP2.Value);
        UpdateUI(2, scoreP3.Value);
        UpdateUI(3, scoreP4.Value);

        playerName1.OnValueChanged += (o, n) => playerUIs[0].nameLabel.text = n.ToString();
        playerName2.OnValueChanged += (o, n) => playerUIs[1].nameLabel.text = n.ToString();
        playerName3.OnValueChanged += (o, n) => playerUIs[2].nameLabel.text = n.ToString();
        playerName4.OnValueChanged += (o, n) => playerUIs[3].nameLabel.text = n.ToString();

        // initial
        playerUIs[0].nameLabel.text = playerName1.Value.ToString();
        playerUIs[1].nameLabel.text = playerName2.Value.ToString();
        playerUIs[2].nameLabel.text = playerName3.Value.ToString();
        playerUIs[3].nameLabel.text = playerName4.Value.ToString();

        if (IsClient)
        {
            EnterLobby();
        }

    }

    void UpdateUI(int index, int newValue)
    {
        if (index < playerUIs.Length)
        {
            playerUIs[index].scoreSlider.value = newValue;
            playerUIs[index].scoreLabel.text = newValue.ToString();
        }
    }

    [ClientRpc]
    private void StartCountdownClientRpc()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        print("Starting Countdown");
        countDownInThree.gameObject.SetActive(true);

        string[] countdownSteps = { "3", "2", "1", "Start!" };

        foreach (var step in countdownSteps)
        {
            countDownInThree.text = step;
            yield return StartCoroutine(AnimateTextScale(countDownInThree.transform));
        }

        countDownInThree.gameObject.SetActive(false);
        isGameStarted = true;
    }

    private IEnumerator AnimateTextScale(Transform target)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * scaleUpMultiplier;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * animationSpeed;
            target.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * animationSpeed;
            target.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
    }

    public void Update()
    {
        if (isGameStarted)
        {
            UpdateTimer();
        }
    }

    public void UpdateTimer()
    {
        if (!IsServer) return;

        matchTime -= Time.deltaTime;
        if (matchTime <= 0)
        {
            matchTime = 0;
            isGameStarted = false;
            EndMatch();
        }

        SyncTimerClientRpc(matchTime);
    }

    [ClientRpc]
    private void SyncTimerClientRpc(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timer.text = $"{minutes:00}:{seconds:00}";
    }



    public void UpdatePlayerScore(int playerIndex, int scoreToAdd)
    {
        if (!IsServer) return;

        switch (playerIndex)
        {
            case 0: scoreP1.Value += scoreToAdd; break;
            case 1: scoreP2.Value += scoreToAdd; break;
            case 2: scoreP3.Value += scoreToAdd; break;
            case 3: scoreP4.Value += scoreToAdd; break;
        }
    }

    public void EndMatch()
    {
        Debug.Log("Match End");
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        Debug.Log("SERVER RPC CALLED");
        StartGame();

    }
    private void StartGame()
    {
        if (!IsServer) return;

        isInLobby = false;
        ItemPoolManager.Instance.InitializePools();

        foreach (var spawner in FindObjectsOfType<ItemSpawner>())
        {
            spawner.StartSpawning();
        }

        StartCoroutine(TeleportAllPlayers());

        SwitchToGameplayUIClientRpc();
        StartCountdownClientRpc();
    }

    private IEnumerator TeleportAllPlayers()
    {

        yield return null;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject;
            if (player == null) continue;

            int slot = ConnectionManager.Instance.GetPlayerSlot(client.ClientId);
            Vector3 targetPos = GetGameSpawnPosition(slot);

            if (player.TryGetComponent<NetworkTransform>(out var netTransform))
            {
                netTransform.Teleport(targetPos, Quaternion.identity, player.transform.localScale);
            }
            else
            {
                player.transform.position = targetPos;
            }

            Debug.Log($"Teleported Client {client.ClientId} to Slot {slot}");
        }
        yield return null;
    }
    //private void StartGame()
    //{
    //    if (!IsServer) return;

    //    isInLobby = false;

    //    ItemPoolManager.Instance.InitializePools();


    //    foreach (var spawner in FindObjectsOfType<ItemSpawner>())
    //    {
    //        spawner.StartSpawning();
    //    }

    //    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
    //    {
    //        Debug.Log($"Client {client.ClientId}");

    //        var player = client.PlayerObject;

    //        if (player == null)
    //        {
    //            Debug.LogError("PlayerObject is NULL!");
    //            continue;
    //        }

    //        int slot = ConnectionManager.Instance.GetPlayerSlot(client.ClientId);
    //        Debug.Log($"Slot = {slot}");

    //        Vector3 pos = GetGameSpawnPosition(slot);

    //        var netTransform = player.GetComponent<NetworkTransform>();

    //        if (netTransform != null)
    //        {
    //            netTransform.Teleport(pos, Quaternion.identity, player.transform.localScale);
    //        }
    //        else
    //        {
    //            player.transform.position = pos;
    //        }
    //    }

    //    SwitchToGameplayUIClientRpc();
    //    StartCountdownClientRpc();
    //}

    [ClientRpc]
    private void SwitchToGameplayUIClientRpc()
    {
        mainCam.gameObject.SetActive(true);
        lobbyCam.gameObject.SetActive(false);

        LobbyCanvas.SetActive(false);
        GamePlayCanvas.SetActive(true);
    }

    [SerializeField] private Transform[] gameSpawnPoints;

    private Vector3 GetGameSpawnPosition(int slot)
    {
        if (slot >= 0 && slot < gameSpawnPoints.Length)
        {
            return gameSpawnPoints[slot].position;
        }
        return Vector3.zero;
    }

    public bool IsInLobby()
    {
        return isInLobby;
    }

    public void ShowCopyCodeNoti()
    {
        StartCoroutine(CopyCodeNoti());
    }
    private IEnumerator CopyCodeNoti()
    {
        copyCodeText.text = "COPIED!";

        yield return new WaitForSeconds(1f);

        copyCodeText.text = "COPY";
    }

    public void SetupLobbyUI()
    {
        bool isHost = NetworkManager.Singleton.IsHost;

        startGameButton.SetActive(isHost);
        waitForHostText.gameObject.SetActive(!isHost);
    }

    public void SetPlayerName(int slot, string name)
    {
        if (!IsServer) return;

        switch (slot)
        {
            case 0: playerName1.Value = name; break;
            case 1: playerName2.Value = name; break;
            case 2: playerName3.Value = name; break;
            case 3: playerName4.Value = name; break;
        }
    }

}
