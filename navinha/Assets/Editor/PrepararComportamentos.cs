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
        AnimatorState[] estados = new AnimatorState[9];
        if (!System.Array.Exists(controller.parameters, p => p.name == "FaseVida"))
        {
            controller.AddParameter("FaseVida", AnimatorControllerParameterType.Int);
            estadosAlterados = true;
        }
        if (!System.Array.Exists(controller.parameters, p => p.name == "MudarFase"))
        {
            controller.AddParameter("MudarFase", AnimatorControllerParameterType.Trigger);
            estadosAlterados = true;
        }
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
            estados[i] = estado;
            // Estados sem Motion podem nao ser avaliados continuamente pelo Animator.
            string caminhoClip = Pasta + "/Estado" + (i + 1).ToString("00") + ".anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(caminhoClip);
            if (clip == null)
            {
                clip = new AnimationClip();
                clip.name = "Estado" + (i + 1).ToString("00");
                clip.frameRate = 30f;
                clip.wrapMode = WrapMode.Loop;
                // Curva neutra de 1 s: garante tempo de estado para Exit Time.
                clip.SetCurve("", typeof(SpriteRenderer), "m_Color.a", AnimationCurve.Constant(0f, 1f, 1f));
                AssetDatabase.CreateAsset(clip, caminhoClip);
            }
            if (Mathf.Abs(estado.speed - (1f / 3.5f)) > .0001f)
            {
                estado.speed = 1f / 3.5f;
                estadosAlterados = true;
            }
            if (clip.length < .99f)
            {
                clip.SetCurve("", typeof(SpriteRenderer), "m_Color.a", AnimationCurve.Constant(0f, 1f, 1f));
                EditorUtility.SetDirty(clip);
                estadosAlterados = true;
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
        // Transicoes ciclicas sao configuradas no Animator, nao no script do chefe.
        for (int i = 0; i < 9; i++)
        {
            AnimatorState origem = estados[i];
            AnimatorState destino = estados[(i / 3) * 3 + (i + 1) % 3];
            bool configurado = origem.transitions.Length == 1 &&
                               origem.transitions[0].destinationState == destino &&
                               origem.transitions[0].hasExitTime;
            if (!configurado)
            {
                foreach (var antiga in origem.transitions) origem.RemoveTransition(antiga);
                AnimatorStateTransition transicao = origem.AddTransition(destino);
                transicao.hasExitTime = true;
                transicao.exitTime = 1f;
                transicao.duration = 0f;
                transicao.hasFixedDuration = true;
                estadosAlterados = true;
            }
        }
        bool fasesConfiguradas = maquina.anyStateTransitions.Length == 2;
        if (fasesConfiguradas)
            fasesConfiguradas = maquina.anyStateTransitions[0].destinationState == estados[3] &&
                                maquina.anyStateTransitions[1].destinationState == estados[6];
        if (!fasesConfiguradas)
        {
            foreach (var antiga in maquina.anyStateTransitions) maquina.RemoveAnyStateTransition(antiga);
            for (int fase = 1; fase <= 2; fase++)
            {
                AnimatorStateTransition transicao = maquina.AddAnyStateTransition(estados[fase * 3]);
                transicao.hasExitTime = false;
                transicao.duration = 0f;
                transicao.hasFixedDuration = true;
                transicao.canTransitionToSelf = false;
                transicao.AddCondition(AnimatorConditionMode.If, 0f, "MudarFase");
                transicao.AddCondition(AnimatorConditionMode.Equals, fase, "FaseVida");
            }
            estadosAlterados = true;
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
