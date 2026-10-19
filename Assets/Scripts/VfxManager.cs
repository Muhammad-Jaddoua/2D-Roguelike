using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VfxType
{
    WallDestroyed,
    FoodCollected,
    EnemyDeath
}

/// <summary>
/// Plays particle effects at a world position. Particle systems are pooled (created once,
/// reused with Play()/Stop()) and live on their own objects, so they keep playing even
/// after the cell object that triggered them has been deactivated by the object pool.
/// </summary>
public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance { get; private set; }

    [Header("Particle prefabs")]
    [SerializeField] private ParticleSystem m_WallDestroyedPrefab;
    [SerializeField] private ParticleSystem m_FoodCollectedPrefab;
    [SerializeField] private ParticleSystem m_EnemyDeathPrefab;

    [SerializeField, Range(1, 10), Tooltip("Particle systems pre-created per effect")]
    private int m_PoolSizePerEffect = 3;

    private ParticleSystem[] m_Prefabs;
    private Queue<ParticleSystem>[] m_Pools;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Index of each array matches the VfxType enum order
        m_Prefabs = new ParticleSystem[] { m_WallDestroyedPrefab, m_FoodCollectedPrefab, m_EnemyDeathPrefab };
        m_Pools = new Queue<ParticleSystem>[m_Prefabs.Length];

        for (int i = 0; i < m_Prefabs.Length; ++i)
        {
            m_Pools[i] = new Queue<ParticleSystem>();

            if (m_Prefabs[i] == null)
            {
                Debug.LogWarning("VfxManager: particle prefab #" + i + " is not assigned.");
                continue;
            }

            for (int n = 0; n < m_PoolSizePerEffect; ++n)
            {
                m_Pools[i].Enqueue(CreateInstance(m_Prefabs[i]));
            }
        }
    }

    private ParticleSystem CreateInstance(ParticleSystem prefab)
    {
        ParticleSystem instance = Instantiate(prefab, transform);
        instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        instance.gameObject.SetActive(false);
        return instance;
    }

    public static void Play(VfxType type, Vector3 position)
    {
        if (Instance != null)
        {
            Instance.PlayEffect(type, position);
        }
    }

    private void PlayEffect(VfxType type, Vector3 position)
    {
        int index = (int)type;
        if (m_Prefabs[index] == null)
        {
            return;
        }

        Queue<ParticleSystem> pool = m_Pools[index];
        ParticleSystem effect = pool.Count > 0 ? pool.Dequeue() : CreateInstance(m_Prefabs[index]);

        effect.transform.position = position;
        effect.gameObject.SetActive(true);
        effect.Play();

        ParticleSystem.MainModule main = effect.main;
        float lifetime = main.duration + main.startLifetime.constantMax + 0.1f;
        StartCoroutine(StopAndRecycle(effect, pool, lifetime));
    }

    private IEnumerator StopAndRecycle(ParticleSystem effect, Queue<ParticleSystem> pool, float delay)
    {
        yield return new WaitForSeconds(delay);

        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.gameObject.SetActive(false);
        pool.Enqueue(effect);
    }
}
