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
    private bool combateEncerrado;
    private bool emTransicao;
    private Coroutine rotinaTransicao;
    private ChefaoAlvo vida;
    private Animator animator;
    private Transform jogador;
    private Vector3 origem;
    private float tempoTiro;
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
        // O Controller pode estar no objeto visual filho do chefao.
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            Animator[] animadores = GetComponentsInChildren<Animator>(true);
            foreach (Animator candidato in animadores)
                if (candidato.runtimeAnimatorController != null) { animator = candidato; break; }
        }
        visual = GetComponent<SpriteRenderer>();
        if (visual != null) corOriginal = visual.color;
        origem = transform.position;
    }

    private void Start()
    {
        Jogador p = FindAnyObjectByType<Jogador>();
        if (p != null) jogador = p.transform;
        DisparoJogador disparo = FindAnyObjectByType<DisparoJogador>();
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
        vida.VidaAlterada += AoAlterarVida;
        faseAnterior = IndiceFase();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Chefao: atribua o Animator Controller com os nove estados.", this);
            enabled = false;
            return;
        }
        animator.SetInteger("FaseVida", 0);
    }

    public void EncerrarCombate()
    {
        combateEncerrado = true;
        if (rotinaTransicao != null) { StopCoroutine(rotinaTransicao); rotinaTransicao = null; }
        emTransicao = false;
        vida.DefinirInvulnerabilidade(false);
        if (animator != null) animator.speed = 0f;
    }

    private void AoAlterarVida(ChefaoAlvo alvo)
    {
        if (combateEncerrado) return;
        if (alvo.Vida <= 0) { EncerrarCombate(); return; }
        int fase = IndiceFase();
        if (fase != faseAnterior && !emTransicao)
            rotinaTransicao = StartCoroutine(Transicionar());
    }

    public bool PodeAgir => !combateEncerrado && !emTransicao && vida != null && vida.Vida > 0 && pool != null;
    public float Amplitude => amplitudeMovimento;
    public Vector3 Origem => origem;

    public void IniciarEstado(int numero)
    {
        tempoTiro = 0f;
    }

    public void Mover(Vector3 destino)
    {
        if (!PodeAgir) return;
        transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocidadeSuavizacao, .28f, 12f);
        if (visual != null)
            visual.color = Color.Lerp(corOriginal, Color.white, .12f + .18f * Mathf.Abs(Mathf.Sin(Time.time * 2f)));
    }

    public void Atacar(float intervalo, int quantidade, float abertura, float velocidade, Color cor)
    {
        if (!PodeAgir) return;
        tempoTiro += Time.deltaTime;
        if (tempoTiro < intervalo) return;
        tempoTiro = 0f;
        Disparar(quantidade, abertura, velocidade, cor);
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
        faseAnterior = IndiceFase();
        animator.SetInteger("FaseVida", faseAnterior);
        animator.SetTrigger("MudarFase");
    }

    private int IndiceFase() => vida.Fase == FaseVidaChefao.Alta ? 0 :
                                 vida.Fase == FaseVidaChefao.Media ? 1 : 2;

    private void Disparar(int quantidade, float abertura, float velocidade, Color cor)
    {
        if (!PodeAgir || jogador == null || prefabTiro == null) return;
        Vector2 baseDir = (jogador.position - transform.position).normalized;
        for (int i = 0; i < quantidade; i++)
        {
            float angulo = quantidade == 1 ? 0 : Mathf.Lerp(-abertura, abertura, (float)i / (quantidade - 1));
            Vector2 direcao = Quaternion.Euler(0, 0, angulo) * baseDir;
            if (pool.CountInactive == 0) break;
            TiroChefao bala = pool.Get();
            if (bala == null) continue;
            bala.transform.position = transform.position + Vector3.down * .65f;
            bala.transform.localScale = escalaBalaJogador;
            SpriteRenderer sprite = bala.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                if (spriteBalaJogador != null) sprite.sprite = spriteBalaJogador;
                sprite.color = cor;
            }
            bala.Iniciar(direcao, velocidade, DevolverTiro);
        }
    }

    private void DevolverTiro(TiroChefao tiro)
    {
        if (pool == null || tiro == null) return;
        pool.Release(tiro);
    }

    private void OnDisable()
    {
        if (vida != null) vida.VidaAlterada -= AoAlterarVida;
        if (vida != null) vida.DefinirInvulnerabilidade(false);
        if (animator != null) animator.speed = 1f;
    }

    private void OnDestroy()
    {
        // A Unity destroi os objetos da cena; nao limpar o pool aqui.
        pool = null;
    }

    public void EncerrarEstado()
    {
        if (visual != null && !emTransicao) visual.color = corOriginal;
    }
}
