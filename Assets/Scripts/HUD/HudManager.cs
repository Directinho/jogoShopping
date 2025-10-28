using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [Header("Botões do HUD")]
    public Button cursorButton;
    public Button moveButton;
    public Button trashButton;

    [Header("Barra de Construção")]
    public RectTransform barra;
    public Vector2 barraPosVisivel = new Vector2(0, 0);
    public Vector2 barraPosEscondida = new Vector2(0, -200);
    public float animationSpeed = 8f; // Lerp speed

    // Optional: referência ao Canvas (se quiser garantir parent)
    public Canvas parentCanvas;

    private bool isBarVisible = false;
    private Vector2 targetPos;
    private CanvasGroup canvasGroup;

    void Start()
    {
        if (barra == null)
        {
            Debug.LogError("Barra não atribuída no HudManager!");
            return;
        }

        // Garante que barra esteja com escala 1 (evita distorções causadas por pais)
        barra.localScale = Vector3.one;

        // Se forneceu Canvas, garante que a barra seja filha dele (mantendo transform local)
        if (parentCanvas != null)
        {
            barra.SetParent(parentCanvas.transform, false);
        }

        // Garante que a barra use um CanvasGroup para controlar visibilidade/interação
        canvasGroup = barra.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = barra.gameObject.AddComponent<CanvasGroup>();

        // Inicializa escondida (posição e alpha)
        barra.anchoredPosition = barraPosEscondida;
        targetPos = barraPosEscondida;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Liga botões (com checagem nula)
        if (cursorButton) cursorButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Place));
        if (moveButton) moveButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Drag));
        if (trashButton) trashButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Delete));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isBarVisible = !isBarVisible;
            targetPos = isBarVisible ? barraPosVisivel : barraPosEscondida;

            // Ativa/Desativa interação imediatamente (mas a alpha ainda anima)
            canvasGroup.blocksRaycasts = isBarVisible;
            canvasGroup.interactable = isBarVisible;
        }

        // Anima suavemente posição
        barra.anchoredPosition = Vector2.Lerp(barra.anchoredPosition, targetPos, Time.deltaTime * animationSpeed);

        // Anima alpha (fade) para ficar mais óbvio visualmente
        float targetAlpha = isBarVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * animationSpeed * 1.5f);
    }
}
