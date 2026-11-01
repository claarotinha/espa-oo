using System;
using UnityEngine;

public class TiroJogador : MonoBehaviour
{
    private Action<TiroJogador> devolver;
    private float velocidade;
    private Camera cameraJogo;
    private bool ativo;

    public void Preparar(Action<TiroJogador> retorno, float novaVelocidade)
    {
        devolver = retorno;
        velocidade = novaVelocidade;
        cameraJogo = Camera.main;
        ativo = true;
    }

    private void Update()
    {
        if (!ativo) return;
        transform.position += Vector3.up * (velocidade * Time.deltaTime);
        if (cameraJogo == null) { Recolher(); return; }
        Vector3 ponto = cameraJogo.WorldToViewportPoint(transform.position);
        if (ponto.y > 1.1f || ponto.x < -0.1f || ponto.x > 1.1f) Recolher();
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!ativo || outro.CompareTag("Player")) return;
        // A integracao com a vida do chefao sera implementada na parte 4.
        if (outro.TryGetComponent<ChefaoAlvo>(out var alvo)) alvo.ReceberDano(1);
        Recolher();
    }

    private void Recolher()
    {
        if (!ativo) return;
        ativo = false;
        devolver?.Invoke(this);
    }
}
