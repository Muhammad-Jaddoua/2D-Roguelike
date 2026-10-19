using UnityEngine;

/// <summary>
/// Anything that can be pooled implements this so a CellObject can hand itself back
/// without knowing which concrete pool created it.
/// </summary>
public interface ICellObjectPool
{
    void Release(CellObject obj);
}

public class CellObject : MonoBehaviour
{
    protected Vector2Int m_Cell;

    /// <summary>Set by the pool that created this object (null if it was not pooled).</summary>
    public ICellObjectPool OwnerPool { get; set; }

    public virtual void Init(Vector2Int cell)
    {
        m_Cell = cell;
    }

    public virtual void PlayerEntered()
    {

    }

    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }

    /// <summary>
    /// Removes this object from its board cell and returns it to its pool.
    /// This replaces Destroy(gameObject): the object is only deactivated (SetActive(false)).
    /// </summary>
    protected void Despawn()
    {
        GameManager.Instance.BoardManager.ReleaseObject(this, m_Cell);
    }
}
