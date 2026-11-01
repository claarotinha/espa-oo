#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CriarFaseInicial
{
    [MenuItem("Ferramentas/Preparar fase inicial")]
    public static void Preparar()
    {
        const string pasta = "Assets/Scenes";
        if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);
        CriarBoot(pasta + "/_Boot.unity");
        CriarJogo(pasta + "/Jogo.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(pasta + "/_Boot.unity", true),
            new EditorBuildSettingsScene(pasta + "/Jogo.unity", true)
        };
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Fase pronta", "Abra Assets/Scenes/_Boot e pressione Play.", "OK");
    }

    private static void CriarBoot(string caminho)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Inicializador").AddComponent<InicioJogo>();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), caminho);
    }

    private static Sprite SpriteQuadrado()
    {
        Texture2D tex = Texture2D.whiteTexture;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 1);
    }

    private static GameObject ObjetoVisual(string nome, Vector3 posicao, Vector3 escala, Color cor)
    {
        var obj = new GameObject(nome);
        obj.transform.position = posicao;
        obj.transform.localScale = escala;
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteQuadrado();
        sr.color = cor;
        return obj;
    }

    private static void CriarJogo(string caminho)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera camera = cam.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.03f, .04f, .12f);
        cam.transform.position = new Vector3(0, 0, -10);
        cam.AddComponent<AudioListener>();

        GameObject jogador = ObjetoVisual("Navinha", new Vector3(0, -3.4f, 0), new Vector3(.7f, .7f, 1), Color.cyan);
        jogador.tag = "Player";
        jogador.AddComponent<Jogador>();
        jogador.AddComponent<DisparoJogador>();
        var saida = new GameObject("Saida Tiro").transform;
        saida.SetParent(jogador.transform);
        saida.localPosition = new Vector3(0, .65f, 0);

        var tiro = ObjetoVisual("Bala (modelo)", Vector3.zero, new Vector3(.14f, .35f, 1), Color.yellow);
        var corpo = tiro.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;
        var col = tiro.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        var componente = tiro.AddComponent<TiroJogador>();
        const string pasta = "Assets/Prefabs";
        if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);
        var prefab = PrefabUtility.SaveAsPrefabAsset(tiro, pasta + "/BalaJogador.prefab");
        Object.DestroyImmediate(tiro);
        var disparo = jogador.GetComponent<DisparoJogador>();
        var so = new SerializedObject(disparo);
        so.FindProperty("prefabBala").objectReferenceValue = prefab.GetComponent<TiroJogador>();
        so.FindProperty("saidaTiro").objectReferenceValue = saida;
        so.ApplyModifiedPropertiesWithoutUndo();

        var chefao = ObjetoVisual("Chefao (alvo provisório)", new Vector3(0, 3.25f, 0), new Vector3(2.2f, 1.2f, 1), Color.red);
        chefao.AddComponent<BoxCollider2D>();
        chefao.AddComponent<ChefaoAlvo>();

        var canvasObj = new GameObject("Interface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var painel = new GameObject("Vidas", typeof(RectTransform), typeof(VidasUI));
        painel.transform.SetParent(canvasObj.transform, false);
        var rect = (RectTransform)painel.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(24, -24);
        rect.sizeDelta = new Vector2(150, 40);
        var icones = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var obj = new GameObject("Vida " + (i + 1), typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(painel.transform, false);
            var r = (RectTransform)obj.transform;
            r.anchorMin = r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(i * 42, 0);
            r.sizeDelta = new Vector2(30, 30);
            icones[i] = obj.GetComponent<Image>();
            icones[i].color = Color.cyan;
        }
        var ui = new SerializedObject(painel.GetComponent<VidasUI>());
        ui.FindProperty("jogador").objectReferenceValue = jogador.GetComponent<Jogador>();
        var arr = ui.FindProperty("icones");
        arr.arraySize = 3;
        for (int i = 0; i < 3; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = icones[i];
        ui.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), caminho);
    }
}
#endif
