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

            // Prevent held items from pushing the ship or characters
            int heldItemLayer = LayerMask.NameToLayer("HeldItem");
            int shipLayer = LayerMask.NameToLayer("Ship");
            int charLayer = LayerMask.NameToLayer("Character");

            if (heldItemLayer != -1)
            {
                if (shipLayer != -1) Physics.IgnoreLayerCollision(heldItemLayer, shipLayer, true);
                if (charLayer != -1) Physics.IgnoreLayerCollision(heldItemLayer, charLayer, true);
            }
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
    public AudioClip startSound;

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

    [Header("End Game UI")]
    public GameObject endGamePanel;
    public TMP_Text[] rankNames;
    public TMP_Text[] rankScores;
    public TMP_Text podium1stName;
    public TMP_Text podium2ndName;
    public TMP_Text podium3rdName;
    public Button returnLobbyButton;

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

    public void ExitLobby()
    {
        MenuCanvas.SetActive(true);
        LobbyCanvas.SetActive(false);
        GamePlayCanvas.SetActive(false);
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
            
            if (SFXManager.Instance != null)
            {
                if (step == "Start!") 
                {
                    if (startSound != null) SFXManager.Instance.PlaySFX2D(startSound);
                }
            }

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
        if (!IsServer) return;

        Debug.Log("Match End");

        ShowEndGameClientRpc(
            scoreP1.Value,
            scoreP2.Value,
            scoreP3.Value,
            scoreP4.Value,
            playerName1.Value.ToString(),
            playerName2.Value.ToString(),
            playerName3.Value.ToString(),
            playerName4.Value.ToString()
        );
    }

    [ClientRpc]
    private void ShowEndGameClientRpc(
    int s1, int s2, int s3, int s4,
    string n1, string n2, string n3, string n4)
    {
        SetCursorStateClientRpc(false);
        endGamePanel.SetActive(true);

        // Sort players by score descending
        var players = new System.Collections.Generic.List<(string name, int score)>
        {
            (n1, s1),
            (n2, s2),
            (n3, s3),
            (n4, s4)
        };

        players.Sort((a, b) => b.score.CompareTo(a.score));

        // Assign to Leaderboard UI
        if (rankNames != null && rankScores != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < rankNames.Length && rankNames[i] != null)
                    rankNames[i].text = players[i].name;

                if (i < rankScores.Length && rankScores[i] != null)
                    rankScores[i].text = players[i].score.ToString();
            }
        }

        // Assign to Podium UI
        if (podium1stName != null) podium1stName.text = players[0].name;
        if (podium2ndName != null) podium2ndName.text = players[1].name;
        if (podium3rdName != null) podium3rdName.text = players[2].name;

        // ปุ่ม host เท่านั้น
        bool isHost = NetworkManager.Singleton.IsHost;
        returnLobbyButton.gameObject.SetActive(isHost);
    }

    public void OnClickReturnToLobby()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        ReturnToLobbyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReturnToLobbyServerRpc()
    {
        ResetGame();
        StartCoroutine(TeleportAllPlayersToLobby());
        ReturnToLobbyClientRpc();
    }

    private void ResetGame()
    {
        foreach (var spawner in FindObjectsOfType<ItemSpawner>()) { spawner.StopSpawning(); }
        foreach (var pooledItem in FindObjectsOfType<PooledItem>()) { if (pooledItem.gameObject.activeInHierarchy) pooledItem.ReturnToPool(); }
        foreach (var ship in FindObjectsOfType<NetworkShipHandler>()) 
        { 
            ship.ResetShip(); 
            if (IsServer) ship.shipColor.Value = Color.white;
        }
        foreach (var boost in FindObjectsOfType<BoostPickup>()) { boost.ResetBoost(); }
        foreach (var pBoost in FindObjectsOfType<PlayerBoost>()) { pBoost.ResetAllBoosts(); }

        matchTime = 300f;
        isGameStarted = false;

        scoreP1.Value = 0;
        scoreP2.Value = 0;
        scoreP3.Value = 0;
        scoreP4.Value = 0;
    }

    [ClientRpc]
    private void ReturnToLobbyClientRpc()
    {
        endGamePanel.SetActive(false);

        mainCam.gameObject.SetActive(false);
        lobbyCam.gameObject.SetActive(true);

        LobbyCanvas.SetActive(true);
        GamePlayCanvas.SetActive(false);

        isInLobby = true;

        foreach (var steering in FindObjectsOfType<SteeringInteract>())
        {
            steering.ExitShipControl();
        }
    }

    [SerializeField] private Transform[] lobbySpawnPoints;

    private IEnumerator TeleportAllPlayersToLobby()
    {
        yield return new WaitForSeconds(0.2f);

        if (!IsServer) yield break;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            int slot = ConnectionManager.Instance.GetPlayerSlot(client.ClientId);
            Vector3 pos = lobbySpawnPoints[slot].position;
            pos.y += 2f;

            TeleportClientRpc(pos, client.ClientId);
        }
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
        SetCursorStateClientRpc(true);
        StartCountdownClientRpc();


    }

    private IEnumerator TeleportAllPlayers()
    {
        // รอให้ทุก player spawn
        yield return new WaitUntil(() =>
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null) return false;
            }
            return true;
        });

        yield return new WaitForSeconds(0.1f); // ป้องกัน desync

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            int slot = ConnectionManager.Instance.GetPlayerSlot(client.ClientId);
            Vector3 targetPos = GetGameSpawnPosition(slot);

            TeleportClientRpc(targetPos, client.ClientId);

            // Sync ship color
            if (IsServer && playerShips != null && slot >= 0 && slot < playerShips.Length && playerShips[slot] != null)
            {
                var appearance = client.PlayerObject.GetComponent<PlayerAppearance>();
                if (appearance != null && appearance.availableColors.Length > appearance.colorIndex.Value)
                {
                    playerShips[slot].shipColor.Value = appearance.availableColors[appearance.colorIndex.Value];
                }
            }
        }
    }
   

    [ClientRpc]
    public void TeleportClientRpc(Vector3 targetPos, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

        var movement = player.GetComponent<NetworkPlayerMovement>();
        if (movement != null)
        {
            movement.StartCoroutine(movement.TeleportRoutine(targetPos));
        }
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
        isInLobby = false;

        mainCam.gameObject.SetActive(true);
        lobbyCam.gameObject.SetActive(false);

        LobbyCanvas.SetActive(false);
        GamePlayCanvas.SetActive(true);
    }

    [SerializeField] private Transform[] gameSpawnPoints;
    [SerializeField] private NetworkShipHandler[] playerShips;

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

    [ClientRpc]
    private void SetCursorStateClientRpc(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

