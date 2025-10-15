using UnityEngine;

public class PlayerBasico : MonoBehaviour
{
    // Velocidade horizontal do jogador
    public float velocidade = 5f;

    // For�a aplicada ao pular
    public float forcaPulo = 8f;


    // Componentes
    private Rigidbody2D rb;
    private bool estaNoChao;

    void Start()
    {
        // Pega a refer�ncia ao Rigidbody2D do jogador
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Captura o movimento horizontal (setas ou A/D)
        float movimentoX = 0f;
        float movimentoY = 0f;

        if (Input.GetKey(KeyCode.A)) movimentoX = -1f;
        if (Input.GetKey(KeyCode.D)) movimentoX = 1f;
        if (Input.GetKey(KeyCode.W)) movimentoY = 1f;
        if (Input.GetKey(KeyCode.S)) movimentoY = -1f;

        // Define a velocidade horizontal (mant�m a velocidade vertical)
        rb.linearVelocity = new Vector2(movimentoX * velocidade, movimentoY * velocidade);


        // Se o jogador apertar espa�o e estiver no ch�o, pula
        if (Input.GetKeyDown(KeyCode.Space) && estaNoChao)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaPulo);
        }

        // Inverte o sprite quando muda de dire��o (opcional)
        if (movimentoX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(movimentoX), 1f, 1f);
        }
    }
}

    // Desenha o c�rculo de checagem do ch�o no editor (ajuda visualmente)
  