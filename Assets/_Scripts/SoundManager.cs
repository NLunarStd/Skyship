using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    [SerializeField] AudioSource audioSource;

    [Header("Clip")]
    [SerializeField] AudioClip pickupSound;
    [SerializeField] AudioClip throwSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip dropSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayPickupSound()
    {
        audioSource.PlayOneShot(pickupSound);
    }

    public void PlayThrowSound()
    {
        audioSource.PlayOneShot(throwSound);
    }

    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(jumpSound);
    }

    public void PlayDropSound()
    {
        audioSource.PlayOneShot(dropSound);
    }

}
