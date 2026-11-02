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

    private void Awake()
    {
        // O Slider nao pode controlar o mesmo RectTransform que ajustamos manualmente.
        if (barra != null) barra.fillRect = null;
    }

    private void LateUpdate()
    {
        if (chefao == null) return;
        if (barra != null)
        {
            barra.minValue = 0;
            barra.maxValue = chefao.VidaMaxima;
            barra.SetValueWithoutNotify(chefao.Vida);
        }
        if (preenchimento == null) return;

        float percentual = Mathf.Clamp01(chefao.PercentualVida);
        RectTransform area = preenchimento.rectTransform;
        area.anchorMin = new Vector2(0f, 0f);
        area.anchorMax = new Vector2(percentual, 1f);
        area.offsetMin = new Vector2(3f, 3f);
        area.offsetMax = new Vector2(percentual > 0f ? -3f : 0f, -3f);
        preenchimento.enabled = percentual > 0f;
        preenchimento.color = chefao.Fase == FaseVidaChefao.Alta ? alta :
                              chefao.Fase == FaseVidaChefao.Media ? media : baixa;
    }
}
