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

    private void Awake()
    {
        jogador = GetComponent<Jogador>();
        pool = new ObjectPool<TiroJogador>(
            () => Instantiate(prefabBala),
            bala => bala.gameObject.SetActive(true),
            bala => bala.gameObject.SetActive(false),
            bala => Destroy(bala.gameObject),
            true, 20, 100
        );
    }

    private void Update()
    {
        if (Keyboard.current == null || prefabBala == null || (jogador != null && jogador.Derrotado)) return;
        bool pressionado = Keyboard.current.spaceKey.isPressed;
        if (pressionado && (!botaoAnterior || Time.time >= proximoDisparo))
        {
            Atirar();
            proximoDisparo = Time.time + intervalo;
        }
        botaoAnterior = pressionado;
    }

    private void Atirar()
    {
        TiroJogador bala = pool.Get();
        bala.transform.position = saidaTiro != null ? saidaTiro.position : transform.position + Vector3.up * 0.6f;
        bala.transform.rotation = Quaternion.identity;
        bala.Preparar(b => pool.Release(b), velocidadeBala);
    }

    private void OnDestroy()
    {
        pool?.Clear();
    }
}
