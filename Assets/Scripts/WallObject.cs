using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    [SerializeField, FormerlySerializedAs("ObstacleTile")] private Tile m_ObstacleTile;

    [SerializeField, Range(1, 10), FormerlySerializedAs("MaxHealth"), Tooltip("Hits needed to destroy this wall")]
    private int m_MaxHealth = 3;

    private int m_HealthPoint;
    private Tile m_OriginalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        // Pooled walls are reused, so health and original tile must be reset on every Init
        m_HealthPoint = m_MaxHealth;

        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, m_ObstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        m_HealthPoint -= 1;
        SoundManager.PlayWallAttack();

        if (m_HealthPoint > 0)
        {
            return false;
        }

        GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
        VfxManager.Play(VfxType.WallDestroyed, transform.position);
        Despawn();
        return true;
    }
}
