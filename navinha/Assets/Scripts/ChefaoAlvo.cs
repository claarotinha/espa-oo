using UnityEngine;
using System;

public enum FaseVidaChefao { Alta, Media, Baixa, Derrotado }

public class ChefaoAlvo : MonoBehaviour
{
    [SerializeField, Min(1)] private int vidaMaxima = 180;
    [SerializeField] private int vidaAtual;
    public bool Invulneravel { get; private set; }
    public int Vida => vidaAtual;
    public int VidaMaxima => vidaMaxima;
    public float PercentualVida => (float)vidaAtual / vidaMaxima;
    public FaseVidaChefao Fase { get; private set; }
    public event Action<ChefaoAlvo> VidaAlterada;

    private void Awake()
    {
        vidaAtual = vidaMaxima;
        AtualizarFase();
    }

    public void ReceberDano(int dano)
    {
        if (vidaAtual <= 0 || dano <= 0 || Invulneravel) return;
        vidaAtual = Mathf.Max(0, vidaAtual - dano);
        AtualizarFase();
        VidaAlterada?.Invoke(this);
        if (vidaAtual == 0) Debug.Log("ChefaoAlvo: vida chegou a ZERO; evento de vitoria enviado.", this);
    }

    public void DefinirInvulnerabilidade(bool valor)
    {
        Invulneravel = valor && vidaAtual > 0;
    }

    private void AtualizarFase()
    {
        // Limites sem lacunas: 50% pertence a Media e 20% pertence a Baixa.
        Fase = vidaAtual <= 0 ? FaseVidaChefao.Derrotado :
               PercentualVida <= .2f ? FaseVidaChefao.Baixa :
               PercentualVida <= .5f ? FaseVidaChefao.Media :
               FaseVidaChefao.Alta;
    }
}
