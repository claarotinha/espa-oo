#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class CorrigirFase
{
    static CorrigirFase() { EditorApplication.delayCall += Corrigir; }

    [MenuItem("Ferramentas/Conferir tiros e vidas")]
    public static void Corrigir()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        const string cena = "Assets/Scenes/Jogo.unity";
        const string caminhoBala = "Assets/Scripts/bala.prefab";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(cena) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(caminhoBala) == null) return;

        GameObject raiz = PrefabUtility.LoadPrefabContents(caminhoBala);
        if (raiz.GetComponent<BoxCollider2D>() == null) raiz.AddComponent<BoxCollider2D>();
        raiz.GetComponent<BoxCollider2D>().isTrigger = true;
        Rigidbody2D rb = raiz.GetComponent<Rigidbody2D>();
        if (rb == null) rb = raiz.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        if (raiz.GetComponent<TiroJogador>() == null) raiz.AddComponent<TiroJogador>();
        PrefabUtility.SaveAsPrefabAsset(raiz, caminhoBala);
        PrefabUtility.UnloadPrefabContents(raiz);
        GameObject bala = AssetDatabase.LoadAssetAtPath<GameObject>(caminhoBala);

        Scene atual = SceneManager.GetActiveScene();
        bool jaAberta = atual.path == cena;
        if (!jaAberta && atual.isDirty) return; // nao descartar edicoes nao salvas
        Scene jogo = jaAberta ? atual : EditorSceneManager.OpenScene(cena, OpenSceneMode.Single);
        Jogador jogador = Object.FindFirstObjectByType<Jogador>();
        if (jogador == null)
        {
            Debug.LogWarning("Nao encontrei Jogador na cena Jogo.");
            return;
        }
        DisparoJogador disparo = jogador.GetComponent<DisparoJogador>();
        if (disparo == null) disparo = jogador.gameObject.AddComponent<DisparoJogador>();
        Transform saida = jogador.transform.Find("Saida Tiro");
        if (saida == null)
        {
            saida = new GameObject("Saida Tiro").transform;
            saida.SetParent(jogador.transform, false);
            saida.localPosition = new Vector3(0, .65f, 0);
        }
        SerializedObject serial = new SerializedObject(disparo);
        serial.FindProperty("prefabBala").objectReferenceValue = bala.GetComponent<TiroJogador>();
        serial.FindProperty("saidaTiro").objectReferenceValue = saida;
        serial.ApplyModifiedPropertiesWithoutUndo();

        Camera camera = Camera.main;
        if (camera != null) { camera.orthographic = true; camera.orthographicSize = 5f; }
        ChefaoAlvo alvo = Object.FindFirstObjectByType<ChefaoAlvo>();
        if (alvo == null)
        {
            GameObject chefe = GameObject.Find("Chefão");
            if (chefe == null) chefe = GameObject.Find("Chefao");
            if (chefe != null)
            {
                if (chefe.GetComponent<Collider2D>() == null) chefe.AddComponent<BoxCollider2D>();
                chefe.AddComponent<ChefaoAlvo>();
            }
        }

        VidasUI ui = Object.FindFirstObjectByType<VidasUI>();
        if (ui == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject c = new GameObject("Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = c.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            GameObject painel = new GameObject("Vidas Jogador", typeof(RectTransform), typeof(VidasUI));
            painel.transform.SetParent(canvas.transform, false);
            RectTransform r = (RectTransform)painel.transform;
            r.anchorMin = r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(24, -24);
            r.sizeDelta = new Vector2(150, 40);
            Image[] imagens = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject icone = new GameObject("Vida " + (i + 1), typeof(RectTransform), typeof(Image));
                icone.transform.SetParent(painel.transform, false);
                RectTransform ir = (RectTransform)icone.transform;
                ir.anchorMin = ir.anchorMax = new Vector2(0, 1);
                ir.pivot = new Vector2(0, 1);
                ir.anchoredPosition = new Vector2(i * 42, 0);
                ir.sizeDelta = new Vector2(30, 30);
                imagens[i] = icone.GetComponent<Image>();
                imagens[i].color = Color.cyan;
            }
            ui = painel.GetComponent<VidasUI>();
            SerializedObject uiDados = new SerializedObject(ui);
            uiDados.FindProperty("jogador").objectReferenceValue = jogador;
            SerializedProperty lista = uiDados.FindProperty("icones");
            lista.arraySize = 3;
            for (int i = 0; i < 3; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = imagens[i];
            uiDados.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.MarkSceneDirty(jogo);
        EditorSceneManager.SaveScene(jogo);
        Debug.Log("Tiros, ObjectPool, colisao, camera e vidas configurados na cena Jogo.");
    }
}
#endif