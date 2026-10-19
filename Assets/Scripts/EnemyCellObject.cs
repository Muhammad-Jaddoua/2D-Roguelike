using UnityEngine;
using UnityEngine.Serialization;

public class EnemyCellObject : CellObject
{
    [SerializeField, Range(1, 10), FormerlySerializedAs("Health"), Tooltip("Hits needed to kill this enemy")]
    private int m_MaxHealth = 3;

    [SerializeField, Range(1, 10), Tooltip("Food the player loses when this enemy attacks")]
    private int m_Damage = 3;

    private int m_CurrentHealth;

    // Enemies are pooled, so they are re-activated instead of re-created:
    // they subscribe to the turn event in Init() (every time they are placed on a board)
    // and unsubscribe as soon as they are deactivated (OnDisable) or destroyed (OnDestroy).
    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        TurnManager turnManager = GameManager.Instance.TurnManager;
        turnManager.OnTick -= TurnHappened; // never subscribe twice
        turnManager.OnTick += TurnHappened;
    }

    private void Unsubscribe()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        }
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = m_MaxHealth; // pooled objects must be reset every time they are reused
        Subscribe();
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;

        if (m_CurrentHealth <= 0)
        {
            SoundManager.PlayEnemyDeath();
            VfxManager.Play(VfxType.EnemyDeath, transform.position);
            Despawn();
        }
        else
        {
            SoundManager.PlayWallAttack();
        }

        return false;
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }

        //remove enemy from current cell
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        //add it to the next cell
        targetCell.ContainedObject = this;
        m_Cell = coord;
        transform.position = board.CellToWorld(coord);

        return true;
    }

    void TurnHappened()
    {
        //We added a public property that return the player current cell!
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1)
            || (yDist == 0 && absXDist == 1))
        {
            //we are adjacent to the player, attack!
            SoundManager.PlayEnemyAttack();
            GameManager.Instance.ChangeFood(-m_Damage);
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    //if our move was not successful (so no move and not attack)
                    //we try to move along Y
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    bool TryMoveInX(int xDist)
    {
        //try to get closer in x

        //player to our right
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }

        //player to our left
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        //try to get closer in y

        //player on top
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }

        //player below
        return MoveTo(m_Cell + Vector2Int.down);
    }
}
