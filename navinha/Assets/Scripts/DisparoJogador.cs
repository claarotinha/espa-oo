using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class DisparoJogador : MonoBehaviour
{
    [SerializeField, Range(0.2f, 1f)] private float intervalo = 0.35f;
    [SerializeField] private float velocidadeBala = 10f;
    [SerializeField] private TiroJogador prefabBala;
    [SerializeField] private Transform saidaTiro;

    private ObjectPool<TiroJogador> pool;
    private float proximoDisparo;
    private bool botaoAnterior;
    private Jogador jogador;

    public Sprite SpriteBala => prefabBala != null && prefabBala.TryGetComponent<SpriteRenderer>(out var visual) ? visual.sprite : null;
    public Vector3 EscalaBala => prefabBala != null ? prefabBala.transform.localScale : new Vector3(.2f, .5f, 1f);

    private void Awake()
    {
        jogador = GetComponent<Jogador>();
        if (prefabBala == null)
        {
            Debug.LogError("DisparoJogador: atribua um prefab de bala valido no Inspector.", this);
            enabled = false;
            return;
        }

        pool = new ObjectPool<TiroJogador>(
            () => Instantiate(prefabBala),
            bala => { if (bala != null) bala.gameObject.SetActive(true); },
            bala => { if (bala != null) bala.gameObject.SetActive(false); },
            bala => { if (bala != null) Destroy(bala.gameObject); },
            true, 20, 100
        );
    }

    private void Update()
    {
        if (Keyboard.current == null || prefabBala == null || pool == null ||
            (jogador != null && jogador.Derrotado)) return;

        bool pressionado = Keyboard.current.spaceKey.isPressed;
        if (pressionado && Time.time >= proximoDisparo)
        {
            Atirar();
            proximoDisparo = Time.time + intervalo;
        }
        botaoAnterior = pressionado;
    }

    private void Atirar()
    {
        if (pool == null) return;
        TiroJogador bala = pool.Get();
        if (bala == null) return;
        bala.transform.position = saidaTiro != null ? saidaTiro.position : transform.position + Vector3.up * 0.6f;
        bala.transform.rotation = Quaternion.identity;
        bala.Preparar(DevolverBala, velocidadeBala);
    }

    private void DevolverBala(TiroJogador bala)
    {
        // Durante troca de cena/destruicao do jogador, nao reutilizar referencias invalidas.
        if (pool == null || bala == null || !isActiveAndEnabled) return;
        pool.Release(bala);
    }

    private void OnDestroy()
    {
        // Nao limpar objetos do pool durante a destruicao da cena: o Unity
        // pode ja ter destruido as balas e Clear tentaria acessa-las novamente.
        pool = null;
    }
}
