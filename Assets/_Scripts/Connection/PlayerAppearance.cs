using Unity.Netcode;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    [Header("Type 1")]
    public GameObject type1Root;
    public Renderer bodyRenderer;
    public GameObject[] heads;
    public Renderer[] headRenderers;

    [Header("Type 2")]
    public GameObject type2Root;
    public GameObject[] headsType2;

    [Header("Colors")]
    public Color[] availableColors;

    public NetworkVariable<int> typeIndex = new NetworkVariable<int>(0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    public NetworkVariable<int> colorIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> headIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        colorIndex.OnValueChanged += (o, n) => ApplyAll();
        headIndex.OnValueChanged += (o, n) => ApplyAll();
        typeIndex.OnValueChanged += (o, n) => ApplyAll();

        ApplyAll();
    }

    void ApplyAll()
    {
        ApplyType(typeIndex.Value);

        if (typeIndex.Value == 0) // Type 1
        {
            ApplyHead(headIndex.Value);
            ApplyColor(colorIndex.Value);
        }
        else // Type 2
        {
            ApplyHeadType2(headIndex.Value);
        }
    }

    void ApplyType(int type)
    {
        type1Root.SetActive(type == 0);
        type2Root.SetActive(type == 1);
    }

    void ApplyHead(int index)
    {
        for (int i = 0; i < heads.Length; i++)
        {
            heads[i].SetActive(i == index);
        }
    }

    void ApplyColor(int index)
    {
        Color c = availableColors[index];

        // body (ทุก material)
        var mats = bodyRenderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].color = c;
        }

        // head
        foreach (var r in headRenderers)
        {
            var headMats = r.materials;
            for (int i = 0; i < headMats.Length; i++)
            {
                headMats[i].color = c;
            }
        }
    }

    void ApplyHeadType2(int index)
    {
        for (int i = 0; i < headsType2.Length; i++)
        {
            headsType2[i].SetActive(i == index);
        }
    }

    [ServerRpc]
    public void ChangeTypeServerRpc(int index)
    {
        typeIndex.Value = index;

        // reset head index กัน out of range
        headIndex.Value = 0;
    }

    [ServerRpc]
    public void ChangeColorServerRpc(int index)
    {
        colorIndex.Value = index;
    }

    [ServerRpc]
    public void ChangeHeadServerRpc(int index)
    {
        headIndex.Value = index;
    }
}