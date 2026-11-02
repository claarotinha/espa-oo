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

    public void Iniciar(Vector2 novaDirecao, float novaVelocidade, Action<TiroChefao> retorno)
    {
        direcao = novaDirecao.normalized;
        velocidade = novaVelocidade;
        devolver = retorno;
        tempoRestante = 7f;
        ativo = true;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(direcao.x, direcao.y, 0f));
    }

    private void Update()
    {
        if (!ativo) return;
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);
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
