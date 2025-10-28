using UnityEngine;
using TMPro;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance { get; private set; }

    [Header("=== PREFABS DAS LOJAS (ARRASTE AQUI) ===")]
    [Tooltip("1 - Hamburgueria")]
    public GameObject hamburgueriaPrefab;
    [Tooltip("2 - Padaria")]
    public GameObject padariaPrefab;
    [Tooltip("3 - Abibas")]
    public GameObject abibasPrefab;
    [Tooltip("4 - Arcade Alley")]
    public GameObject arcadeAlleyPrefab;

    [Header("Configurações Visuais")]
    public float storeScale = 0.7f;

    [Header("Preview Ghost")]
    public float ghostAlpha = 0.3f;
    public Color validColor = new Color(1f, 1f, 1f, 0.3f);
    public Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.3f);

    [Header("UI Feedback (Opcional)")]
    public TextMeshProUGUI storeNameText;
    public TextMeshProUGUI costWarningText;

    private GameObject previewInstance;
    private int currentPreviewIndex = -1;
    private float lastSelectionTime = 0f;
    private int lastSelectedIndex = -1;
    private const float DOUBLE_TAP_THRESHOLD = 0.3f;

    private GameObject[] storePrefabs;
    private readonly string[] storeNames = { "Hamburgueria", "Padaria", "Abibas", "Arcade Alley" };
    private readonly int[] storeCosts = { 150, 50, 100, 300 }; // CORRIGIDO: Padaria=50, Abibas=100

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

        storePrefabs = new GameObject[] { hamburgueriaPrefab, padariaPrefab, abibasPrefab, arcadeAlleyPrefab };
    }

    void Start()
    {
        HidePreview();
        UpdateStoreUI();
    }

    void Update()
    {
        HandleStoreSelection();
        HandleCancelInput();
        HandleAddMoney(); // NOVA FUNÇÃO
        UpdatePreviewPosition();
    }

    private void HandleStoreSelection()
    {
        int index = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) index = 3;

        if (index != -1)
        {
            float timeSinceLast = Time.time - lastSelectionTime;

            if (currentPreviewIndex == index && timeSinceLast < DOUBLE_TAP_THRESHOLD)
            {
                CancelSelection();
            }
            else
            {
                SelectStore(index);
            }

            lastSelectionTime = Time.time;
            lastSelectedIndex = index;
        }
    }

    private void HandleCancelInput()
    {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.Space))
        {
            CancelSelection();
        }
    }

    private void HandleAddMoney() // NOVA FUNÇÃO
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameManager.Instance?.AddMoney(100);
            Debug.Log("Adicionado +100 dinheiro!");
        }
    }

    public void SelectStore(int storeIndex)
    {
        if (storeIndex < 0 || storeIndex >= storePrefabs.Length)
        {
            Debug.LogWarning($"Índice de loja inválido: {storeIndex}");
            return;
        }

        if (storePrefabs[storeIndex] == null)
        {
            Debug.LogError($"Prefab da loja {storeNames[storeIndex]} não foi atribuído no Inspector!");
            return;
        }

        int cost = storeCosts[storeIndex];
        bool canAfford = GameManager.Instance == null || GameManager.Instance.GetCurrentMoney() >= cost;

        if (!canAfford)
        {
            ShowCostWarning();
            return;
        }

        currentPreviewIndex = storeIndex;
        GameManager.Instance?.SetSelectedItem(storeIndex);

        CreatePreview();
        UpdateStoreUI();
        HideCostWarning();

        Debug.Log($"Loja selecionada: {storeNames[storeIndex]} (Custo: ${cost})");
    }

    public void CancelSelection()
    {
        if (currentPreviewIndex == -1) return;

        Debug.Log($"Seleção cancelada: {storeNames[currentPreviewIndex]}");
        GameManager.Instance?.SetSelectedItem(-1);
        HidePreview();
    }

    private void CreatePreview()
    {
        if (previewInstance != null)
            Destroy(previewInstance);

        GameObject prefab = storePrefabs[currentPreviewIndex];
        if (prefab == null) return;

        previewInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        previewInstance.name = "StorePreview";
        previewInstance.transform.localScale = Vector3.one * storeScale;

        ApplyGhostEffect(previewInstance);
    }

    private void ApplyGhostEffect(GameObject obj)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            if (r is SpriteRenderer sprite)
            {
                Color c = sprite.color;
                c.a = ghostAlpha;
                sprite.color = c;
            }
        }

        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        foreach (var script in obj.GetComponentsInChildren<MonoBehaviour>())
        {
            if (script.GetType() != typeof(Transform))
                script.enabled = false;
        }
    }

    private void UpdatePreviewPosition()
    {
        if (previewInstance == null || GridManager.Instance == null || Camera.main == null) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;

        float cellSize = GridManager.Instance.cellSize;
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);

        x = Mathf.Clamp(x, 0, GridManager.Instance.gridWidth - 1);
        y = Mathf.Clamp(y, 0, GridManager.Instance.gridHeight - 1);

        Vector3 gridPos = new Vector3(x * cellSize, y * cellSize, -1f);
        previewInstance.transform.position = gridPos;

        bool isValid = !GridManager.Instance.IsCellOccupied(x, y);
        Color targetColor = isValid ? validColor : invalidColor;

        foreach (Renderer r in previewInstance.GetComponentsInChildren<Renderer>())
        {
            if (r is SpriteRenderer sprite)
            {
                sprite.color = Color.Lerp(sprite.color, targetColor, Time.deltaTime * 20f);
            }
        }
    }

    private void UpdateStoreUI()
    {
        if (storeNameText != null)
        {
            if (currentPreviewIndex >= 0 && currentPreviewIndex < storeNames.Length)
            {
                storeNameText.text = $"{storeNames[currentPreviewIndex]} - Custo: ${storeCosts[currentPreviewIndex]}";
            }
            else
            {
                storeNameText.text = "Selecione uma loja (1-4)";
            }
        }
    }

    private void ShowCostWarning()
    {
        if (costWarningText != null)
        {
            costWarningText.text = "Dinheiro insuficiente!";
            costWarningText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideCostWarning));
            Invoke(nameof(HideCostWarning), 2f);
        }
    }

    private void HideCostWarning()
    {
        if (costWarningText != null)
            costWarningText.gameObject.SetActive(false);
    }

    public void HidePreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
        currentPreviewIndex = -1;
        UpdateStoreUI();
    }

    public float GetStoreScale()
    {
        return storeScale;
    }

    void OnDestroy()
    {
        HidePreview();
    }
}