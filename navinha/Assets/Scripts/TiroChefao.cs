using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class TiroChefao : MonoBehaviour
{
    private Action<TiroChefao> devolver;
    private Vector2 direcao;
    private float velocidade;
    private float tempoRestante;
    private bool ativo;
    private Jogador alvo;
    private Collider2D colisorAlvo;
    private SpriteRenderer spriteAlvo;

    public void Iniciar(Vector2 novaDirecao, float novaVelocidade, Action<TiroChefao> retorno)
    {
        direcao = novaDirecao.normalized;
        velocidade = novaVelocidade;
        devolver = retorno;
        tempoRestante = 7f;
        ativo = true;
        if (alvo == null) alvo = FindFirstObjectByType<Jogador>();
        if (alvo != null)
        {
            colisorAlvo = alvo.GetComponentInChildren<Collider2D>();
            spriteAlvo = alvo.GetComponentInChildren<SpriteRenderer>();
        }
        transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(direcao.x, direcao.y, 0f));
    }

    private void Update()
    {
        if (!ativo) return;
        Vector2 anterior = transform.position;
        Vector2 proxima = anterior + direcao * velocidade * Time.deltaTime;
        transform.position = proxima;
        // Verifica o trajeto percorrido para nao perder acertos entre frames,
        // inclusive quando a navinha nao possui Collider2D configurado.
        if (alvo != null && !alvo.Derrotado)
        {
            Bounds limites = colisorAlvo != null ? colisorAlvo.bounds :
                spriteAlvo != null ? spriteAlvo.bounds :
                new Bounds(alvo.transform.position, new Vector3(.6f, .6f, 1f));
            Vector2 centro = limites.center;
            Vector2 segmento = proxima - anterior;
            float fator = segmento.sqrMagnitude > .00001f ?
                Mathf.Clamp01(Vector2.Dot(centro - anterior, segmento) / segmento.sqrMagnitude) : 0f;
            Vector2 ponto = anterior + segmento * fator;
            float raio = Mathf.Max(.12f, Mathf.Min(limites.extents.x, limites.extents.y) * .75f);
            if (Vector2.Distance(ponto, centro) <= raio)
            {
                alvo.ReceberDano();
                Recolher();
                return;
            }
        }
        tempoRestante -= Time.deltaTime;
        if (tempoRestante <= 0f) Recolher();
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!ativo) return;
        // Balas inimigas nao colidem com balas aliadas nem com o chefao.
        if (outro.TryGetComponent<Jogador>(out var jogador))
        {
            jogador.ReceberDano();
            Recolher();
        }
    }

    private void Recolher()
    {
        if (!ativo) return;
        ativo = false;
        devolver?.Invoke(this);
    }

    public void Desativar() { ativo = false; devolver = null; }
}
