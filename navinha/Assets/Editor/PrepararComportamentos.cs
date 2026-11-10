#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Configura automaticamente os assets e a cena, sem comandos no menu Ferramentas.
[InitializeOnLoad]
public static class PrepararComportamentos
{
    private const string Pasta = "Assets/Animacoes";
    private const string Caminho = Pasta + "/Chefao.controller";
    private const string Cena = "Assets/Scenes/Jogo.unity";
    static PrepararComportamentos()
    {
        EditorApplication.delayCall += Preparar;
        EditorSceneManager.sceneOpened += (scene, mode) => EditorApplication.delayCall += Preparar;
    }

    private static void Preparar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (!AssetDatabase.IsValidFolder(Pasta)) AssetDatabase.CreateFolder("Assets", "Animacoes");
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Caminho);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(Caminho);

        var maquina = controller.layers[0].stateMachine;
        bool estadosAlterados = false;
        for (int i = 0; i < 9; i++)
        {
            string nome = "Comportamento" + (i + 1);
            AnimatorState estado = null;
            foreach (var filho in maquina.states)
                if (filho.state.name == nome) { estado = filho.state; break; }
            if (estado == null)
            {
                estado = maquina.AddState(nome, new Vector3((i % 3) * 240, (i / 3) * 100, 0));
                estadosAlterados = true;
            }
            // Estados sem Motion podem nao ser avaliados continuamente pelo Animator.
            string caminhoClip = Pasta + "/Estado" + (i + 1).ToString("00") + ".anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(caminhoClip);
            if (clip == null)
            {
                clip = new AnimationClip();
                clip.name = "Estado" + (i + 1).ToString("00");
                clip.frameRate = 30f;
                clip.wrapMode = WrapMode.Loop;
                // Clip vazio: nao sobrescreve a posicao controlada pelo script.
                AssetDatabase.CreateAsset(clip, caminhoClip);
            }
            if (estado.motion != clip)
            {
                estado.motion = clip;
                estadosAlterados = true;
            }
            System.Type esperado = System.Type.GetType("Comportamento" + (i + 1).ToString("00") + ", Assembly-CSharp");
            if (esperado == null) continue; // Aguarda a compilacao/importacao dos scripts de estado.
            bool correto = estado.behaviours.Length == 1 && estado.behaviours[0] != null &&
                           estado.behaviours[0].GetType() == esperado;
            if (!correto)
            {
                foreach (var antigo in estado.behaviours)
                    if (antigo != null) Object.DestroyImmediate(antigo, true);
                estado.AddStateMachineBehaviour(esperado);
                estadosAlterados = true;
            }
            if (i == 0 && maquina.defaultState != estado)
            {
                maquina.defaultState = estado;
                estadosAlterados = true;
            }
        }
        if (estadosAlterados) AssetDatabase.SaveAssets();

        Scene atual = SceneManager.GetActiveScene();
        if (atual.path != Cena) return;
        GameObject chefe = GameObject.Find("Chefão") ?? GameObject.Find("Chefao") ?? GameObject.Find("Chefao (alvo provisório)");
        if (chefe == null) return;
        Animator animator = chefe.GetComponent<Animator>();
        if (animator == null) animator = chefe.AddComponent<Animator>();
        bool mudou = animator.runtimeAnimatorController != controller;
        if (mudou) animator.runtimeAnimatorController = controller;
        if (animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            mudou = true;
        }
        if (chefe.GetComponent<ChefaoCombate>() == null)
        {
            chefe.AddComponent<ChefaoCombate>();
            mudou = true;
        }
        if (mudou)
        {
            EditorSceneManager.MarkSceneDirty(atual);
            // Nao salvar automaticamente uma cena com alteracoes do usuario.
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                Debug.Log("Controller do chefao atribuido. Salve a cena para persistir a configuracao.");
        }
    }
}
#endif
