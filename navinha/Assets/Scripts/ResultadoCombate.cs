using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Interface criada automaticamente na cena Jogo, sem configuracao manual.
public class ResultadoCombate : MonoBehaviour
{
    private Jogador jogador;
    private ChefaoAlvo chefao;
    private ChefaoCombate combate;
    private GameObject painel;
    private TMP_Text titulo;
    private bool terminou;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Preparar()
    {
        if (FindFirstObjectByType<ResultadoCombate>() != null) return;
        Jogador jogador = FindFirstObjectByType<Jogador>();
        ChefaoAlvo chefao = FindFirstObjectByType<ChefaoAlvo>();
        if (jogador == null || chefao == null) return;
        new GameObject("Resultado do Combate").AddComponent<ResultadoCombate>();
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        jogador = FindFirstObjectByType<Jogador>();
        chefao = FindFirstObjectByType<ChefaoAlvo>();
        combate = chefao != null ? chefao.GetComponent<ChefaoCombate>() : null;
        CriarInterface();
    }

    private void OnEnable()
    {
        if (jogador != null) jogador.DerrotaOcorrida += AoPerder;
        if (chefao != null) chefao.VidaAlterada += AoMudarVida;
    }

    private void Start()
    {
        if (chefao != null && chefao.Vida <= 0) Mostrar(true);
        else if (jogador != null && jogador.Derrotado) Mostrar(false);
    }

    private void OnDisable()
    {
        if (jogador != null) jogador.DerrotaOcorrida -= AoPerder;
        if (chefao != null) chefao.VidaAlterada -= AoMudarVida;
    }

    private void AoPerder(Jogador alvo) { Mostrar(false); }
    private void AoMudarVida(ChefaoAlvo alvo)
    {
        if (alvo.Vida <= 0) Mostrar(true);
    }

    private void Mostrar(bool vitoria)
    {
        if (terminou) return;
        terminou = true;
        if (combate != null) combate.EncerrarCombate();
        if (painel != null) painel.SetActive(true);
        if (titulo != null)
        {
            titulo.text = vitoria ? "VITORIA!" : "DERROTA!";
            titulo.color = vitoria ? new Color(.5f, 1f, .7f) : new Color(1f, .45f, .45f);
        }
        Time.timeScale = 0f;
    }

    public void TentarNovamente()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CriarInterface()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventos = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        GameObject canvasObjeto = new GameObject("Canvas Resultado", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObjeto.transform.SetParent(transform, false);
        Canvas canvas = canvasObjeto.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObjeto.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        painel = new GameObject("Painel Resultado", typeof(RectTransform), typeof(Image));
        painel.transform.SetParent(canvasObjeto.transform, false);
        RectTransform fundo = painel.GetComponent<RectTransform>();
        fundo.anchorMin = Vector2.zero;
        fundo.anchorMax = Vector2.one;
        fundo.offsetMin = fundo.offsetMax = Vector2.zero;
        painel.GetComponent<Image>().color = new Color(.04f, .06f, .14f, .94f);

        GameObject textoObjeto = new GameObject("Titulo", typeof(RectTransform), typeof(TextMeshProUGUI));
        textoObjeto.transform.SetParent(painel.transform, false);
        RectTransform textoRect = textoObjeto.GetComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(.15f, .52f);
        textoRect.anchorMax = new Vector2(.85f, .7f);
        textoRect.offsetMin = textoRect.offsetMax = Vector2.zero;
        titulo = textoObjeto.GetComponent<TextMeshProUGUI>();
        titulo.fontSize = 90;
        titulo.alignment = TextAlignmentOptions.Center;
        titulo.enableAutoSizing = true;
        titulo.fontSizeMin = 30;

        GameObject botaoObjeto = new GameObject("Tentar Novamente", typeof(RectTransform), typeof(Image), typeof(Button));
        botaoObjeto.transform.SetParent(painel.transform, false);
        RectTransform botaoRect = botaoObjeto.GetComponent<RectTransform>();
        botaoRect.anchorMin = botaoRect.anchorMax = new Vector2(.5f, .39f);
        botaoRect.sizeDelta = new Vector2(440, 110);
        botaoRect.anchoredPosition = Vector2.zero;
        Image imagemBotao = botaoObjeto.GetComponent<Image>();
        imagemBotao.color = new Color(.15f, .42f, .75f);
        Button botao = botaoObjeto.GetComponent<Button>();
        botao.targetGraphic = imagemBotao;
        botao.onClick.AddListener(TentarNovamente);

        GameObject rotuloObjeto = new GameObject("Texto Botao", typeof(RectTransform), typeof(TextMeshProUGUI));
        rotuloObjeto.transform.SetParent(botaoObjeto.transform, false);
        RectTransform rotuloRect = rotuloObjeto.GetComponent<RectTransform>();
        rotuloRect.anchorMin = Vector2.zero;
        rotuloRect.anchorMax = Vector2.one;
        rotuloRect.offsetMin = rotuloRect.offsetMax = Vector2.zero;
        TMP_Text rotulo = rotuloObjeto.GetComponent<TextMeshProUGUI>();
        rotulo.text = "TENTAR NOVAMENTE";
        rotulo.fontSize = 42;
        rotulo.enableAutoSizing = true;
        rotulo.fontSizeMin = 18;
        rotulo.color = Color.white;
        rotulo.alignment = TextAlignmentOptions.Center;
        rotulo.raycastTarget = false;
        painel.SetActive(false);
    }
}
