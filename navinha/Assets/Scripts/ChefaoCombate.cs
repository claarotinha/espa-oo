using UnityEngine;

// 0-2: vida alta; 3-5: media; 6-8: baixa.
[RequireComponent(typeof(ChefaoAlvo), typeof(Animator))]
public class ChefaoCombate : MonoBehaviour
{
    [SerializeField] private float duracaoComportamento = 3.5f;
    [SerializeField] private float amplitudeMovimento = 2.6f;
    [SerializeField] private float danoContatoCooldown = 1f;
    private ChefaoAlvo vida;
    private Animator animator;
    private Transform jogador;
    private Vector3 origem;
    private float tempoEstado, tempoTiro, tempoDano;
    private int comportamento = -1;
    private int anterior = -1;
    private int faseAnterior = -1;
    private SpriteRenderer visual;
    private Color corOriginal;
    private Vector3 velocidadeSuavizacao;
    private Sprite spriteBalaJogador;
    private Vector3 escalaBalaJogador = new Vector3(.2f, .5f, 1f);

    private void Awake()
    {
        vida = GetComponent<ChefaoAlvo>();
        animator = GetComponent<Animator>();
        visual = GetComponent<SpriteRenderer>();
        if (visual != null) corOriginal = visual.color;
        origem = transform.position;
    }

    private void Start()
    {
        Jogador p = FindFirstObjectByType<Jogador>();
        if (p != null) jogador = p.transform;
        DisparoJogador disparo = FindFirstObjectByType<DisparoJogador>();
        if (disparo != null)
        {
            spriteBalaJogador = disparo.SpriteBala;
            escalaBalaJogador = disparo.EscalaBala;
        }
        Trocar(true);
    }

    private void Update()
    {
        if (vida.Vida <= 0) { animator.speed = 0; return; }
        int fase = IndiceFase();
        if (fase != faseAnterior || tempoEstado >= duracaoComportamento)
            Trocar(fase != faseAnterior);
    }

    private int IndiceFase() => vida.Fase == FaseVidaChefao.Alta ? 0 :
                                 vida.Fase == FaseVidaChefao.Media ? 1 : 2;

    private void Trocar(bool mudouFase)
    {
        int fase = IndiceFase();
        int proximo = fase * 3 + (mudouFase ? 0 : (anterior % 3 + 1) % 3);
        anterior = proximo;
        faseAnterior = fase;
        tempoEstado = 0;
        animator.Play("Comportamento" + (proximo + 1), 0, 0f);
    }

    public void Iniciar(int tipo)
    {
        comportamento = tipo;
        tempoEstado = 0;
        tempoTiro = 0;
    }

    public void Executar(int tipo)
    {
        if (vida.Vida <= 0) return;
        tempoEstado += Time.deltaTime;
        float t = Time.time;
        float rapidez = tipo < 3 ? 1f : tipo < 6 ? 1.6f : 2.4f;
        float amplitude = amplitudeMovimento;
        float x = origem.x;
        // 9 padroes de movimento distintos.
        switch (tipo)
        {
            case 0: x += Mathf.Sin(t * rapidez) * amplitude * .45f; break;
            case 1: x += Mathf.Sin(t * rapidez * 1.5f) * amplitude * .75f; break;
            case 2: x += Mathf.PingPong(t * rapidez * 1.5f, amplitude * 2) - amplitude; break;
            case 3: x += Mathf.Sin(t * rapidez * 2f) * amplitude; break;
            case 4: x += Mathf.Sin(t * rapidez) * Mathf.Cos(t * rapidez * 2f) * amplitude; break;
            case 5: x += Mathf.PingPong(t * rapidez * 2.4f, amplitude * 2) - amplitude; break;
            case 6: x += Mathf.Sin(t * rapidez * 2.6f) * amplitude; break;
            case 7: x += Mathf.Sin(t * rapidez * 3.3f) * amplitude * .85f; break;
            case 8: x += Mathf.PingPong(t * rapidez * 3.4f, amplitude * 2) - amplitude; break;
        }
        float y = origem.y + (tipo % 3 == 1 ? Mathf.Sin(t * rapidez * 2f) * .35f : 0);
        // Interpola entre padroes para evitar saltos nas trocas de estado.
        Vector3 destino = new Vector3(x, y, origem.z);
        transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocidadeSuavizacao, .28f, 12f);
        // Variacao visual para evidenciar os estados sem depender de assets extras.
        if (visual != null) visual.color = Color.Lerp(corOriginal, Color.white, .12f + .18f * Mathf.Abs(Mathf.Sin(t * rapidez)));
        tempoTiro += Time.deltaTime;
        float intervalo = tipo < 3 ? 1.5f : tipo < 6 ? .95f : .55f;
        if (tempoTiro >= intervalo)
        {
            tempoTiro = 0;
            Disparar(tipo);
        }
    }

    private void Disparar(int tipo)
    {
        if (jogador == null) return;
        int quantidade = tipo < 3 ? 1 : tipo < 6 ? 3 : 5;
        float abertura = tipo < 3 ? 0 : tipo < 6 ? 20 : 35;
        Vector2 baseDir = (jogador.position - transform.position).normalized;
        for (int i = 0; i < quantidade; i++)
        {
            float angulo = quantidade == 1 ? 0 : Mathf.Lerp(-abertura, abertura, (float)i / (quantidade - 1));
            Vector2 direcao = Quaternion.Euler(0, 0, angulo) * baseDir;
            GameObject bala = new GameObject("Tiro Chefao");
            bala.transform.position = transform.position + Vector3.down * .65f;
            var sprite = bala.AddComponent<SpriteRenderer>();
            sprite.sprite = spriteBalaJogador != null ? spriteBalaJogador : SpritePadrao();
            sprite.color = tipo < 3 ? Color.yellow : tipo < 6 ? new Color(1, .5f, 0) : Color.red;
            bala.transform.localScale = escalaBalaJogador;
            bala.transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(direcao.x, direcao.y, 0f));
            var col = bala.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            var corpo = bala.AddComponent<Rigidbody2D>();
            corpo.bodyType = RigidbodyType2D.Kinematic;
            var proj = bala.AddComponent<TiroChefao>();
            proj.Iniciar(direcao, tipo < 3 ? 4f : tipo < 6 ? 5.5f : 7f);
        }
    }

    private static Sprite spriteCache;
    private static Sprite SpritePadrao()
    {
        if (spriteCache == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            spriteCache = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 1);
        }
        return spriteCache;
    }

    public void Encerrar()
    {
        if (visual != null) visual.color = corOriginal;
    }
}
