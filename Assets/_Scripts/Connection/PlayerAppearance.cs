using Unity.Netcode;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    [Header("Body")]
    public Renderer bodyRenderer;

    [Header("Head")]
    public GameObject[] heads; // Head1, Head2, Head3
    public Renderer[] headRenderers; // renderer ของแต่ละหัว

    [Header("Colors")]
    public Color[] availableColors;

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

        ApplyAll();
    }

    void ApplyAll()
    {
        ApplyHead(headIndex.Value);
        ApplyColor(colorIndex.Value);
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

        // ?? body (ทุก material)
        var mats = bodyRenderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].color = c;
        }

        // ?? head
        foreach (var r in headRenderers)
        {
            var headMats = r.materials;
            for (int i = 0; i < headMats.Length; i++)
            {
                headMats[i].color = c;
            }
        }
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