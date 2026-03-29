using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    private GameObject speedPanel;
    private GameObject jumpPanel;
    private Image speedImage;
    private Image jumpImage;

    private void Awake()
    {
        EventManager.Instance.OnSpeedBoostPickup += OnSpeedPick;
        EventManager.Instance.OnJumpBoostPickup += OnJumpPick;
        EventManager.Instance.OnSpeedBoostActive += OnSpeedUse;
        EventManager.Instance.OnJumpBoostActive += OnJumpUse;
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnSpeedBoostPickup -= OnSpeedPick;
        EventManager.Instance.OnJumpBoostPickup -= OnJumpPick;
        EventManager.Instance.OnSpeedBoostActive -= OnSpeedUse;
        EventManager.Instance.OnJumpBoostActive -= OnJumpUse;
    }
    void Start()
    {
        speedPanel = transform.GetChild(4).gameObject;
        jumpPanel = transform.GetChild(5).gameObject;
        
        speedImage = speedPanel.transform.GetChild(0).gameObject.GetComponent<Image>();
        
        jumpImage = jumpPanel.transform.GetChild(0).gameObject.GetComponent<Image>();
        
        Debug.Log(speedImage.name + " name of speed | name of jump " + jumpImage.name);
    }

    private void OnSpeedUse()
    {
        Debug.Log("OnSpeedUse");
        speedImage.enabled = false;
    }

    private void OnJumpUse()
    {
        Debug.Log("OnJumpUse");
        jumpImage.enabled = false;
    }

    private void OnSpeedPick()
    {
        Debug.Log("OnSpeedPick");
        speedImage.enabled = true;
        Debug.Log(speedImage.name + speedImage.enabled);
    }

    private void OnJumpPick()
    {
        Debug.Log("OnJumpPick");
        jumpImage.enabled = true;
        Debug.Log(jumpImage.name + jumpImage.enabled);
    }
    
}
