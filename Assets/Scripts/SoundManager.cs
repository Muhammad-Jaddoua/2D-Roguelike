using UnityEngine;

/// <summary>
/// Background music + sound effects. Music plays from an AudioSource on the Main Camera
/// (looping, reduced volume); every sound effect is played with PlayOneShot().
/// Other scripts call the static helpers, e.g. SoundManager.PlayFoodPickup().
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField, Tooltip("The AudioSource on the Main Camera that plays the background music")]
    private AudioSource m_MusicSource;
    [SerializeField, Range(0f, 1f), Tooltip("Music volume (kept low so it doesn't drown the effects)")]
    private float m_MusicVolume = 0.3f;

    [Header("Sound effects")]
    [SerializeField, Tooltip("AudioSource used for PlayOneShot(). Created automatically if left empty")]
    private AudioSource m_SfxSource;
    [SerializeField, Range(0f, 1f)] private float m_SfxVolume = 1f;
    [SerializeField] private AudioClip[] m_MoveClips;
    [SerializeField] private AudioClip[] m_WallAttackClips;
    [SerializeField] private AudioClip[] m_FoodClips;
    [SerializeField] private AudioClip[] m_EnemyAttackClips;
    [SerializeField] private AudioClip[] m_EnemyDeathClips;
    [SerializeField] private AudioClip m_GameOverClip;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (m_SfxSource == null)
        {
            m_SfxSource = gameObject.AddComponent<AudioSource>();
            m_SfxSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        if (m_MusicSource != null)
        {
            m_MusicSource.loop = true;
            m_MusicSource.volume = m_MusicVolume;
            if (!m_MusicSource.isPlaying)
            {
                m_MusicSource.Play();
            }
        }
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        PlayClip(clips[Random.Range(0, clips.Length)]);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            m_SfxSource.PlayOneShot(clip, m_SfxVolume);
        }
    }

    // Static helpers so any script can play a sound without holding a reference.
    public static void PlayMove() { if (Instance != null) Instance.PlayRandom(Instance.m_MoveClips); }
    public static void PlayWallAttack() { if (Instance != null) Instance.PlayRandom(Instance.m_WallAttackClips); }
    public static void PlayFoodPickup() { if (Instance != null) Instance.PlayRandom(Instance.m_FoodClips); }
    public static void PlayEnemyAttack() { if (Instance != null) Instance.PlayRandom(Instance.m_EnemyAttackClips); }
    public static void PlayEnemyDeath() { if (Instance != null) Instance.PlayRandom(Instance.m_EnemyDeathClips); }
    public static void PlayGameOver() { if (Instance != null) Instance.PlayClip(Instance.m_GameOverClip); }
}
