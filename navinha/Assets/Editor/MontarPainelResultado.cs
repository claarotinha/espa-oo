#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Monta uma vez os objetos reais na cena e salva no arquivo Jogo.unity.
// Nenhum elemento de resultado e criado durante o Play Mode.
[InitializeOnLoad]
public static class MontarPainelResultado
{
    static MontarPainelResultado()
    {
        EditorApplication.delayCall += Preparar;
        EditorSceneManager.sceneOpened += (cena, modo) => EditorApplication.delayCall += Preparar;
    }

    [MenuItem("Ferramentas/Preparar Painel Resultado")]
    public static void Preparar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene cena = SceneManager.GetActiveScene();
        if (!cena.IsValid() || !cena.path.EndsWith("/Jogo.unity")) return;
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;
        ResultadoCombate resultado = canvas.GetComponent<ResultadoCombate>();
        if (resultado == null) resultado = canvas.gameObject.AddComponent<ResultadoCombate>();

        Transform anterior = canvas.transform.Find("Painel Resultado");
        GameObject painel;
        if (anterior == null)
        {
            painel = Criar("Painel Resultado", canvas.transform);
            Image imagem = painel.AddComponent<Image>();
            imagem.color = new Color(.14f, .035f, .07f, .97f);
            RectTransform rect = painel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        else painel = anterior.gameObject;

        TMP_Text titulo = Texto(painel.transform, "Titulo", "DERROTA!", new Vector2(0, 105), new Vector2(650, 95), 58);
        TMP_Text subtitulo = Texto(painel.transform, "Mensagem Resultado", "SUA NAVINHA FOI DESTRUIDA", new Vector2(0, 15), new Vector2(700, 65), 25);
        Transform botaoExistente = painel.transform.Find("Reiniciar");
        GameObject botaoObj = botaoExistente != null ? botaoExistente.gameObject : Criar("Reiniciar", painel.transform);
        RectTransform botaoRect = botaoObj.GetComponent<RectTransform>();
        botaoRect.anchorMin = botaoRect.anchorMax = new Vector2(.5f, .5f);
        botaoRect.pivot = new Vector2(.5f, .5f);
        botaoRect.anchoredPosition = new Vector2(0, -95);
        botaoRect.sizeDelta = new Vector2(250, 62);
        Image botaoImagem = botaoObj.GetComponent<Image>();
        if (botaoImagem == null) botaoImagem = botaoObj.AddComponent<Image>();
        botaoImagem.color = new Color(.55f, .16f, .22f);
        Button botao = botaoObj.GetComponent<Button>();
        if (botao == null) botao = botaoObj.AddComponent<Button>();
        botao.targetGraphic = botaoImagem;
        Texto(botaoObj.transform, "Texto Botao", "REINICIAR", Vector2.zero, new Vector2(220, 50), 24);

        SerializedObject serial = new SerializedObject(resultado);
        serial.FindProperty("painel").objectReferenceValue = painel;
        serial.FindProperty("titulo").objectReferenceValue = titulo;
        serial.FindProperty("subtitulo").objectReferenceValue = subtitulo;
        serial.FindProperty("botaoReiniciar").objectReferenceValue = botao;
        serial.FindProperty("fundo").objectReferenceValue = painel.GetComponent<Image>();
        serial.FindProperty("fundoBotao").objectReferenceValue = botaoImagem;
        serial.ApplyModifiedPropertiesWithoutUndo();

        // Corrige a escala zero encontrada no Canvas serializado.
        canvas.transform.localScale = Vector3.one;
        painel.transform.SetAsLastSibling();
        painel.SetActive(false);
        EditorUtility.SetDirty(resultado);
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
    }

    private static GameObject Criar(string nome, Transform pai)
    {
        GameObject obj = new GameObject(nome, typeof(RectTransform));
        obj.layer = 5;
        obj.transform.SetParent(pai, false);
        return obj;
    }

    private static TMP_Text Texto(Transform pai, string nome, string valor, Vector2 pos, Vector2 tamanho, float fonte)
    {
        Transform existente = pai.Find(nome);
        GameObject obj = existente != null ? existente.gameObject : Criar(nome, pai);
        TextMeshProUGUI texto = obj.GetComponent<TextMeshProUGUI>();
        if (texto == null) texto = obj.AddComponent<TextMeshProUGUI>();
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = tamanho;
        texto.text = valor;
        texto.fontSize = fonte;
        texto.enableAutoSizing = false;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = Color.white;
        texto.raycastTarget = false;
        return texto;
    }
}
#endif
