using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Configurações da Grade")]
    public int gridWidth = 20; // Largura da grade em células
    public int gridHeight = 20; // Altura da grade em células
    public float cellSize = 1f; // Tamanho de cada célula

    [Header("Prefabs dos Itens")]
    public GameObject[] itemPrefabs; // Prefabs para itens 0-8
    public GameObject cursorPrefab; // Cursor visual
    public GameObject cellOutlinePrefab; // Outline das células

    private bool[,] occupiedCells;
    private GameObject[,] grid;
    private int cursorX = 0;
    private int cursorY = 0;
    private GameObject cursorInstance;

    public static GridManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Inicializa arrays
        occupiedCells = new bool[gridWidth, gridHeight];
        grid = new GameObject[gridWidth, gridHeight];

        // Cria cursor
        if (cursorPrefab != null)
        {
            cursorInstance = Instantiate(cursorPrefab, Vector3.zero, Quaternion.identity);
            cursorInstance.transform.localScale = Vector3.one * cellSize;
        }
        else
        {
            Debug.LogError("CursorPrefab não atribuído no GridManager!");
        }

        // Cria outlines das células
        if (cellOutlinePrefab != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 pos = new Vector3(x * cellSize, y * cellSize, 1);
                    Instantiate(cellOutlinePrefab, pos, Quaternion.identity, transform);
                }
            }
        }
        else
        {
            Debug.LogError("CellOutlinePrefab não atribuído!");
        }
    }

    void Update()
    {
        // === MOVIMENTAÇÃO DO CURSOR ===
        if (Input.GetKeyDown(KeyCode.UpArrow)) cursorY = Mathf.Clamp(cursorY + 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) cursorY = Mathf.Clamp(cursorY - 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) cursorX = Mathf.Clamp(cursorX - 1, 0, gridWidth - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) cursorX = Mathf.Clamp(cursorX + 1, 0, gridWidth - 1);

        // Atualiza posição do cursor (com proteção contra null)
        if (cursorInstance != null)
        {
            cursorInstance.transform.position = new Vector3(cursorX * cellSize, cursorY * cellSize, 0);
        }

        // === COLOCAR ITEM ===
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance != null && GameManager.Instance.selectedItemIndex >= 0)
            {
                GameManager.Instance.TryPlaceItem(cursorX, cursorY, GameManager.Instance.selectedItemIndex);
            }
            else
            {
                Debug.Log("Nenhum item selecionado para colocar!");
            }
        }

        // === REMOVER ITEM ===
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveItem(cursorX, cursorY);
        }
    }

    public bool IsCellOccupied(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return occupiedCells[x, y];
        }
        return true; // Fora dos limites = ocupado
    }

    public void PlaceItem(int x, int y, int itemIndex)
    {
        // Validações de segurança
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            Debug.LogWarning($"Posição inválida: ({x}, {y})");
            return;
        }

        if (IsCellOccupied(x, y))
        {
            Debug.Log($"Célula ({x}, {y}) já está ocupada!");
            return;
        }

        if (itemIndex < 0 || itemIndex >= itemPrefabs.Length || itemPrefabs[itemIndex] == null)
        {
            Debug.LogWarning($"Prefab inválido no índice {itemIndex}");
            return;
        }

        Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
        grid[x, y] = Instantiate(itemPrefabs[itemIndex], pos, Quaternion.identity);
        occupiedCells[x, y] = true;

        Debug.Log($"Item {itemIndex} colocado em ({x}, {y})");
    }

    public void RemoveItem(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return;

        if (!occupiedCells[x, y])
            return;

        if (grid[x, y] != null)
        {
            Destroy(grid[x, y]);   // Destroi o objeto
            grid[x, y] = null;     // ← ESSENCIAL: limpa a referência
        }

        occupiedCells[x, y] = false;
        Debug.Log($"Item removido em ({x}, {y})");
    }

    // Opcional: Retorna o item na posição (útil para UI ou save)
    public GameObject GetItemAt(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];
        return null;
    }

    // Opcional: Limpa toda a grade (útil para reset)
    public void ClearGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                RemoveItem(x, y);
            }
        }
    }
}