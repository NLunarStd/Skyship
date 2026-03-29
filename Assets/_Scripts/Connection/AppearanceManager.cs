using Unity.Netcode;
using UnityEngine;

public class AppearanceManager : MonoBehaviour
{
    [SerializeField] GameObject ChooseTypePanel;
    [SerializeField] GameObject ChooseColorPanel;
    [SerializeField] GameObject ChooseHeadPanel;
    [SerializeField] GameObject ChooseHeadPanel_Type2;

    [SerializeField] GameObject Head_Type1_Button;
    [SerializeField] GameObject Head_Type2_Button;
    [SerializeField] GameObject Color_Button;

    private void Start()
    {
        ChooseColorPanel.SetActive(false);
        ChooseHeadPanel.SetActive(false);
        ChooseHeadPanel_Type2.SetActive(false);
        ChooseTypePanel.SetActive(false);

        Head_Type2_Button.SetActive(false);
    }

    public void OnClickType(int typeIndex)
    {
        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var appearance = player.GetComponent<PlayerAppearance>();

        appearance.ChangeTypeServerRpc(typeIndex);

        // ?? UI logic
        if (typeIndex == 0)
        {
            Head_Type1_Button.SetActive(true);
            Head_Type2_Button.SetActive(false);

            ChooseHeadPanel.SetActive(false);
            ChooseHeadPanel_Type2.SetActive(false);

            Color_Button.SetActive(true);
            ChooseColorPanel.SetActive(false);
        }
        else if(typeIndex == 1) 
        {
            Head_Type1_Button.SetActive(false);
            Head_Type2_Button.SetActive(true);

            ChooseHeadPanel.SetActive(false);
            ChooseHeadPanel_Type2.SetActive(false);

            Color_Button.SetActive(false);
            ChooseColorPanel.SetActive(false);
        }
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

    public void ToggleChooseType()
    {
        if (ChooseTypePanel.activeSelf)
        {
            ChooseTypePanel.SetActive(false);
        }
        else if (!ChooseTypePanel.activeSelf)
        {
            ChooseTypePanel.SetActive(true);
        }
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

    public void ToggleChooseHeadPanel_Type2()
    {
        if (ChooseHeadPanel_Type2.activeSelf)
        {
            ChooseHeadPanel_Type2.SetActive(false);
        }
        else if (!ChooseHeadPanel_Type2.activeSelf)
        {
            ChooseHeadPanel_Type2.SetActive(true);
        }
    }

    public void OnClickHeadType2(int index)
    {
        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var appearance = player.GetComponent<PlayerAppearance>();

        appearance.ChangeHeadServerRpc(index);
    }
}
