using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Configurações da Grade")]
    public int gridWidth = 20; // Largura da grade em células
    public int gridHeight = 20; // Altura da grade em células
    public float cellSize = 1f; // Tamanho de cada célula (ajuste para combinar com seus assets)

    [Header("Prefabs dos Itens")]
    public GameObject[] itemPrefabs; // Array de prefabs para itens 0-8 (atribua no Inspector)

    public GameObject cursorPrefab; // Prefab do cursor visual
    public GameObject cellOutlinePrefab; // Prefab para outline das células

    private bool[,] occupiedCells; // Array para rastrear células ocupadas
    private GameObject[,] grid; // Array para armazenar os objetos colocados
    private int selectedObject = 0;
    private int cursorX = 0;
    private int cursorY = 0;
    private GameObject cursorInstance;

    public static GridManager Instance { get; private set; } // Singleton pattern

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        occupiedCells = new bool[gridWidth, gridHeight]; // Inicializa todas como falsas (livres)
        grid = new GameObject[gridWidth, gridHeight]; // Inicializa o grid de objetos

        // Cria o cursor visual
        cursorInstance = Instantiate(cursorPrefab, Vector3.zero, Quaternion.identity);
        cursorInstance.transform.localScale = Vector3.one * cellSize;

        // Cria os outlines das células uma única vez
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 1); // Z=1 para não cobrir sprites
                Instantiate(cellOutlinePrefab, pos, Quaternion.identity, transform);
            }
        }
    }

    void Update()
    {
        // Seleciona objeto (1–9, assumindo até 9 itens)
        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
                selectedObject = i;
        }

        // Movimenta cursor
        if (Input.GetKeyDown(KeyCode.UpArrow)) cursorY = Mathf.Clamp(cursorY + 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) cursorY = Mathf.Clamp(cursorY - 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) cursorX = Mathf.Clamp(cursorX - 1, 0, gridWidth - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) cursorX = Mathf.Clamp(cursorX + 1, 0, gridWidth - 1);

        // Atualiza posição do cursor visual (2D)
        cursorInstance.transform.position = new Vector3(cursorX * cellSize, cursorY * cellSize, 0);

        // Coloca objeto na célula atual se não ocupada
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceItem(cursorX, cursorY, selectedObject);
        }

        // Remove objeto
        if (Input.GetKeyDown(KeyCode.Delete))
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
        return true; // Fora dos limites = considerado ocupado
    }

    public void PlaceItem(int x, int y, int itemIndex)
    {
        if (IsCellOccupied(x, y)) return; // Não coloca se já ocupado

        if (itemIndex < 0 || itemIndex >= itemPrefabs.Length || itemPrefabs[itemIndex] == null)
        {
            Debug.LogWarning("Prefab de item inválido ou não atribuído!");
            return;
        }

        Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
        grid[x, y] = Instantiate(itemPrefabs[itemIndex], pos, Quaternion.identity);
        occupiedCells[x, y] = true;
    }

    public void RemoveItem(int x, int y)
    {
        if (!IsCellOccupied(x, y)) return;

        Destroy(grid[x, y]);
        grid[x, y] = null;
        occupiedCells[x, y] = false;
    }
}
 