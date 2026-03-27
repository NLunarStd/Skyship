using Unity.Netcode;
using TMPro;
using UnityEngine;
using Unity.Collections;

public class PlayerName : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> playerName =
        new NetworkVariable<FixedString32Bytes>();

    [SerializeField] private TMP_Text nameText;

    private void OnEnable()
    {
        playerName.OnValueChanged += OnNameChanged;
    }

    private void OnDisable()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }

    public override void OnNetworkSpawn()
    {
        // อัพเดทครั้งแรก
        UpdateName(playerName.Value.ToString());

        if (IsOwner)
        {
            // ส่งชื่อขึ้น server
            SetNameServerRpc(ConnectionManager.Instance.LocalUsername);
        }
    }

    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        UpdateName(newName.ToString());
    }

    private void UpdateName(string name)
    {
        nameText.text = name;
    }

    [ServerRpc]
    private void SetNameServerRpc(string name)
    {
        playerName.Value = name;
    }
}