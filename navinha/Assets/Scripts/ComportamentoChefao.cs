using UnityEngine;

// Cada estado do Animator usa este StateMachineBehaviour.
public class ComportamentoChefao : StateMachineBehaviour
{
    public int tipo;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        ChefaoCombate combate = animator.GetComponent<ChefaoCombate>();
        if (combate != null) combate.Iniciar(tipo);
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        ChefaoCombate combate = animator.GetComponent<ChefaoCombate>();
        if (combate != null) combate.Executar(tipo);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        ChefaoCombate combate = animator.GetComponent<ChefaoCombate>();
        if (combate != null) combate.Encerrar();
    }
}
