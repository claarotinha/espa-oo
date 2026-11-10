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
    private TMP_Text subtitulo;
    private Image fundoResultado;
    private Image fundoBotao;
    private bool terminou;
    private bool reiniciando;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegistrarCena()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private static void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        Preparar();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Preparar()
    {
        Jogador jogador = FindAnyObjectByType<Jogador>();
        ChefaoAlvo chefao = FindAnyObjectByType<ChefaoAlvo>();
        if (jogador == null || chefao == null) return;
        ResultadoCombate existente = FindAnyObjectByType<ResultadoCombate>();
        if (existente != null)
        {
            existente.Conectar();
            return;
        }
        new GameObject("Resultado do Combate").AddComponent<ResultadoCombate>();
    }

    private void Conectar()
    {
        if (jogador != null) jogador.DerrotaOcorrida -= AoPerder;
        if (chefao != null) chefao.VidaAlterada -= AoMudarVida;
        jogador = FindAnyObjectByType<Jogador>();
        chefao = FindAnyObjectByType<ChefaoAlvo>();
        combate = chefao != null ? chefao.GetComponent<ChefaoCombate>() : null;
        if (jogador != null) jogador.DerrotaOcorrida += AoPerder;
        if (chefao != null) chefao.VidaAlterada += AoMudarVida;
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        CriarInterface();
        Conectar();
    }

    private void Update()
    {
        if (terminou || reiniciando) return;
        if (chefao == null || jogador == null) Conectar();
        if (chefao != null && chefao.Vida <= 0) Mostrar(true);
        else if (jogador != null && jogador.Derrotado) Mostrar(false);
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
        if (terminou || reiniciando) return;
        if (painel == null) CriarInterface();
        if (painel == null) { Debug.LogError("ResultadoCombate: painel nao foi criado no Canvas.", this); return; }
        Debug.Log(vitoria ? "ResultadoCombate: VITORIA - vida do chefao zerada." : "ResultadoCombate: DERROTA - navinha perdeu a quarta vida.", this);
        terminou = true;
        if (combate != null) combate.EncerrarCombate();
        painel.transform.SetAsLastSibling();
        painel.SetActive(true);
        if (fundoResultado != null) fundoResultado.color = vitoria ? new Color(.025f, .14f, .13f, .97f) : new Color(.14f, .035f, .07f, .97f);
        if (fundoBotao != null) fundoBotao.color = vitoria ? new Color(.13f, .52f, .38f) : new Color(.55f, .16f, .22f);
        if (subtitulo != null) subtitulo.text = vitoria ? "VOCE DERROTOU O CHEFAO!" : "SUA NAVINHA FOI DESTRUIDA";
        if (titulo != null)
        {
            titulo.text = vitoria ? "VITORIA!" : "DERROTA!";
            titulo.color = vitoria ? new Color(.5f, 1f, .7f) : new Color(1f, .45f, .45f);
        }
        Time.timeScale = 0f;
    }

    public void TentarNovamente()
    {
        if (reiniciando) return;
        reiniciando = true;
        Time.timeScale = 1f;
        Scene cena = SceneManager.GetActiveScene();
        // Funciona tambem quando a cena e aberta diretamente no Editor
        // e nao possui indice nas Build Settings.
        if (cena.buildIndex >= 0) SceneManager.LoadScene(cena.buildIndex);
        else SceneManager.LoadScene(cena.path);
    }

    private void CriarInterface()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventos = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        // Usa o Canvas da cena, deixando o painel visivel e editavel na Hierarchy.
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("ResultadoCombate deve estar no Canvas da cena.", this);
            return;
        }
        Transform canvasObjeto = canvas.transform;

        painel = new GameObject("Painel Resultado", typeof(RectTransform), typeof(Image));
        painel.transform.SetParent(canvasObjeto, false);
        painel.transform.SetAsLastSibling();
        painel.layer = 5;
        RectTransform fundo = painel.GetComponent<RectTransform>();
        fundo.anchorMin = Vector2.zero;
        fundo.anchorMax = Vector2.one;
        fundo.offsetMin = fundo.offsetMax = Vector2.zero;
        fundoResultado = painel.GetComponent<Image>();
        fundoResultado.color = new Color(.035f, .055f, .12f, .96f);

        GameObject textoObjeto = new GameObject("Titulo", typeof(RectTransform), typeof(TextMeshProUGUI));
        textoObjeto.transform.SetParent(painel.transform, false);
        RectTransform textoRect = textoObjeto.GetComponent<RectTransform>();
        textoRect.anchorMin = textoRect.anchorMax = new Vector2(.5f, .5f);
        textoRect.sizeDelta = new Vector2(700, 100);
        textoRect.anchoredPosition = new Vector2(0, 100);
        titulo = textoObjeto.GetComponent<TextMeshProUGUI>();
        titulo.fontSize = 58;
        titulo.alignment = TextAlignmentOptions.Center;
        titulo.enableAutoSizing = true;
        titulo.fontSizeMin = 30;
        titulo.color = Color.white;
        titulo.raycastTarget = false;

        GameObject subtituloObjeto = new GameObject("Mensagem Resultado", typeof(RectTransform), typeof(TextMeshProUGUI));
        subtituloObjeto.transform.SetParent(painel.transform, false);
        RectTransform subtituloRect = subtituloObjeto.GetComponent<RectTransform>();
        subtituloRect.anchorMin = subtituloRect.anchorMax = new Vector2(.5f, .5f);
        subtituloRect.sizeDelta = new Vector2(700, 65);
        subtituloRect.anchoredPosition = new Vector2(0, 20);
        subtitulo = subtituloObjeto.GetComponent<TextMeshProUGUI>();
        subtitulo.fontSize = 26;
        subtitulo.enableAutoSizing = true;
        subtitulo.fontSizeMin = 18;
        subtitulo.color = Color.white;
        subtitulo.alignment = TextAlignmentOptions.Center;
        subtitulo.raycastTarget = false;

        GameObject botaoObjeto = new GameObject("Reiniciar", typeof(RectTransform), typeof(Image), typeof(Button));
        botaoObjeto.transform.SetParent(painel.transform, false);
        RectTransform botaoRect = botaoObjeto.GetComponent<RectTransform>();
        botaoRect.anchorMin = botaoRect.anchorMax = new Vector2(.5f, .5f);
        botaoRect.sizeDelta = new Vector2(250, 62);
        botaoRect.anchoredPosition = new Vector2(0, -95);
        Image imagemBotao = botaoObjeto.GetComponent<Image>();
        fundoBotao = imagemBotao;
        imagemBotao.color = new Color(.15f, .42f, .75f);
        Button botao = botaoObjeto.GetComponent<Button>();
        botao.targetGraphic = imagemBotao;
        botao.onClick.AddListener(TentarNovamente);

        GameObject rotuloObjeto = new GameObject("Texto Botao", typeof(RectTransform), typeof(TextMeshProUGUI));
        rotuloObjeto.transform.SetParent(botaoObjeto.transform, false);
        RectTransform rotuloRect = rotuloObjeto.GetComponent<RectTransform>();
        rotuloRect.anchorMin = Vector2.zero;
        rotuloRect.anchorMax = Vector2.one;
        rotuloRect.offsetMin = new Vector2(14, 7);
        rotuloRect.offsetMax = new Vector2(-14, -7);
        TMP_Text rotulo = rotuloObjeto.GetComponent<TextMeshProUGUI>();
        rotulo.text = "REINICIAR";
        rotulo.fontSize = 24;
        rotulo.enableAutoSizing = true;
        rotulo.fontSizeMin = 16;
        rotulo.color = Color.white;
        rotulo.alignment = TextAlignmentOptions.Center;
        rotulo.raycastTarget = false;
        painel.SetActive(false);
    }
}
