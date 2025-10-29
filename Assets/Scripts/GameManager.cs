using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Para o Reset com 'R'

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public GridManager gridManager;
    public MouseManager mouseManager;
    public TextMeshProUGUI moneyText;
    public Canvas hudCanvas;

    [Header("Sistema de Compras")]
    public int[] itemCosts = { 150, 50, 100, 300 };
    public int startingMoney = 1000;
    private int currentMoney;
    public int selectedItemIndex = -1;

    [Header("Efeito de Ganho")]
    public GameObject upcoinPrefab;
    public float upcoinScale = 0.6f;
    public float upcoinAlpha = 0.4f;
    public float floatDuration = 1.2f;
    public float floatDistance = 40f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Inicializa dinheiro APÓS garantir que o objeto não será destruído
        currentMoney = startingMoney;
        UpdateMoneyUI();
    }

    void Start()
    {
        if (gridManager == null) Debug.LogError("GridManager não configurado!");
        if (mouseManager == null) Debug.LogError("MouseManager não configurado!");
        if (hudCanvas == null) Debug.LogError("HUD Canvas não atribuído!");
    }

    void Update()
    {
        // RESET RÁPIDO PARA TESTES
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void TryPlaceItem(int x, int y, int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= itemCosts.Length)
        {
            Debug.LogError($"[GameManager] Índice inválido: {itemIndex}");
            return;
        }

        int cost = itemCosts[itemIndex];
        if (currentMoney < cost)
        {
            PreviewManager.Instance?.ShowWarning("Dinheiro insuficiente!");
            Debug.Log($"[GameManager] Dinheiro insuficiente: {currentMoney} < {cost}");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("[GameManager] gridManager é null!");
            return;
        }

        if (gridManager.IsCellOccupied(x, y))
        {
            PreviewManager.Instance?.ShowWarning("Célula ocupada!");
            Debug.Log($"[GameManager] Célula ({x}, {y}) ocupada!");
            return;
        }

        gridManager.PlaceItem(x, y, itemIndex);
        currentMoney -= cost;
        UpdateMoneyUI();

        Debug.Log($"[GameManager] Loja colocada com sucesso em ({x}, {y})! Dinheiro: {currentMoney}");
    }

    public void SetSelectedItem(int index)
    {
        selectedItemIndex = (index >= 0 && index < itemCosts.Length) ? index : -1;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();

        if (upcoinPrefab != null && moneyText != null && hudCanvas != null)
        {
            StartCoroutine(ShowMoneyGainEffect(amount));
        }
    }

    public int GetCurrentMoney()
    {
        return currentMoney;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"${currentMoney}";
        }
    }

    private IEnumerator ShowMoneyGainEffect(int amount)
    {
        GameObject upcoin = Instantiate(upcoinPrefab, hudCanvas.transform);
        upcoin.name = "UpcoinEffect";

        RectTransform rt = upcoin.GetComponent<RectTransform>();
        if (rt == null) rt = upcoin.AddComponent<RectTransform>();

        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one * upcoinScale;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Image img = upcoin.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = upcoinAlpha;
            img.color = c;
        }

        GameObject textObj = new GameObject("GainText");
        textObj.transform.SetParent(hudCanvas.transform);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchoredPosition = new Vector2(0, -20);
        textRt.localScale = Vector3.one;
        textRt.anchorMin = textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.pivot = new Vector2(0.5f, 0.5f);

        TextMeshProUGUI gainText = textObj.AddComponent<TextMeshProUGUI>();
        gainText.text = $"+{amount}";
        gainText.fontSize = 28;
        gainText.color = new Color(0.2f, 1f, 0.2f, 1f);
        gainText.alignment = TextAlignmentOptions.Center;
        gainText.font = moneyText.font;

        float elapsed = 0f;
        Vector2 startPos = textRt.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatDistance;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            textRt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            Color textColor = gainText.color;
            textColor.a = Mathf.Lerp(1f, 0f, t);
            gainText.color = textColor;

            if (img != null)
            {
                Color iconColor = img.color;
                iconColor.a = Mathf.Lerp(upcoinAlpha, 0f, t);
                img.color = iconColor;
            }

            yield return null;
        }

        if (upcoin != null) Destroy(upcoin);
        if (textObj != null) Destroy(textObj);
    }
}