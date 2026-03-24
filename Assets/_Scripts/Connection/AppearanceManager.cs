using Unity.Netcode;
using UnityEngine;

public class AppearanceManager : MonoBehaviour
{
    [SerializeField] GameObject ChooseColorPanel;
    [SerializeField] GameObject ChooseHeadPanel;

    private void Start()
    {
        ChooseColorPanel.SetActive(false);
    }
    public void OnClickColor(int index)
    {
        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var appearance = player.GetComponent<PlayerAppearance>();

        appearance.ChangeColorServerRpc(index);
    }

    public void OnClickHead(int index)
    {
        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var appearance = player.GetComponent<PlayerAppearance>();

        appearance.ChangeHeadServerRpc(index);
    }

    public void ToggleChooseColorPanel()
    {
        if(ChooseColorPanel.activeSelf)
        {
            ChooseColorPanel.SetActive(false);
        }
        else if (!ChooseColorPanel.activeSelf)
        {
            ChooseColorPanel.SetActive(true);
        }
    }

    public void ToggleChooseHeadPanel()
    {
        if (ChooseHeadPanel.activeSelf)
        {
            ChooseHeadPanel.SetActive(false);
        }
        else if (!ChooseHeadPanel.activeSelf)
        {
            ChooseHeadPanel.SetActive(true);
        }
    }
}
