using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    [Header("Board size (level 1) - grows with the level, see Scaling")]
    [SerializeField, Range(6, 20), Tooltip("Width of the board on level 1")]
    [FormerlySerializedAs("Width")]
    private int m_Width = 8;

    [SerializeField, Range(6, 20), Tooltip("Height of the board on level 1")]
    [FormerlySerializedAs("Height")]
    private int m_Height = 8;

    [Header("Tiles")]
    [SerializeField, FormerlySerializedAs("GroundTiles")] private Tile[] m_GroundTiles;
    [SerializeField, FormerlySerializedAs("WallTiles")] private Tile[] m_WallTiles;

    [Header("Prefabs (each one gets its own object pool)")]
    [SerializeField, Tooltip("Every food prefab here gets a pool; a random one is picked for each food item")]
    private FoodObject[] m_FoodPrefabs;
    [SerializeField, FormerlySerializedAs("WallPrefab")] private WallObject m_WallPrefab;
    [SerializeField, FormerlySerializedAs("ExitCellPrefab")] private ExitCellObject m_ExitCellPrefab;
    [SerializeField, FormerlySerializedAs("EnemyCellPrefab")] private EnemyCellObject m_EnemyCellPrefab;

    [Header("Scaling (bonus: Dynamic Board Scaling)")]
    [SerializeField, Range(6, 24), Tooltip("The board never grows beyond this many cells per side")]
    private int m_MaxBoardSize = 14;
    [SerializeField, Range(1, 10), Tooltip("The board grows by 1 cell (width and height) every N levels")]
    private int m_LevelsPerGrowth = 2;
    [SerializeField, Range(0f, 3f), Tooltip("Empty space (in cells) kept around the board when framing the camera")]
    private float m_CameraPadding = 1f;

    [Header("Walls")]
    [SerializeField, Range(0, 30), Tooltip("Minimum walls on level 1")] private int m_MinWalls = 6;
    [SerializeField, Range(1, 30), Tooltip("Maximum walls on level 1 (exclusive)")] private int m_MaxWalls = 10;
    [SerializeField, Range(1, 10), Tooltip("One extra wall every N levels")] private int m_LevelsPerExtraWall = 2;
    [SerializeField, Range(5, 60), Tooltip("Hard cap on walls per level. Also the wall pool size")] private int m_MaxWallsPerLevel = 30;

    [Header("Food")]
    [SerializeField, Range(1, 20), Tooltip("Food items on level 1")] private int m_BaseFood = 5;
    [SerializeField, Range(1, 10), Tooltip("One extra food item every N levels")] private int m_LevelsPerExtraFood = 3;
    [SerializeField, Range(1, 30), Tooltip("Hard cap on food per level. Also the size of each food pool")] private int m_MaxFood = 10;

    [Header("Enemies")]
    [SerializeField, Range(1, 10), Tooltip("One extra enemy every N levels (level 1 has one enemy)")] private int m_LevelsPerExtraEnemy = 3;
    [SerializeField, Range(1, 15), Tooltip("Hard cap on enemies per level. Also the enemy pool size")] private int m_MaxEnemies = 5;

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;
    private int m_CurrentWidth;
    private int m_CurrentHeight;

    // Object pools (created once, at game start)
    private bool m_PoolsReady;
    private CellObjectPool<FoodObject>[] m_FoodPools;
    private CellObjectPool<WallObject> m_WallPool;
    private CellObjectPool<EnemyCellObject> m_EnemyPool;
    private CellObjectPool<ExitCellObject> m_ExitPool;

    private static readonly Vector2Int PlayerStart = new Vector2Int(1, 1);

    public int CurrentWidth { get { return m_CurrentWidth; } }
    public int CurrentHeight { get { return m_CurrentHeight; } }

    /// <summary>Pre-instantiates every pool. Runs once, the first time the board is initialised.</summary>
    private void CreatePools()
    {
        m_PoolsReady = true;

        int foodPrefabCount = m_FoodPrefabs != null ? m_FoodPrefabs.Length : 0;
        m_FoodPools = new CellObjectPool<FoodObject>[foodPrefabCount];
        for (int i = 0; i < foodPrefabCount; ++i)
        {
            m_FoodPools[i] = new CellObjectPool<FoodObject>(m_FoodPrefabs[i], transform, "Pool_Food_" + i, m_MaxFood);
        }

        m_WallPool = new CellObjectPool<WallObject>(m_WallPrefab, transform, "Pool_Walls", m_MaxWallsPerLevel);
        m_EnemyPool = new CellObjectPool<EnemyCellObject>(m_EnemyCellPrefab, transform, "Pool_Enemies", m_MaxEnemies);
        m_ExitPool = new CellObjectPool<ExitCellObject>(m_ExitCellPrefab, transform, "Pool_Exit", 1);
    }

    public void Init(int level = 1)
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        if (m_GroundTiles == null || m_GroundTiles.Length == 0 || m_WallTiles == null || m_WallTiles.Length == 0)
        {
            Debug.LogError("BoardManager: Ground Tiles / Wall Tiles are not assigned in the Inspector.");
            return;
        }

        if (!m_PoolsReady)
        {
            CreatePools();
        }

        // Dynamic board scaling: the board gets bigger as the level goes up (until the max size).
        int growth = (level - 1) / m_LevelsPerGrowth;
        m_CurrentWidth = Mathf.Min(m_Width + growth, Mathf.Max(m_MaxBoardSize, m_Width));
        m_CurrentHeight = Mathf.Min(m_Height + growth, Mathf.Max(m_MaxBoardSize, m_Height));

        //Initialize the list
        m_EmptyCellsList = new List<Vector2Int>();

        m_BoardData = new CellData[m_CurrentWidth, m_CurrentHeight];

        for (int y = 0; y < m_CurrentHeight; ++y)
        {
            for (int x = 0; x < m_CurrentWidth; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == m_CurrentWidth - 1 || y == m_CurrentHeight - 1)
                {
                    tile = m_WallTiles[Random.Range(0, m_WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = m_GroundTiles[Random.Range(0, m_GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;

                    //this is a passable empty cell, add it to the list!
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        //remove the starting point of the player! It's not empty, the player is there
        m_EmptyCellsList.Remove(PlayerStart);

        Vector2Int endCoord = new Vector2Int(m_CurrentWidth - 2, m_CurrentHeight - 2);
        AddObject(m_ExitPool.Get(), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GenerateWall(level);
        GenerateFood(level);
        GenerateEnemy(level);

        FrameCamera();
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (m_BoardData == null
            || cellIndex.x < 0 || cellIndex.x >= m_CurrentWidth
            || cellIndex.y < 0 || cellIndex.y >= m_CurrentHeight)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    /// <summary>Picks (and removes) a random free cell. Returns false if the board is full.</summary>
    private bool TryTakeEmptyCell(out Vector2Int coord)
    {
        if (m_EmptyCellsList.Count == 0)
        {
            coord = Vector2Int.zero;
            return false;
        }

        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        coord = m_EmptyCellsList[randomIndex];
        m_EmptyCellsList.RemoveAt(randomIndex);
        return true;
    }

    private void GenerateWall(int level)
    {
        int wallCount = Random.Range(m_MinWalls, Mathf.Max(m_MaxWalls, m_MinWalls + 1)) + (level - 1) / m_LevelsPerExtraWall;
        wallCount = Mathf.Min(wallCount, m_MaxWallsPerLevel);

        for (int i = 0; i < wallCount; ++i)
        {
            Vector2Int coord;
            if (!TryTakeEmptyCell(out coord))
            {
                return;
            }

            AddObject(m_WallPool.Get(), coord);
        }
    }

    private void GenerateFood(int level)
    {
        if (m_FoodPools.Length == 0)
        {
            Debug.LogError("BoardManager: no Food Prefabs assigned.");
            return;
        }

        int foodCount = Mathf.Min(m_BaseFood + (level - 1) / m_LevelsPerExtraFood, m_MaxFood);

        for (int i = 0; i < foodCount; ++i)
        {
            Vector2Int coord;
            if (!TryTakeEmptyCell(out coord))
            {
                return;
            }

            CellObjectPool<FoodObject> pool = m_FoodPools[Random.Range(0, m_FoodPools.Length)];
            AddObject(pool.Get(), coord);
        }
    }

    private void GenerateEnemy(int level)
    {
        int enemyCount = Mathf.Min(1 + (level - 1) / m_LevelsPerExtraEnemy, m_MaxEnemies);

        for (int i = 0; i < enemyCount; ++i)
        {
            Vector2Int coord;
            if (!TryTakeEmptyCell(out coord))
            {
                return;
            }

            AddObject(m_EnemyPool.Get(), coord);
        }
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    private void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    /// <summary>
    /// Takes a CellObject off the board: the cell is explicitly emptied and the object goes back to
    /// its pool (SetActive(false)). Must be used instead of Destroy now that objects are pooled -
    /// a deactivated object is NOT "null", so the cell has to be cleared by hand.
    /// </summary>
    public void ReleaseObject(CellObject obj, Vector2Int cell)
    {
        CellData data = GetCellData(cell);
        if (data != null && data.ContainedObject == obj)
        {
            data.ContainedObject = null;
        }

        if (obj.OwnerPool != null)
        {
            obj.OwnerPool.Release(obj);
        }
        else
        {
            Destroy(obj.gameObject);
        }
    }

    public void Clean()
    {
        //no board data, so exit early, nothing to clean
        if (m_BoardData == null)
            return;

        // Use the size of the board that was actually generated (it changes with the level).
        int width = m_BoardData.GetLength(0);
        int height = m_BoardData.GetLength(1);

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    // Recycle (SetActive(false)) instead of Destroy
                    ReleaseObject(cellData.ContainedObject, new Vector2Int(x, y));
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }

    /// <summary>Centers the camera on the board and zooms so the whole (possibly larger) board fits.</summary>
    private void FrameCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 bottomLeft = CellToWorld(Vector2Int.zero);
        Vector3 topRight = CellToWorld(new Vector2Int(m_CurrentWidth - 1, m_CurrentHeight - 1));
        Vector3 center = (bottomLeft + topRight) * 0.5f;

        float halfHeight = m_CurrentHeight * 0.5f + m_CameraPadding;
        float halfWidth = m_CurrentWidth * 0.5f + m_CameraPadding;

        cam.orthographicSize = Mathf.Max(halfHeight, halfWidth / cam.aspect);
        cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
    }
}
