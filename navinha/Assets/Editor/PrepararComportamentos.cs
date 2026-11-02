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
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(Caminho);
            var maquina = controller.layers[0].stateMachine;
            for (int i = 0; i < 9; i++)
            {
                var estado = maquina.AddState("Comportamento" + (i + 1), new Vector3((i % 3) * 240, (i / 3) * 100, 0));
                var comportamento = estado.AddStateMachineBehaviour<ComportamentoChefao>();
                comportamento.tipo = i;
                if (i == 0) maquina.defaultState = estado;
            }
            AssetDatabase.SaveAssets();
        }

        Scene atual = SceneManager.GetActiveScene();
        if (atual.path != Cena) return;
        GameObject chefe = GameObject.Find("Chefão") ?? GameObject.Find("Chefao") ?? GameObject.Find("Chefao (alvo provisório)");
        if (chefe == null) return;
        Animator animator = chefe.GetComponent<Animator>();
        if (animator == null) animator = chefe.AddComponent<Animator>();
        bool mudou = animator.runtimeAnimatorController != controller;
        if (mudou) animator.runtimeAnimatorController = controller;
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
