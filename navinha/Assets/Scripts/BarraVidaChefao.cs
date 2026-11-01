using UnityEngine;
using UnityEngine.UI;

public class BarraVidaChefao : MonoBehaviour
{
    [SerializeField] private ChefaoAlvo chefao;
    [SerializeField] private Slider barra;
    [SerializeField] private Image preenchimento;
    [SerializeField] private Color alta = new Color(.15f, .8f, .25f);
    [SerializeField] private Color media = new Color(1f, .72f, .12f);
    [SerializeField] private Color baixa = new Color(.95f, .18f, .18f);

    private void OnEnable()
    {
        if (chefao != null) chefao.VidaAlterada += Atualizar;
        Atualizar(chefao);
    }
    private void OnDisable()
    {
        if (chefao != null) chefao.VidaAlterada -= Atualizar;
    }
    private void Atualizar(ChefaoAlvo alvo)
    {
        if (alvo == null || barra == null) return;
        barra.minValue = 0;
        barra.maxValue = alvo.VidaMaxima;
        barra.value = alvo.Vida;
        if (preenchimento != null)
            preenchimento.color = alvo.Fase == FaseVidaChefao.Alta ? alta :
                                  alvo.Fase == FaseVidaChefao.Media ? media : baixa;
    }
}
