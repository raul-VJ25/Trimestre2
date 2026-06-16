using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Gestor del tablero de juego
// Crea y administra el mapa procedural con tiles, objetos y enemigos
public class BoardManager : MonoBehaviour
{
    // Clase interna para datos de cada celda
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    // Referencias de componentes
    public ExitCellObject ExitCellPrefab;
    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    // Prefabs de objetos del juego
    [Header("Prefabs de Objetos")]
    public FoodObject[] FoodPrefabs;
    public XPGemObject[] GemPrefabs;
    public ChestObject ChestPrefab;
    public WallObject WallPrefab;

    // Configuracion del tamano del tablero
    [Header("Dimensiones del Tablero")]
    public int Width = 8;
    public int Height = 8;

    // Tiles para el suelo y paredes
    [Header("Tiles")]
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    // Configuracion de enemigos
    [Header("Enemigos")]
    public EnemyObject[] EnemyPrefabs;
    [Range(0, 15)]
    public int EnemyDensity = 2;

    // Densidad de generacion de objetos
    [Header("Densidad Objetos")]
    [Range(0, 15)]
    public int WallDensity = 3;
    [Range(0, 15)]
    public int FoodDensity = 2;
    [Range(0, 15)]
    public int GemDensity = 3;
    [Range(0, 15)]
    public int ChestDensity = 1;

    // Inicializa el tablero y genera todo el contenido
    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        // Genera el grid base de tiles
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        // Coloca la salida en la esquina opuesta
        m_EmptyCellsList.Remove(new Vector2Int(1, 1));
        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        // Genera todos los objetos del nivel
        GenerateWall();
        GenerateChests();
        GenerateEnemies();
        GenerateGems();
        GenerateFood();
    }

    // Genera gemas de experiencia en celdas aleatorias
    void GenerateGems()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int gemCount = (totalPassableCells * GemDensity) / 27;

        if (gemCount > m_EmptyCellsList.Count) gemCount = m_EmptyCellsList.Count;

        for (int i = 0; i < gemCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            XPGemObject prefabToSpawn = GemPrefabs[Random.Range(0, GemPrefabs.Length)];
            AddObject(Instantiate(prefabToSpawn), coord);
        }
    }

    // Genera cofres con recompensas
    void GenerateChests()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int chestCount = (totalPassableCells * ChestDensity) / 150;

        if (chestCount > m_EmptyCellsList.Count) chestCount = m_EmptyCellsList.Count;

        for (int i = 0; i < chestCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            ChestObject newChest = Instantiate(ChestPrefab);
            AddObject(newChest, coord);
        }
    }

    // Genera comida para recuperar vida
    void GenerateFood()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int foodCount = (totalPassableCells * FoodDensity) / 27;

        if (foodCount > m_EmptyCellsList.Count)
            foodCount = m_EmptyCellsList.Count;

        for (int i = 0; i < foodCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            FoodObject prefabToSpawn = FoodPrefabs[Random.Range(0, FoodPrefabs.Length)];
            AddObject(Instantiate(prefabToSpawn), coord);
        }
    }

    // Genera muros destructibles
    void GenerateWall()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int wallCount = (totalPassableCells * WallDensity) / 27;

        if (wallCount > m_EmptyCellsList.Count)
            wallCount = m_EmptyCellsList.Count;

        for (int i = 0; i < wallCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);
            AddObject(Instantiate(WallPrefab), coord);
        }
    }

    // Genera enemigos en posiciones aleatorias
    void GenerateEnemies()
    {
        int totalPassableCells = (Width - 2) * (Height - 2);
        int enemyCount = (totalPassableCells * EnemyDensity) / 20;

        if (enemyCount > m_EmptyCellsList.Count) enemyCount = m_EmptyCellsList.Count;

        for (int i = 0; i < enemyCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            EnemyObject prefabToSpawn = EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)];
            AddObject(Instantiate(prefabToSpawn), coord);
        }
    }

    // Convierte coordenadas de celda a posicion mundial
    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    // Obtiene los datos de una celda especifica
    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
            return null;
        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    // Anade un objeto a una celda del tablero
    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    // Cambia el tile de una celda
    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    // Obtiene el tile de una celda
    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    // Limpia todo el tablero
    public void Clean()
    {
        if (m_BoardData == null)
            return;

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