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
            System.Type esperado = TipoEstado(i);
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
    private static System.Type TipoEstado(int indice)
    {
        switch (indice)
        {
            case 0: return typeof(Comportamento01);
            case 1: return typeof(Comportamento02);
            case 2: return typeof(Comportamento03);
            case 3: return typeof(Comportamento04);
            case 4: return typeof(Comportamento05);
            case 5: return typeof(Comportamento06);
            case 6: return typeof(Comportamento07);
            case 7: return typeof(Comportamento08);
            default: return typeof(Comportamento09);
        }
    }

}
#endif
