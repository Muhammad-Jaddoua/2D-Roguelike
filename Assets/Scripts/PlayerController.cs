using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [Header("Movement")]
    [SerializeField, Range(0.05f, 0.6f), Tooltip("Seconds the player takes to glide from one cell to the next")]
    private float m_MoveDuration = 0.2f;

    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;
    private Animator m_Animator;
    private bool m_HasAttackParameter;

    // True while the movement coroutine is running; input is ignored during that time
    private bool m_IsMoving;
    private Coroutine m_MoveRoutine;

    public Vector2Int Cell => m_CellPosition;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();

        // The Attack trigger is created by the Section 7 setup tool; don't spam warnings if it's missing.
        foreach (AnimatorControllerParameter parameter in m_Animator.parameters)
        {
            if (parameter.name == "Attack" && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                m_HasAttackParameter = true;
            }
        }
    }

    public void MoveTo(Vector2Int cell, bool immediate)
    {
        m_CellPosition = cell;

        // Never let an old movement coroutine keep running (e.g. when a new level/game starts mid-move)
        if (m_MoveRoutine != null)
        {
            StopCoroutine(m_MoveRoutine);
            m_MoveRoutine = null;
        }

        if (immediate)
        {
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
            m_Animator.SetBool(MovingHash, false);
        }
        else
        {
            m_MoveRoutine = StartCoroutine(MoveRoutine(m_Board.CellToWorld(m_CellPosition)));
        }
    }

    /// <summary>Smoothly lerps the player from where it is now to the target position.</summary>
    private IEnumerator MoveRoutine(Vector3 target)
    {
        m_IsMoving = true;
        m_Animator.SetBool(MovingHash, true);
        SoundManager.PlayMove();

        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < m_MoveDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, Mathf.Clamp01(elapsed / m_MoveDuration));
            yield return null; // continue next frame
        }

        // Lerp never guarantees landing exactly on the target, so snap to it.
        transform.position = target;
        m_IsMoving = false;
        m_MoveRoutine = null;
        m_Animator.SetBool(MovingHash, false);

        // PlayerEntered is called once the player has arrived in the cell
        var cellData = m_Board.GetCellData(m_CellPosition);
        if (cellData != null && cellData.ContainedObject != null)
        {
            cellData.ContainedObject.PlayerEntered();
        }
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell, true);
    }

    private void PlayAttackAnimation()
    {
        if (m_HasAttackParameter)
        {
            m_Animator.SetTrigger(AttackHash);
        }
    }

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }

        // Input is blocked while the player is gliding between cells, so cells can't be skipped
        if (m_IsMoving)
        {
            return;
        }

        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
        }

        if (hasMoved)
        {
            //check if the new position is passable, then move there if it is.
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

            if (cellData != null && cellData.Passable)
            {
                GameManager.Instance.TurnManager.Tick();

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget, false);
                }
                else if (cellData.ContainedObject.PlayerWantsToEnter())
                {
                    //PlayerEntered is called once the player arrives (end of MoveRoutine)
                    MoveTo(newCellTarget, false);
                }
                else
                {
                    // Bumped into a wall or an enemy
                    PlayAttackAnimation();
                }
            }
        }
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public void Init()
    {
        m_IsGameOver = false;
    }
}
