using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject loginCanvas;
    public GameObject mainMenuCanvas;

    [Header("Login & Register UI")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI feedbackText;
    
    [Header("Main Menu UI")]
    public TextMeshProUGUI currentUserText; // Text โชว์ชื่อผู้เล่นในหน้า MainMenu
    
    [Header("Change Username UI")]
    public GameObject changeUserPanel;
    public TMP_InputField changeUsernameInput;

    // ตัวแปรเก็บชื่อ Username ปัจจุบันของเครื่องนี้ (เรียกใช้จากสคริปต์อื่นได้ด้วย LoginManager.LocalUsername)
    public static string LocalUsername { get; private set; } = "Player";

    private void Start()
    {
        // บังคับให้ช่อง Password แสดงผลเป็นจุด (....)
        if (passwordInput != null)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
        }

        // เริ่มเกมมาให้โชว์หน้า Login ปิดหน้า MainMenu
        if (loginCanvas != null) loginCanvas.SetActive(true);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (changeUserPanel != null) changeUserPanel.SetActive(false);
    }

    public void OnClickRegister()
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            SetFeedback("Username and Password cannot be empty!", Color.red);
            return;
        }

        // เช็คว่าเคยมีในฐานข้อมูลเครื่องหรือยัง
        if (PlayerPrefs.HasKey("User_" + user))
        {
            SetFeedback("Username already exists!", Color.red);
            return;
        }

        // เซฟลงเครื่อง
        PlayerPrefs.SetString("User_" + user, pass);
        PlayerPrefs.Save();
        
        SetFeedback("Registered successfully! You can now login.", Color.green);
    }

    public void OnClickLogin()
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            SetFeedback("Username and Password cannot be empty!", Color.red);
            return;
        }

        if (!PlayerPrefs.HasKey("User_" + user))
        {
            SetFeedback("Username not found! Please register.", Color.red);
            return;
        }

        string savedPass = PlayerPrefs.GetString("User_" + user);
        if (savedPass == pass)
        {
            // Login ผ่าน!
            LocalUsername = user;
            SetFeedback("Login Success!", Color.green);
            
            // สลับหน้า Canvas
            loginCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);
            
            UpdateMenuUI();
        }
        else
        {
            SetFeedback("Incorrect password!", Color.red);
        }
    }

    private void SetFeedback(string msg, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = color;
        }
    }

    private void UpdateMenuUI()
    {
        if (currentUserText != null) currentUserText.text = LocalUsername;
    }

    // ----------- ระบบกดเปลี่ยนชื่อ (ในหน้า MainMenu) -----------

    public void ToggleChangeUsernamePanel()
    {
        if (changeUserPanel != null) 
        {
            changeUserPanel.SetActive(!changeUserPanel.activeSelf);
        }
    }

    public void OnConfirmChangeUsername()
    {
        string newName = changeUsernameInput.text.Trim();
        if (string.IsNullOrEmpty(newName)) return;

        // เช็คว่าชื่อใหม่ยังไม่มีคนใช้
        if (PlayerPrefs.HasKey("User_" + newName) && newName != LocalUsername)
        {
            // ถ้าชื่อซ้ำ แจ้งเตือนหรือเด้งออก
            Debug.LogWarning("Username already exists!");
            return;
        }

        // ย้ายรหัสผ่านไปยังชื่อใหม่
        string pass = PlayerPrefs.GetString("User_" + LocalUsername, "");
        if (!string.IsNullOrEmpty(pass))
        {
            PlayerPrefs.DeleteKey("User_" + LocalUsername);
            PlayerPrefs.SetString("User_" + newName, pass);
            PlayerPrefs.Save();
        }

        LocalUsername = newName;
        UpdateMenuUI();
        
        // ปิด Panel ด้วยการเรียก Toggle
        if (changeUserPanel != null) changeUserPanel.SetActive(false);
    }
}
