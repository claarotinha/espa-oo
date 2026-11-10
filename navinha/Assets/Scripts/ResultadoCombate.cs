using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Apenas controla elementos ja existentes no Canvas da cena.
public class ResultadoCombate : MonoBehaviour
{
    [SerializeField] private GameObject painel;
    [SerializeField] private TMP_Text titulo;
    [SerializeField] private TMP_Text subtitulo;
    [SerializeField] private Button botaoReiniciar;
    [SerializeField] private Image fundo;
    [SerializeField] private Image fundoBotao;
    private Jogador jogador;
    private ChefaoAlvo chefao;
    private bool terminou;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (painel != null) painel.SetActive(false);
    }

    private void Start() { Conectar(); }

    private void Conectar()
    {
        if (jogador != null) jogador.DerrotaOcorrida -= AoPerder;
        if (chefao != null) chefao.VidaAlterada -= AoAlterarVida;
        jogador = FindAnyObjectByType<Jogador>();
        chefao = FindAnyObjectByType<ChefaoAlvo>();
        if (jogador != null) jogador.DerrotaOcorrida += AoPerder;
        if (chefao != null) chefao.VidaAlterada += AoAlterarVida;
        if (botaoReiniciar != null)
        {
            botaoReiniciar.onClick.RemoveListener(Reiniciar);
            botaoReiniciar.onClick.AddListener(Reiniciar);
        }
    }

    private void Update()
    {
        if (terminou) return;
        if (jogador == null || chefao == null) Conectar();
        if (chefao != null && chefao.Vida <= 0) Mostrar(true);
        else if (jogador != null && jogador.Derrotado) Mostrar(false);
    }

    private void AoAlterarVida(ChefaoAlvo alvo)
    {
        if (alvo.Vida <= 0) Mostrar(true);
    }
    private void AoPerder(Jogador alvo) { Mostrar(false); }

    private void Mostrar(bool vitoria)
    {
        if (terminou) return;
        if (painel == null || titulo == null || subtitulo == null || botaoReiniciar == null)
        {
            Debug.LogError("ResultadoCombate: faltam referencias do Painel Resultado no Canvas.", this);
            return;
        }
        terminou = true;
        titulo.text = vitoria ? "VITORIA!" : "DERROTA!";
        titulo.color = vitoria ? new Color(.6f, 1f, .75f) : new Color(1f, .65f, .65f);
        subtitulo.text = vitoria ? "VOCE DERROTOU O CHEFAO!" : "SUA NAVINHA FOI DESTRUIDA";
        if (fundo != null) fundo.color = vitoria ? new Color(.025f, .14f, .13f, .97f) : new Color(.14f, .035f, .07f, .97f);
        if (fundoBotao != null) fundoBotao.color = vitoria ? new Color(.13f, .52f, .38f) : new Color(.55f, .16f, .22f);
        painel.transform.SetAsLastSibling();
        painel.SetActive(true);
        if (chefao != null)
        {
            ChefaoCombate combate = chefao.GetComponent<ChefaoCombate>();
            if (combate != null) combate.EncerrarCombate();
        }
        Debug.Log(vitoria ? "Resultado: VITORIA" : "Resultado: DERROTA", this);
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        if (jogador != null) jogador.DerrotaOcorrida -= AoPerder;
        if (chefao != null) chefao.VidaAlterada -= AoAlterarVida;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        Scene cena = SceneManager.GetActiveScene();
        if (cena.buildIndex >= 0) SceneManager.LoadScene(cena.buildIndex);
        else SceneManager.LoadScene(cena.path);
    }
}
