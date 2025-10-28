using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public GridManager gridManager;
    public MouseManager mouseManager;
    public TextMeshProUGUI moneyText;
    public Canvas hudCanvas; // ARRASTE O CANVAS DO HUD AQUI!

    [Header("Sistema de Compras")]
    public int[] itemCosts = { 150, 50, 100, 300 };
    public int startingMoney = 1000;
    private int currentMoney;
    public int selectedItemIndex = -1;

    [Header("Efeito de Ganho")]
    public GameObject upcoinPrefab; // upcoin_0
    public float upcoinScale = 0.6f;
    public float upcoinAlpha = 0.4f;
    public float floatDuration = 1.2f;
    public float floatDistance = 40f;

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
        currentMoney = startingMoney;
        UpdateMoneyUI();

        if (hudCanvas == null)
            Debug.LogError("HUD Canvas não atribuído no GameManager!");
    }

    public void TryPlaceItem(int x, int y, int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= itemCosts.Length) return;

        int cost = itemCosts[itemIndex];
        if (currentMoney < cost) return;
        if (gridManager.IsCellOccupied(x, y)) return;

        gridManager.PlaceItem(x, y, itemIndex);
        currentMoney -= cost;
        UpdateMoneyUI();
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
            moneyText.text = $"${currentMoney}"; // SÓ OS NÚMEROS
        }
    }

    private IEnumerator ShowMoneyGainEffect(int amount)
    {
        // === CRIA ÍCONE UPCOIN NO CANVAS ===
        GameObject upcoin = Instantiate(upcoinPrefab, hudCanvas.transform);
        upcoin.name = "UpcoinEffect";

        // Usa RectTransform para UI
        RectTransform rt = upcoin.GetComponent<RectTransform>();
        if (rt == null) rt = upcoin.AddComponent<RectTransform>();

        // Posiciona no centro do MoneyText
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one * upcoinScale;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Aplica transparência
        Image img = upcoin.GetComponent<Image>();
        if (img == null)
        {
            SpriteRenderer sr = upcoin.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = upcoinAlpha;
                sr.color = c;
            }
        }
        else
        {
            Color c = img.color;
            c.a = upcoinAlpha;
            img.color = c;
        }

        // === CRIA TEXTO FLUTUANTE ===
        GameObject textObj = new GameObject("GainText");
        textObj.transform.SetParent(hudCanvas.transform);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchoredPosition = new Vector2(0, -20);
        textRt.localScale = Vector3.one;
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.pivot = new Vector2(0.5f, 0.5f);

        TextMeshProUGUI gainText = textObj.AddComponent<TextMeshProUGUI>();
        gainText.text = $"+{amount}";
        gainText.fontSize = 28;
        gainText.color = new Color(0.2f, 1f, 0.2f, 1f);
        gainText.alignment = TextAlignmentOptions.Center;
        gainText.font = moneyText.font;

        // === ANIMAÇÃO ===
        float elapsed = 0f;
        Vector2 startPos = textRt.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatDistance;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            textRt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // Fade texto
            Color textColor = gainText.color;
            textColor.a = Mathf.Lerp(1f, 0f, t);
            gainText.color = textColor;

            // Fade ícone
            if (img != null)
            {
                Color iconColor = img.color;
                iconColor.a = Mathf.Lerp(upcoinAlpha, 0f, t);
                img.color = iconColor;
            }

            yield return null;
        }

        // Destroi
        if (upcoin != null) Destroy(upcoin);
        if (textObj != null) Destroy(textObj);
    }
}