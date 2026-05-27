using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource สำหรับเสียง 2D (ไม่มีทิศทาง เช่น UI, เสียงเข้าตัวเราเอง)")]
    [SerializeField] private AudioSource sfxSource2D;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ทำให้ข้ามซีนได้
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// เล่นเสียงแบบ 2D (ได้ยินชัดเจนเท่ากันหมด ไม่สนใจระยะทาง)
    /// เหมาะกับ: UI, เสียงคนเล่นกดปุ่ม, เสียงกระโดดของตัวเอง (Local)
    /// </summary>
    public void PlaySFX2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource2D == null) return;
        sfxSource2D.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// เล่นเสียงแบบ 3D (ได้ยินตามตำแหน่ง อิงระยะห่างจากกล้อง)
    /// เหมาะกับ: เสียงระเบิด, เสียงปืนคนอื่น, เสียงกระโดดของเพื่อน
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        
        // PlayClipAtPoint จะสร้าง AudioSource ชั่วคราวขึ้นมาเล่นเสียงแล้วทำลายทิ้งเอง
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
