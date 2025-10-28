using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public GridManager gridManager;
    public MouseManager mouseManager;
    public TextMeshProUGUI moneyText; // UI para exibir dinheiro

    [Header("Sistema de Compras")]
    public int[] itemCosts = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90 }; // Custos dos itens 1-9
    public int startingMoney = 1000;
    private int currentMoney;
    public int selectedItemIndex = -1; // Inicie como -1, a ser definido por UI ou Inspector

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

        if (gridManager == null)
        {
            Debug.LogError("GridManager não configurado no GameManager!");
        }

        if (mouseManager == null)
        {
            Debug.LogError("MouseManager não configurado no GameManager!");
        }
    }

    public void TryPlaceItem(int x, int y, int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= itemCosts.Length)
        {
            Debug.LogWarning("Índice de item inválido!");
            return;
        }

        int cost = itemCosts[itemIndex];
        if (currentMoney >= cost && !gridManager.IsCellOccupied(x, y))
        {
            // Chama PlaceItem com 3 argumentos (x, y, itemIndex)
            gridManager.PlaceItem(x, y, itemIndex);
            currentMoney -= cost;
            UpdateMoneyUI();
            Debug.Log($"Item {itemIndex + 1} colocado em ({x}, {y}) por {cost} moedas.");
        }
        else if (currentMoney < cost)
        {
            Debug.Log("Dinheiro insuficiente para comprar este item!");
        }
        else
        {
            Debug.Log("Célula já ocupada!");
        }
    }

    public void SetSelectedItem(int index)
    {
        if (index >= 0 && index < itemCosts.Length)
        {
            selectedItemIndex = index;
            Debug.Log($"Item selecionado: {selectedItemIndex + 1}");
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Dinheiro: {currentMoney}";
        }
    }
}
 