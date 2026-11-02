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
        barra.SetValueWithoutNotify(alvo.Vida);
        // Controla a largura diretamente: funciona mesmo com imagens sem sprite.
        if (preenchimento != null)
        {
            RectTransform area = preenchimento.rectTransform;
            area.anchorMin = new Vector2(0f, 0f);
            area.anchorMax = new Vector2(alvo.PercentualVida, 1f);
            area.offsetMin = new Vector2(3f, 3f);
            area.offsetMax = new Vector2(-3f, -3f);
        }
        if (preenchimento != null)
            preenchimento.color = alvo.Fase == FaseVidaChefao.Alta ? alta :
                                  alvo.Fase == FaseVidaChefao.Media ? media : baixa;
    }
}
