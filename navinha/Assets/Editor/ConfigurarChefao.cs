#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ConfigurarChefao
{
    [MenuItem("Ferramentas/Configurar vida do chefao")]
    public static void Configurar()
    {
        const string caminho = "Assets/Scenes/Jogo.unity";
        Scene atual = SceneManager.GetActiveScene();
        if (atual.path != caminho && atual.isDirty)
        {
            Debug.LogWarning("Salve sua cena antes de configurar o chefao.");
            return;
        }
        Scene cena = atual.path == caminho ? atual : EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
        ChefaoAlvo chefe = Object.FindFirstObjectByType<ChefaoAlvo>();
        if (chefe == null)
        {
            GameObject obj = GameObject.Find("Chefão") ?? GameObject.Find("Chefao");
            if (obj == null) { Debug.LogError("Chefao nao encontrado na cena."); return; }
            chefe = obj.AddComponent<ChefaoAlvo>();
        }
        Transform t = chefe.transform;
        t.position = new Vector3(0, 3.25f, t.position.z);
        if (chefe.GetComponent<Collider2D>() == null) chefe.gameObject.AddComponent<BoxCollider2D>();
        SerializedObject dados = new SerializedObject(chefe);
        dados.FindProperty("vidaMaxima").intValue = 180;
        dados.ApplyModifiedPropertiesWithoutUndo();

        BarraVidaChefao existente = Object.FindFirstObjectByType<BarraVidaChefao>();
        if (existente == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject c = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = c.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            GameObject raiz = new GameObject("Vida Chefao", typeof(RectTransform), typeof(BarraVidaChefao));
            raiz.transform.SetParent(canvas.transform, false);
            RectTransform rt = (RectTransform)raiz.transform;
            rt.anchorMin = new Vector2(.5f, 1);
            rt.anchorMax = new Vector2(.5f, 1);
            rt.pivot = new Vector2(.5f, 1);
            rt.anchoredPosition = new Vector2(0, -24);
            rt.sizeDelta = new Vector2(320, 28);

            GameObject fundo = new GameObject("Fundo", typeof(RectTransform), typeof(Image));
            fundo.transform.SetParent(raiz.transform, false);
            RectTransform fr = (RectTransform)fundo.transform;
            fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
            fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;
            fundo.GetComponent<Image>().color = new Color(.12f, .12f, .16f, 1);

            GameObject enchimento = new GameObject("Preenchimento", typeof(RectTransform), typeof(Image));
            enchimento.transform.SetParent(raiz.transform, false);
            RectTransform er = (RectTransform)enchimento.transform;
            er.anchorMin = Vector2.zero; er.anchorMax = Vector2.one;
            er.offsetMin = new Vector2(3, 3); er.offsetMax = new Vector2(-3, -3);
            Image imagem = enchimento.GetComponent<Image>();
            imagem.color = Color.green;

            Slider slider = raiz.AddComponent<Slider>();
            slider.interactable = false;
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = er;
            slider.targetGraphic = imagem;
            slider.minValue = 0; slider.maxValue = 180; slider.value = 180;
            SerializedObject ui = new SerializedObject(raiz.GetComponent<BarraVidaChefao>());
            ui.FindProperty("chefao").objectReferenceValue = chefe;
            ui.FindProperty("barra").objectReferenceValue = slider;
            ui.FindProperty("preenchimento").objectReferenceValue = imagem;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log("Chefao com 180 HP e barra de vida configurados.");
    }
}
#endif
