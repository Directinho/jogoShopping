using UnityEngine;
using TMPro;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance { get; private set; }

    [Header("=== PREFABS DAS LOJAS ===")]
    public GameObject[] storePrefabsArray = new GameObject[4];

    [Header("Configurações Visuais")]
    public float storeScale = 0.7f;

    [Header("Preview Ghost")]
    public float ghostAlpha = 0.3f;
    public Color validColor = new Color(1f, 1f, 1f, 0.3f);
    public Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.3f);

    [Header("UI Feedback")]
    public TextMeshProUGUI storeNameText;
    public TextMeshProUGUI costWarningText;

    private GameObject previewInstance;
    private int currentPreviewIndex = -1;
    private float lastSelectionTime = 0f;
    private const float DOUBLE_TAP_THRESHOLD = 0.3f;

    private readonly string[] storeNames = { "Hamburgueria", "Padaria", "Abibas", "Arcade Alley" };
    private readonly int[] storeCosts = { 150, 50, 100, 300 };

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

    void Start()
    {
        HidePreview();
        UpdateStoreUI();

        // FORÇA SINCRONIZAÇÃO COM GRIDMANAGER
        if (GridManager.Instance != null && GridManager.Instance.itemPrefabs != null)
        {
            for (int i = 0; i < Mathf.Min(storePrefabsArray.Length, GridManager.Instance.itemPrefabs.Length); i++)
            {
                if (storePrefabsArray[i] == null && GridManager.Instance.itemPrefabs[i] != null)
                {
                    storePrefabsArray[i] = GridManager.Instance.itemPrefabs[i];
                    Debug.Log($"[PreviewManager] Prefab sincronizado: {storeNames[i]}");
                }
            }
        }
        else
        {
            Debug.LogError("[PreviewManager] GridManager ou itemPrefabs é null! Verifique a cena.");
        }
    }

    void Update()
    {
        HandleStoreSelection();
        HandleCancelInput();
        HandleAddMoney();
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
        }
    }

    private void HandleCancelInput()
    {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.Space))
        {
            CancelSelection();
        }
    }

    private void HandleAddMoney()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameManager.Instance?.AddMoney(100);
            Debug.Log("Adicionado +100 dinheiro!");
        }
    }

    public void SelectStore(int storeIndex)
    {
        if (storeIndex < 0 || storeIndex >= storePrefabsArray.Length)
        {
            Debug.LogWarning($"Índice de loja inválido: {storeIndex}");
            return;
        }

        if (storePrefabsArray[storeIndex] == null)
        {
            Debug.LogError($"Prefab da loja {storeNames[storeIndex]} (índice {storeIndex}) não foi atribuído!");
            return;
        }

        int cost = storeCosts[storeIndex];
        bool canAfford = GameManager.Instance == null || GameManager.Instance.GetCurrentMoney() >= cost;

        if (!canAfford)
        {
            ShowWarning("Dinheiro insuficiente!");
            return;
        }

        currentPreviewIndex = storeIndex;
        GameManager.Instance?.SetSelectedItem(storeIndex);
        CreatePreview();
        UpdateStoreUI();
        HideWarning();

        Debug.Log($"Loja selecionada: {storeNames[storeIndex]} (Custo: ${cost})");
    }

    public void CancelSelection()
    {
        if (currentPreviewIndex == -1) return;
        GameManager.Instance?.SetSelectedItem(-1);
        HidePreview();
    }

    private void CreatePreview()
    {
        if (previewInstance != null) Destroy(previewInstance);

        GameObject prefab = storePrefabsArray[currentPreviewIndex];
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
        foreach (var col in obj.GetComponentsInChildren<Collider2D>()) col.enabled = false;
        foreach (var script in obj.GetComponentsInChildren<MonoBehaviour>())
        {
            if (script.GetType() != typeof(Transform)) script.enabled = false;
        }
    }

    private void UpdatePreviewPosition()
    {
        if (previewInstance == null || GridManager.Instance == null || Camera.main == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
            storeNameText.text = currentPreviewIndex >= 0
                ? $"{storeNames[currentPreviewIndex]} - Custo: ${storeCosts[currentPreviewIndex]}"
                : "Selecione uma loja (1-4)";
        }
    }

    public void ShowWarning(string message)
    {
        if (costWarningText != null)
        {
            costWarningText.text = message;
            costWarningText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideWarning));
            Invoke(nameof(HideWarning), 2f);
        }
    }

    private void HideWarning()
    {
        if (costWarningText != null) costWarningText.gameObject.SetActive(false);
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

    public float GetStoreScale() => storeScale;

    void OnDestroy() => HidePreview();
}