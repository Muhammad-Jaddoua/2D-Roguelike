using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple object pool for CellObjects (food, walls, enemies, exit).
/// Instances are pre-created at game start and kept inactive; Get() activates one and
/// Release() deactivates it again, so levels never need Instantiate/Destroy.
/// </summary>
public class CellObjectPool<T> : ICellObjectPool where T : CellObject
{
    private readonly T m_Prefab;
    private readonly Transform m_Holder;
    private readonly Queue<T> m_Available = new Queue<T>();
    private readonly string m_Name;

    public CellObjectPool(T prefab, Transform parent, string name, int initialSize)
    {
        m_Prefab = prefab;
        m_Name = name;

        // Pooled objects live under an empty holder so the Hierarchy stays tidy.
        m_Holder = new GameObject(name).transform;
        m_Holder.SetParent(parent, false);

        for (int i = 0; i < initialSize; ++i)
        {
            m_Available.Enqueue(CreateInstance());
        }
    }

    private T CreateInstance()
    {
        T instance = Object.Instantiate(m_Prefab, m_Holder);
        instance.OwnerPool = this;
        instance.gameObject.SetActive(false);
        return instance;
    }

    public T Get()
    {
        T obj;

        if (m_Available.Count > 0)
        {
            obj = m_Available.Dequeue();
        }
        else
        {
            // Should not happen when the pool sizes are tuned correctly, but never crash.
            Debug.LogWarning("Pool '" + m_Name + "' ran empty - creating an extra instance. Increase its size.");
            obj = CreateInstance();
        }

        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Release(CellObject obj)
    {
        // Guard against releasing the same object twice (it would be queued twice).
        if (!obj.gameObject.activeSelf)
        {
            return;
        }

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(m_Holder, false);
        m_Available.Enqueue((T)obj);
    }
}
