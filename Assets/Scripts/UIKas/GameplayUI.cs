using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [Header("Boost Icons")]
    [SerializeField] private Image speedImage;
    [SerializeField] private Image jumpImage;

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
        // ทำให้ไอคอนโชว์ตลอดเวลาแต่มืดลง (สว่างเฉพาะตอนเก็บ)
        if (speedImage != null)
        {
            speedImage.enabled = true;
            speedImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // มืด
        }

        if (jumpImage != null)
        {
            jumpImage.enabled = true;
            jumpImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // มืด
        }
    }

    private void OnSpeedUse()
    {
        Debug.Log("OnSpeedUse");
        if (speedImage != null) speedImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // กลับไปมืด
    }

    private void OnJumpUse()
    {
        Debug.Log("OnJumpUse");
        if (jumpImage != null) jumpImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // กลับไปมืด
    }

    private void OnSpeedPick()
    {
        Debug.Log("OnSpeedPick");
        if (speedImage != null) speedImage.color = Color.white; // สว่าง 100%
    }

    private void OnJumpPick()
    {
        Debug.Log("OnJumpPick");
        if (jumpImage != null) jumpImage.color = Color.white; // สว่าง 100%
    }
    
}
