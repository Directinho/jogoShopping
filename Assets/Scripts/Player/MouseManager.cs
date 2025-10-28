using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; } // Singleton

    public enum MouseMode { Place, Drag, Delete }

    [Header("Configurações")]
    public float dragSpeed = 0.1f; // Velocidade do arrasto da câmera (ajuste no Inspector)
    public float dragThreshold = 5f; // Distância mínima para considerar arrasto (evita mudanças acidentais)

    [Header("Visual do Cursor no Grid")]
    public Transform gridCursor; // Arraste um GameObject (ex: Sprite) para ser o indicador de célula
    public bool clampToGridBounds = true; // Mantém o cursor dentro dos limites da grade
    public float gridCursorZ = 0f; // Z para posicionar o visual do cursor
    public SpriteRenderer gridCursorRenderer; // Opcional: definir cor/ordem do visual
    public Color freeCellColor = new Color(0f, 1f, 0f, 0.35f);
    public Color occupiedCellColor = new Color(1f, 0f, 0f, 0.35f);
    public bool matchCellSize = true; // Ajustar o tamanho do visual à célula
    public string gridCursorSortingLayer = "Default";
    public int gridCursorSortingOrder = 1000; // alto para ficar na frente

    // Estado para respeitar regras do grid
    private Vector2Int lastFreeCell;
    private bool hasLastFreeCell = false;

    private MouseMode currentMode = MouseMode.Place;
    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Toggle modos com atalhos
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetMode(MouseMode.Drag);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            SetMode(MouseMode.Place);
        }

        // Obtém posição do mouse
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        int x = Mathf.FloorToInt(worldPos.x / GridManager.Instance.cellSize);
        int y = Mathf.FloorToInt(worldPos.y / GridManager.Instance.cellSize);

        bool isDeleteMode = (currentMode == MouseMode.Delete);

        // Atualiza visual do cursor alinhado ao grid
        if (gridCursor != null && GridManager.Instance != null && Camera.main != null)
        {
            int gx = x;
            int gy = y;
            if (clampToGridBounds)
            {
                gx = Mathf.Clamp(gx, 0, GridManager.Instance.gridWidth - 1);
                gy = Mathf.Clamp(gy, 0, GridManager.Instance.gridHeight - 1);
            }

            float cs = GridManager.Instance.cellSize;
            bool insideBounds = gx >= 0 && gx < GridManager.Instance.gridWidth && gy >= 0 && gy < GridManager.Instance.gridHeight;

            // Determina célula alvo respeitando ocupadas e modo
            bool targetOccupied = insideBounds && GridManager.Instance.IsCellOccupied(gx, gy);
            bool isValid = insideBounds && (isDeleteMode ? targetOccupied : !targetOccupied);
            Vector2Int cellToShow;
            if (isValid)
            {
                cellToShow = new Vector2Int(gx, gy);
                hasLastFreeCell = true;
                lastFreeCell = cellToShow;
            }
            else if (hasLastFreeCell)
            {
                cellToShow = lastFreeCell;
            }
            else
            {
                cellToShow = new Vector2Int(Mathf.Clamp(gx, 0, GridManager.Instance.gridWidth - 1), Mathf.Clamp(gy, 0, GridManager.Instance.gridHeight - 1));
            }

            Vector3 snapped = new Vector3(cellToShow.x * cs + cs * 0.5f, cellToShow.y * cs + cs * 0.5f, gridCursorZ);
            gridCursor.position = snapped;

            if (matchCellSize)
            {
                gridCursor.localScale = new Vector3(cs, cs, 1f);
            }

            if (gridCursorRenderer != null)
            {
                gridCursorRenderer.color = isValid ? freeCellColor : occupiedCellColor;
                gridCursorRenderer.sortingLayerName = gridCursorSortingLayer;
                gridCursorRenderer.sortingOrder = gridCursorSortingOrder;
            }

            // Visibilidade baseada em limites
            if (insideBounds)
            {
                if (!gridCursor.gameObject.activeSelf) gridCursor.gameObject.SetActive(true);
            }
            else
            {
                if (gridCursor.gameObject.activeSelf) gridCursor.gameObject.SetActive(false);
            }
        }

        // Lógica baseada no modo
        if (currentMode == MouseMode.Drag)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CursorManager.Instance.SetOnMoveCursor();
                isDragging = true;
                lastMousePosition = mousePos;
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 delta = mousePos - lastMousePosition;
                // Arraste a câmera na direção oposta ao movimento do mouse
                Camera.main.transform.position -= new Vector3(delta.x * dragSpeed, delta.y * dragSpeed, 0);
                lastMousePosition = mousePos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                CursorManager.Instance.SetMoveCursor();
                isDragging = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (x >= 0 && x < GridManager.Instance.gridWidth && y >= 0 && y < GridManager.Instance.gridHeight)
                {
                    if (currentMode == MouseMode.Place)
                    {
                        int selected = GameManager.Instance.selectedItemIndex;
                        if (selected >= 0)
                        {
                            GameManager.Instance.TryPlaceItem(x, y, selected);
                        }
                        else
                        {
                            Debug.Log("Nenhum item selecionado! Configure via UI ou Inspector.");
                        }
                    }
                    else if (currentMode == MouseMode.Delete)
                    {
                        if (GridManager.Instance.IsCellOccupied(x, y))
                        {
                            GridManager.Instance.RemoveItem(x, y);
                        }
                        else
                        {
                            Debug.Log("Nada para remover nesta célula!");
                        }
                    }
                }
            }
        }
    }

    public void SetMode(MouseMode mode)
    {
        currentMode = mode;
        if (mode == MouseMode.Drag)
        {
            CursorManager.Instance.SetMoveCursor();
            Debug.Log("Modo de arrasto ativado (cursor 'move').");
        }
        else if (mode == MouseMode.Place)
        {
            CursorManager.Instance.SetNormalCursor();
            Debug.Log("Modo de colocação ativado (cursor normal).");
        }
        else if (mode == MouseMode.Delete)
        {
            CursorManager.Instance.SetTrashCursor();
            Debug.Log("Modo de remoção ativado (cursor 'trash').");
        }
        isDragging = false;
    }
}