using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    public enum MouseMode { Place, Drag, Delete }

    [Header("Configurações")]
    public float dragSpeed = 0.1f;

    [Header("Visual do Cursor no Grid")]
    public Transform gridCursor;
    public bool clampToGridBounds = true;
    public float gridCursorZ = 0f;
    public SpriteRenderer gridCursorRenderer;
    public Color freeCellColor = new Color(0f, 1f, 0f, 0.35f);
    public Color occupiedCellColor = new Color(1f, 0f, 0f, 0.35f);
    public bool matchCellSize = true;
    public string gridCursorSortingLayer = "Default";
    public int gridCursorSortingOrder = 1000;

    private Vector2Int lastFreeCell;
    private bool hasLastFreeCell = false;
    private MouseMode currentMode = MouseMode.Place;
    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SetMode(MouseMode.Drag);
        if (Input.GetKeyDown(KeyCode.V)) SetMode(MouseMode.Place);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.FloorToInt(worldPos.x / GridManager.Instance.cellSize);
        int y = Mathf.FloorToInt(worldPos.y / GridManager.Instance.cellSize);

        bool isDeleteMode = currentMode == MouseMode.Delete;

        // Atualiza visual do cursor
        if (gridCursor != null && GridManager.Instance != null)
        {
            int gx = Mathf.Clamp(x, 0, GridManager.Instance.gridWidth - 1);
            int gy = Mathf.Clamp(y, 0, GridManager.Instance.gridHeight - 1);
            float cs = GridManager.Instance.cellSize;
            bool insideBounds = x >= 0 && x < GridManager.Instance.gridWidth && y >= 0 && y < GridManager.Instance.gridHeight;

            bool targetOccupied = insideBounds && GridManager.Instance.IsCellOccupied(gx, gy);
            bool isValid = insideBounds && (isDeleteMode ? targetOccupied : !targetOccupied);

            Vector2Int cellToShow = isValid ? new Vector2Int(gx, gy) : (hasLastFreeCell ? lastFreeCell : new Vector2Int(gx, gy));
            if (isValid) { lastFreeCell = cellToShow; hasLastFreeCell = true; }

            Vector3 pos = new Vector3(cellToShow.x * cs + cs * 0.5f, cellToShow.y * cs + cs * 0.5f, gridCursorZ);
            gridCursor.position = pos;
            if (matchCellSize) gridCursor.localScale = new Vector3(cs, cs, 1f);
            if (gridCursorRenderer != null)
            {
                gridCursorRenderer.color = isValid ? freeCellColor : occupiedCellColor;
                gridCursorRenderer.sortingLayerName = gridCursorSortingLayer;
                gridCursorRenderer.sortingOrder = gridCursorSortingOrder;
            }
            gridCursor.gameObject.SetActive(insideBounds);
        }

        // Modo Drag
        if (currentMode == MouseMode.Drag)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CursorManager.Instance.SetOnMoveCursor();
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                Camera.main.transform.position -= new Vector3(delta.x * dragSpeed, delta.y * dragSpeed, 0);
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                CursorManager.Instance.SetMoveCursor();
                isDragging = false;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (x >= 0 && x < GridManager.Instance.gridWidth && y >= 0 && y < GridManager.Instance.gridHeight)
            {
                if (currentMode == MouseMode.Place)
                {
                    int selected = GameManager.Instance.selectedItemIndex;
                    if (selected >= 0)
                    {
                        if (GridManager.Instance.IsCellOccupied(x, y))
                            PreviewManager.Instance.ShowWarning("Célula ocupada!");
                        else
                            GameManager.Instance.TryPlaceItem(x, y, selected);
                    }
                }
                else if (currentMode == MouseMode.Delete && GridManager.Instance.IsCellOccupied(x, y))
                {
                    GridManager.Instance.RemoveItem(x, y);
                }
            }
        }
    }

    public void SetMode(MouseMode mode)
    {
        currentMode = mode;
        CursorManager.Instance.SetNormalCursor();
        if (mode == MouseMode.Drag) CursorManager.Instance.SetMoveCursor();
        else if (mode == MouseMode.Delete) CursorManager.Instance.SetTrashCursor();
        isDragging = false;
    }
}