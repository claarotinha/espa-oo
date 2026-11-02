using System.Collections;
using UnityEngine.Pool;
using UnityEngine;

// 0-2: vida alta; 3-5: media; 6-8: baixa.
[RequireComponent(typeof(ChefaoAlvo), typeof(Animator))]
public class ChefaoCombate : MonoBehaviour
{
    [SerializeField] private float duracaoComportamento = 3.5f;
    [SerializeField, Min(2f)] private float duracaoTransicao = 2.5f;
    [SerializeField] private Color corInvulneravel = new Color(.35f, .9f, 1f, 1f);
    [SerializeField] private float amplitudeMovimento = 2.6f;
    [SerializeField] private TiroChefao prefabTiro;
    [SerializeField, Min(5)] private int quantidadeInicial = 45;
    [SerializeField, Min(10)] private int limiteTiros = 120;
    private ObjectPool<TiroChefao> pool;
    private bool emTransicao;
    private Coroutine rotinaTransicao;
    private ChefaoAlvo vida;
    private Animator animator;
    private Transform jogador;
    private Vector3 origem;
    private float tempoEstado, tempoTiro;
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
        if (prefabTiro == null)
        {
            Debug.LogError("ChefaoCombate: prefabTiro nao configurado.", this);
            enabled = false;
            return;
        }
        pool = new ObjectPool<TiroChefao>(
            () => Instantiate(prefabTiro),
            tiro => { if (tiro != null) tiro.gameObject.SetActive(true); },
            tiro => { if (tiro != null) { tiro.Desativar(); tiro.gameObject.SetActive(false); } },
            tiro => { if (tiro != null) Destroy(tiro.gameObject); },
            true, quantidadeInicial, limiteTiros);
        // Prepara objetos antes do combate para evitar alocacoes nos ataques.
        TiroChefao[] reserva = new TiroChefao[quantidadeInicial];
        for (int i = 0; i < reserva.Length; i++) reserva[i] = pool.Get();
        for (int i = 0; i < reserva.Length; i++) pool.Release(reserva[i]);
        Trocar(true);
    }

    private void Update()
    {
        if (vida.Vida <= 0)
        {
            if (rotinaTransicao != null) { StopCoroutine(rotinaTransicao); rotinaTransicao = null; }
            emTransicao = false;
            vida.DefinirInvulnerabilidade(false);
            animator.speed = 0;
            return;
        }
        if (emTransicao) return;
        int fase = IndiceFase();
        if (fase != faseAnterior)
        {
            rotinaTransicao = StartCoroutine(Transicionar());
            return;
        }
        if (tempoEstado >= duracaoComportamento) Trocar(false);
    }

    private IEnumerator Transicionar()
    {
        emTransicao = true;
        vida.DefinirInvulnerabilidade(true);
        animator.speed = 0f;
        velocidadeSuavizacao = Vector3.zero;
        float tempo = 0f;
        while (tempo < duracaoTransicao)
        {
            tempo += Time.deltaTime;
            if (visual != null)
            {
                float pulso = Mathf.PingPong(tempo * 6f, 1f);
                visual.color = Color.Lerp(corOriginal, corInvulneravel, pulso);
            }
            yield return null;
        }
        if (visual != null) visual.color = corOriginal;
        vida.DefinirInvulnerabilidade(false);
        animator.speed = 1f;
        emTransicao = false;
        rotinaTransicao = null;
        Trocar(true);
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
        tempoEstado = 0;
        tempoTiro = 0;
    }

    public void Executar(int tipo)
    {
        if (vida.Vida <= 0 || emTransicao) return;
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
            // Nunca instanciar projeteis durante o ataque: usar apenas a reserva.
            if (pool.CountInactive == 0) break;
            TiroChefao bala = pool.Get();
            if (bala == null) continue;
            bala.transform.position = transform.position + Vector3.down * .65f;
            bala.transform.localScale = escalaBalaJogador;
            SpriteRenderer sprite = bala.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                if (spriteBalaJogador != null) sprite.sprite = spriteBalaJogador;
                sprite.color = tipo < 3 ? Color.yellow : tipo < 6 ? new Color(1f, .5f, 0f) : Color.red;
            }
            bala.Iniciar(direcao, tipo < 3 ? 4f : tipo < 6 ? 5.5f : 7f, DevolverTiro);
        }
    }

    private void DevolverTiro(TiroChefao tiro)
    {
        if (pool == null || tiro == null) return;
        pool.Release(tiro);
    }

    private void OnDisable()
    {
        if (vida != null) vida.DefinirInvulnerabilidade(false);
        if (animator != null) animator.speed = 1f;
    }

    private void OnDestroy()
    {
        // A Unity destroi os objetos da cena; nao limpar o pool aqui.
        pool = null;
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
        if (visual != null && !emTransicao) visual.color = corOriginal;
    }
}
