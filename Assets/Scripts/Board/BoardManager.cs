using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization; // NECESARIO PARA RECUPERAR REFERENCIAS
using UnityEngine.Tilemaps;

// Gestor del tablero de juego
public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        [SerializeField] private bool m_Passable;
        [SerializeField] private CellObject m_ContainedObject;

        public bool Passable { get => m_Passable; set => m_Passable = value; }
        public CellObject ContainedObject { get => m_ContainedObject; set => m_ContainedObject = value; }
    }

    [Header("Prefabs de Objetos")]
    [FormerlySerializedAs("ExitCellPrefab")][SerializeField] private ExitCellObject m_ExitCellPrefab;
    [FormerlySerializedAs("FoodPrefabs")][SerializeField] private FoodObject[] m_FoodPrefabs;
    [FormerlySerializedAs("GemPrefabs")][SerializeField] private XPGemObject[] m_GemPrefabs;
    [FormerlySerializedAs("ChestPrefab")][SerializeField] private ChestObject m_ChestPrefab;
    [FormerlySerializedAs("WallPrefab")][SerializeField] private WallObject m_WallPrefab;
    [FormerlySerializedAs("EnemyPrefabs")][SerializeField] private EnemyObject[] m_EnemyPrefabs;

    [Header("Dimensiones del Tablero")]
    [FormerlySerializedAs("Width")][SerializeField] private int m_Width = 8;
    [FormerlySerializedAs("Height")][SerializeField] private int m_Height = 8;

    [Header("Tiles")]
    [FormerlySerializedAs("GroundTiles")][SerializeField] private Tile[] m_GroundTiles;
    [FormerlySerializedAs("WallTiles")][SerializeField] private Tile[] m_WallTiles;

    [Header("Densidad Objetos")]
    [FormerlySerializedAs("EnemyDensity")][Range(0, 15)][SerializeField] private int m_EnemyDensity = 2;
    [FormerlySerializedAs("WallDensity")][Range(0, 15)][SerializeField] private int m_WallDensity = 3;
    [FormerlySerializedAs("FoodDensity")][Range(0, 15)][SerializeField] private int m_FoodDensity = 2;
    [FormerlySerializedAs("GemDensity")][Range(0, 15)][SerializeField] private int m_GemDensity = 3;
    [FormerlySerializedAs("ChestDensity")][Range(0, 15)][SerializeField] private int m_ChestDensity = 1;

    // PROPIEDADES PÚBLICAS
    public ExitCellObject ExitCellPrefab => m_ExitCellPrefab;
    public FoodObject[] FoodPrefabs => m_FoodPrefabs;
    public XPGemObject[] GemPrefabs => m_GemPrefabs;
    public ChestObject ChestPrefab => m_ChestPrefab;
    public WallObject WallPrefab => m_WallPrefab;
    public EnemyObject[] EnemyPrefabs => m_EnemyPrefabs;
    public int Width { get => m_Width; set => m_Width = value; }
    public int Height { get => m_Height; set => m_Height = value; }
    public Tile[] GroundTiles => m_GroundTiles;
    public Tile[] WallTiles => m_WallTiles;
    public int EnemyDensity => m_EnemyDensity;
    public int WallDensity => m_WallDensity;
    public int FoodDensity => m_FoodDensity;
    public int GemDensity => m_GemDensity;
    public int ChestDensity => m_ChestDensity;

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = m_WallTiles[Random.Range(0, m_WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = m_GroundTiles[Random.Range(0, m_GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        m_EmptyCellsList.Remove(new Vector2Int(1, 1));
        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(m_ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GenerateWall();
        GenerateChests();
        GenerateEnemies();
        GenerateGems();
        GenerateFood();
    }

    void GenerateGems()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int gemCount = (totalPassableCells * m_GemDensity) / 27;
        if (gemCount > m_EmptyCellsList.Count) gemCount = m_EmptyCellsList.Count;
        for (int i = 0; i < gemCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);
            XPGemObject prefabToSpawn = m_GemPrefabs[Random.Range(0, m_GemPrefabs.Length)];
            AddObject(Instantiate(prefabToSpawn), coord);
        }
    }

    void GenerateChests()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int chestCount = (totalPassableCells * m_ChestDensity) / 150;
        if (chestCount > m_EmptyCellsList.Count) chestCount = m_EmptyCellsList.Count;
        for (int i = 0; i < chestCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);
            ChestObject newChest = Instantiate(m_ChestPrefab);
            AddObject(newChest, coord);
        }
    }

    void GenerateFood()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int foodCount = (totalPassableCells * m_FoodDensity) / 27;
        if (foodCount > m_EmptyCellsList.Count) foodCount = m_EmptyCellsList.Count;
        for (int i = 0; i < foodCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);
            FoodObject prefabToSpawn = m_FoodPrefabs[Random.Range(0, m_FoodPrefabs.Length)];
            AddObject(Instantiate(prefabToSpawn), coord);
        }
    }

    void GenerateWall()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int wallCount = (totalPassableCells * m_WallDensity) / 27;
        if (wallCount > m_EmptyCellsList.Count) wallCount = m_EmptyCellsList.Count;
        for (int i = 0; i < wallCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);
            AddObject(Instantiate(m_WallPrefab), coord);
        }
    }

    void GenerateEnemies()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int enemyCount = (totalPassableCells * m_EnemyDensity) / 20;
        if (enemyCount > m_EmptyCellsList.Count) enemyCount = m_EmptyCellsList.Count;
        for (int i = 0; i < enemyCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);
            EnemyObject prefabToSpawn = m_EnemyPrefabs[Random.Range(0, m_EnemyPrefabs.Length)];
            AddObject(Instantiate(prefabToSpawn), coord);
        }
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
            return null;
        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void Clean()
    {
        if (m_BoardData == null) return;
        for (int y = 0; y < m_BoardData.GetLength(1); ++y)
        {
            for (int x = 0; x < m_BoardData.GetLength(0); ++x)
            {
                var cellData = m_BoardData[x, y];
                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }
                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }
}